using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Proyecto26;
using TrickModule.Core;
using TrickModule.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TrickModule.Game
{
    public class AssetManager : MonoSingleton<AssetManager>
    {
        [Header("Storage - Sprite")]
        [SerializeField] private SaveStrategy _spriteSaveStrategy = SaveStrategy.CacheAndLocal;
        [SerializeField] private string _remoteSpritePath;
        [SerializeField] private string _remotePathSpriteExtension = ".png";
        
        [Header("Storage - Audio")]
        [SerializeField] private SaveStrategy _audioSaveStrategy = SaveStrategy.CacheAndLocal;
        [SerializeField] private string _remoteAudioPath;
        [SerializeField] private string _remotePathAudioExtension = ".mp3";
        
        [Header("Storage - String")]
        [SerializeField] private SaveStrategy _stringSaveStrategy = SaveStrategy.CacheAndLocal;
        [SerializeField] private string _remoteStringPath;
        [SerializeField] private string _remotePathStringExtension = ".text";

        private IStorage<Sprite> _spriteStorage;
        private IStorage<AudioClip> _audioStorage;
        private IStorage<string> _stringStorage;

        public UniTask<Sprite> LoadSprite(string path) => _spriteStorage.Load(path);
        public UniTask<AudioClip> LoadAudio(string path) => _audioStorage.Load(path);
        public UniTask<string> LoadString(string path) => _stringStorage.Load(path);

        public UniTask SaveSprite(string path, Sprite sprite) => _spriteStorage.Save(path, sprite);
        public UniTask SaveAudio(string path, AudioClip audio) => _audioStorage.Save(path, audio);
        public UniTask SaveString(string path, string data) => _stringStorage.Save(path, data);

        public void RemoveAsset(string path)
        {
            _spriteStorage.RemoveAsset(path);
            _audioStorage.RemoveAsset(path);
            _stringStorage.RemoveAsset(path);
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            _spriteStorage = new LocalStorage<Sprite>()
            {
                SaveStrategy = _spriteSaveStrategy,
                RemotePathResolver = (s, arg3) => ResolvePath(s, _remoteSpritePath, _remotePathSpriteExtension),
            };
            
            _audioStorage = new LocalStorage<AudioClip>()
            {
                SaveStrategy = _audioSaveStrategy, 
                RemotePathResolver = (s, arg3) => ResolvePath(s, _remoteAudioPath, _remotePathAudioExtension),
            };
            
            _stringStorage = new LocalStorage<string>()
            {
                SaveStrategy = _stringSaveStrategy, 
                RemotePathResolver = (s, arg3) => ResolvePath(s, _remoteStringPath, _remotePathStringExtension),
            };
        }

        private string ResolvePath(string s, string remotePath, string extension)
        {
            if (s.StartsWith("http://") || s.StartsWith("https://"))
                return s;
            if (s.StartsWith("res://") || s.StartsWith("resource://"))
                return s;
            
            return Path.Combine(remotePath, $"{s}{extension}");
        }
    }

    [Serializable]
    public enum SaveStrategy
    {
        None,
        CacheOnly,
        CacheAndLocal,
    }

    public static class AssetManagerExtensions
    {
        public static void SetImage(this SpriteRenderer renderer, string path, Sprite defaultSprite = null, Action<Sprite> callback = null)
        {
            AssetManager.Instance.LoadSprite(path).ContinueWith(sprite =>
            {
                if (sprite == null) sprite = defaultSprite;
                renderer.sprite = sprite;
                
                callback?.Invoke(sprite);
            }).Forget(Logger.Logger.Game.LogException);
        }
        public static void SetSprite(this SpriteRenderer renderer, string path, Sprite defaultSprite = null, Action<Sprite> callback = null) => SetImage(renderer, path, defaultSprite, callback);
        
        public static void SetImage(this Image renderer, string path, Sprite defaultSprite = null, Action<Sprite> callback = null)
        {
            AssetManager.Instance.LoadSprite(path).ContinueWith(sprite =>
            {
                if (sprite == null) sprite = defaultSprite;
                renderer.sprite = sprite;
                
                callback?.Invoke(sprite);
            }).Forget(Logger.Logger.Game.LogException);
        }
        public static void SetSprite(this Image renderer, string path, Sprite defaultSprite = null, Action<Sprite> callback = null) => SetImage(renderer, path, defaultSprite, callback);

        public static void SetAudio(this AudioSource source, string path, Action<AudioClip> callback = null)
        {
            AssetManager.Instance.LoadAudio(path).ContinueWith(clip =>
            {
                source.clip = clip;
                source.Play();
                
                callback?.Invoke(clip);
            }).Forget(Logger.Logger.Game.LogException);
        }
    }

    public interface IStorage<T> where T : class
    {
        UniTask<T> Load(string path);
        UniTask Save(string path, T data, byte[] bytes = null);
        
        void RemoveAsset(string path);
    }

    public class LocalStorage<T> : IStorage<T> where T : class
    {
        public SaveStrategy SaveStrategy = SaveStrategy.CacheOnly;
        private readonly Dictionary<string, T> _cache = new();
        public Func<string, Type, string> RemotePathResolver { get; set; }

        public async UniTask<T> Load(string path)
        {
            // Check if the file is in the cache
            if (_cache.TryGetValue(path, out var value)) return value;

            // Look inside player prefs
            if (PlayerPrefs.HasKey(path))
            {
                var asset = await ResolveAssetFromData(path, PlayerPrefs.GetString(path));
                await Save(path, asset, null);
                return asset;
            }

            // If platform is not webgl, look inside the persistent data path
#if !UNITY_WEBGL
            var fileName = Path.GetFileName(RemotePathResolver?.Invoke(path, typeof(T)));
            var persistentDataPath = $"{Application.persistentDataPath}/{fileName}";

            // use the file if it exists
            if (File.Exists(persistentDataPath))
            {
                var data = await File.ReadAllBytesAsync(persistentDataPath);
                var asset = await ResolveAssetFromData(path, data);
                await Save(path, asset, null);
                return asset;
            }
#endif

            // Look inside the resources folder if the path starts with res:// or resource://
            if (path.StartsWith("res://") || path.StartsWith("resource://"))
            {
                var resourcePath = path
                    .Replace("res://", "")
                    .Replace("resource://", "");

                // check if T inherits from UnityEngine.Object
                if (typeof(T).IsSubclassOf(typeof(Object)))
                {
                    var resource = Resources.Load(resourcePath);
                    await Save(path, resource as T, null);
                    return resource as T;
                }
            }
            
            // Last resort, try to download the file from the server
            var remoteAssetPath = ResolvePath(path);
            var tcs = new UniTaskCompletionSource<(T obj, byte[] downloadBytes)>();
            return await EnqueueDownload(remoteAssetPath, path, tcs);
        }

        private readonly Dictionary<string, UniTaskCompletionSource<(T obj, byte[] downloadBytes)>> _downloadQueue = new();

        private async UniTask<T> EnqueueDownload(string remoteAssetPath, string path, UniTaskCompletionSource<(T obj, byte[] downloadBytes)> tcs)
        {
            if (_downloadQueue.TryGetValue(remoteAssetPath, out var existingTcs))
            {
                var resolved = await existingTcs.Task;
                Save(path, resolved.obj, resolved.downloadBytes).Forget(Logger.Logger.Game.LogException);
                return (await existingTcs.Task).obj;
            }

            _downloadQueue[remoteAssetPath] = tcs;
            RestClient.Get(remoteAssetPath).Then(response =>
            {
                if (response.StatusCode != 200)
                {
                    tcs.TrySetResult((null, null));
                    return;
                }
                Logger.Logger.Game.LogInfo($"Downloaded external asset: {remoteAssetPath} (size={response.Request.downloadHandler.data.Length})");
                var downloadBytes = response.Request.downloadHandler.data;
                ResolveAssetFromData(path, downloadBytes).ContinueWith(obj =>
                {
                    Save(path, obj, downloadBytes).Forget(Logger.Logger.Game.LogException);
                    tcs.TrySetResult((obj, downloadBytes));
                }).Forget(Logger.Logger.Game.LogException);
            }).Catch(Logger.Logger.Game.LogException);
                
            return (await tcs.Task).obj;
        }

        private string ResolvePath(string path)
        {
            return RemotePathResolver != null ? RemotePathResolver(path, typeof(T)) : path;
        }

        private async UniTask<T> ResolveAssetFromData(string path, object data)
        {
            if (data == null) return null;

            switch (data)
            {
                case string str:
                {
                    if (typeof(T) == typeof(Sprite))
                    {
                        // Convert the string (imageData) to a sprite
                        var imageData = Convert.FromBase64String(str);
                        var sprite = CreateSprite(imageData);
                        return sprite as T;
                    }
                    else if (typeof(T) == typeof(AudioClip))
                    {
                        // Convert the string (imageData) to a sprite
                        var audioData = Convert.FromBase64String(str);
                        var clip = await CreateAudioClip(path, audioData);
                        return clip as T;
                    }
                
                    var jsonData = str.DeserializeJsonBase64TryCatch<T>();
                    return jsonData;
                }
                case byte[] bytes when typeof(T) == typeof(Sprite):
                {
                    // Convert the string (imageData) to a sprite
                    var sprite = CreateSprite(bytes);
                    return sprite as T;
                }
                case byte[] bytes when typeof(T) == typeof(AudioClip):
                {
                    // Convert the bytes to a clip
                    var clip = await CreateAudioClip(path, bytes);
                    return clip as T;
                }
                case byte[] bytes:
                {
                    var base64String = Convert.ToBase64String(bytes);
                    var jsonData = base64String.DeserializeJsonBase64TryCatch<T>();
                    return jsonData;
                }
            }

            return null;

            Sprite CreateSprite(byte[] imageData)
            {
                // Load the image from the base64 string to a Texture2D
                var cachedTexture2D = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                cachedTexture2D.LoadImage(imageData, false);
                // Convert the Texture2D to a Sprite
                var sprite = Sprite.Create(cachedTexture2D,
                    new Rect(0, 0, cachedTexture2D.width, cachedTexture2D.height), Vector2.zero);
                return sprite;
            }
        }

        private async UniTask<AudioClip> CreateAudioClip(string path, byte[] audioData)
        {
            var fileName = Path.GetFileName(RemotePathResolver?.Invoke(path, typeof(T)));
            path = Path.Combine(Application.persistentDataPath, fileName ?? throw new InvalidOperationException());
            
            var audioTypeByExtension = new Regex(@"\.(\w+)$").Match(path).Groups[1].Value switch
            {
                "ogg" => AudioType.OGGVORBIS,
                "mp3" => AudioType.MPEG,
                "wav" => AudioType.WAV,
                _ => AudioType.MPEG,
            };
        
            // Save the file locally
            if (!File.Exists(path)) await File.WriteAllBytesAsync(path, audioData);
            
            UniTaskCompletionSource<AudioClip> ucs = new UniTaskCompletionSource<AudioClip>();
        
            // Run UnityWebRequest using Unity's coroutine context, but await its completion with UniTask
            await UniTask.SwitchToMainThread();
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioTypeByExtension))
            {
                UnityWebRequestAsyncOperation request = www.SendWebRequest();
                await request;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    ucs.TrySetResult(audioClip);
                }
                else
                {
                    ucs.TrySetException(new System.Exception("Failed to load AudioClip: " + www.error));
                }
            }

            return await ucs.Task;
        }

        public UniTask Save(string path, T data, byte[] bytes = null)
        {
            switch (SaveStrategy)
            {
                case SaveStrategy.None:
                    break;
                case SaveStrategy.CacheOnly:
                    _cache[path] = data;
                    break;
                case SaveStrategy.CacheAndLocal:
                    _cache[path] = data;
                    if (bytes != null)
                    {
                        var fileName = Path.GetFileName(RemotePathResolver?.Invoke(path, typeof(T)));
                        var persistentDataPath = $"{Application.persistentDataPath}/{fileName}";
                        
                        File.WriteAllBytes(persistentDataPath, bytes);
                    }
                    break;
            }

            // Save the file locally
            return UniTask.CompletedTask;
        }

        public void RemoveAsset(string path)
        {
            _cache.Remove(path);
            PlayerPrefs.DeleteKey(path);
            
            var fileName = Path.GetFileName(RemotePathResolver?.Invoke(path, typeof(T)));
            var persistentDataPath = $"{Application.persistentDataPath}/{fileName}";
            if (File.Exists(persistentDataPath))
                File.Delete(persistentDataPath);
            
            var remoteAssetPath = ResolvePath(path);
            _downloadQueue.Remove(remoteAssetPath);
        }
    }
}