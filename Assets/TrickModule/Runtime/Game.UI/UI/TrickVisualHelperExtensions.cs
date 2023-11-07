﻿using System;
using System.Collections;
using System.Linq;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace TrickModule.Game
{
    public static class TrickVisualHelperExtensions
    {
        public static float DefaultFadeTime { get; set; } = 0.25f;

        public static void SetAlpha(this MonoBehaviour mono, float alpha)
        {
            var rt = mono.transform as RectTransform;
            SetAlpha(rt, alpha);
        }

        public static void SetAlpha(this RectTransform mono, float alpha)
        {
            var tb = mono.GetComponent<TrickVisualHelper>();
            if (tb == null) tb = mono.gameObject.AddComponent<TrickVisualHelper>();
            tb.TryInitializeUI();
            tb.CurrentCanvasGroup.alpha = alpha;
        }

        public static void SetHighlighted(this RectTransform mono, Image highlightBorderImage,
            HighlightState highlightState)
        {
            var trickButton = mono.GetComponent<TrickVisualMono>();
            if (trickButton == null) trickButton = mono.gameObject.AddComponent<TrickVisualMono>();
            trickButton.SetHighlighted(highlightBorderImage, highlightState);
        }

        public static void SetHighlighted(this MonoBehaviour mono, Image highlightBorderImage,
            HighlightState highlightState)
        {
            var trickButton = mono.GetComponent<TrickVisualMono>();
            if (trickButton == null) trickButton = mono.gameObject.AddComponent<TrickVisualMono>();
            if (highlightBorderImage != null) trickButton.HighlightBorderImage = highlightBorderImage;
            if (trickButton.HighlightBorderImage == null)
            {
                var highlightChild = trickButton.transform.Find("HighlightBorder");
                if (highlightChild != null) trickButton.HighlightBorderImage = highlightChild.GetComponent<Image>();
            }

            trickButton.enabled = false;
            trickButton.SetHighlighted(highlightBorderImage, highlightState);
        }

        public static void SetCanvasGroupInteractable(this MonoBehaviour mono, bool interactable, bool blockRaycast)
        {
            var tb = mono.GetComponent<TrickVisualHelper>();
            if (tb == null) tb = mono.gameObject.AddComponent<TrickVisualHelper>();
            tb.TryInitializeUI();
            tb.CurrentCanvasGroup.interactable = interactable;
            tb.CurrentCanvasGroup.blocksRaycasts = blockRaycast;
        }

        public static void SetCanvasGroupInteractable(this RectTransform mono, bool? interactable, bool? blockRaycast)
        {
            var tb = mono.GetComponent<TrickVisualHelper>();
            if (tb == null) tb = mono.gameObject.AddComponent<TrickVisualHelper>();
            tb.TryInitializeUI();
            if (interactable != null) tb.CurrentCanvasGroup.interactable = interactable.Value;
            if (blockRaycast != null) tb.CurrentCanvasGroup.blocksRaycasts = blockRaycast.Value;
        }

        public static Routine Fade(this MonoBehaviour mono, float fadeTarget = 0.0f, float? fadeTime = null,
            Curve curve = Curve.Linear,
            float delay = 0.0f,
            float? setAlpha = null, bool interactable = false, bool withoutHost = false, Action completeAction = null)
        {
            var rt = mono.transform as RectTransform;
            if (rt != null)
                return Fade(rt, fadeTarget, fadeTime, curve, delay, setAlpha, interactable, withoutHost,
                    completeAction);
            return default;
        }

        public static Routine Fade(this RectTransform mono, float fadeTarget = 0.0f, float? fadeTime = null,
            Curve curve = Curve.Linear,
            float delay = 0.0f, float? setAlpha = null, bool interactable = false, bool withoutHost = false,
            Action completeAction = null)
        {
            return Fade(mono, new TweenSettings(fadeTime.GetValueOrDefault(DefaultFadeTime), curve), fadeTarget, delay,
                setAlpha, interactable, withoutHost, completeAction);
        }

        public static Routine Fade(this RectTransform mono, TweenSettings tweenSettings, float fadeTarget = 0.0f,
            float delay = 0.0f, float? setAlpha = null, bool interactable = false, bool withoutHost = false,
            Action completeAction = null)
        {
            var tb = mono.GetComponent<TrickVisualHelper>();
            if (tb == null) tb = mono.gameObject.AddComponent<TrickVisualHelper>();
            tb.TryInitializeUI();
            if (setAlpha != null) tb.CurrentCanvasGroup.alpha = setAlpha.GetValueOrDefault();
            var tween = tb.CurrentCanvasGroup.FadeTo(fadeTarget, tweenSettings).DelayBy(delay)
                .OnComplete(completeAction);
            tb.FadeRoutine.Replace(withoutHost ? tween.Play() : tween.Play(tb));
            tb.SetCanvasGroupInteractable(interactable, interactable);
            return tb.FadeRoutine;
        }

        public static Routine FadeIn(this MonoBehaviour mono, float? fadeTime = null, Curve curve = Curve.Linear,
            float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, 1.0f, fadeTime, curve, delay, setAlpha, true, withoutHost, completeAction);
        }

        public static Routine FadeOut(this MonoBehaviour mono, float? fadeTime = null, Curve curve = Curve.Linear,
            float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, 0.0f, fadeTime, curve, delay, setAlpha, false, withoutHost, completeAction);
        }

        public static Routine FadeIn(this RectTransform mono, float? fadeTime = null, Curve curve = Curve.Linear,
            float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, 1.0f, fadeTime, curve, delay, setAlpha, true, withoutHost, completeAction);
        }

        public static Routine FadeOut(this RectTransform mono, float? fadeTime = null, Curve curve = Curve.Linear,
            float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, 0.0f, fadeTime, curve, delay, setAlpha, false, withoutHost, completeAction);
        }


        public static Routine FadeIn(this MonoBehaviour mono, TweenSettings tweenSettings, float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, 1.0f, tweenSettings, delay, setAlpha, true, withoutHost, completeAction);
        }

        private static Routine Fade(MonoBehaviour mono, float fadeTarget, TweenSettings tweenSettings, float delay,
            float? setAlpha, bool interactable, bool withoutHost, Action completeAction)
        {
            var rt = mono.transform as RectTransform;
            if (rt != null)
                return Fade(rt, tweenSettings, fadeTarget, delay, setAlpha, interactable, withoutHost, completeAction);
            return default;
        }

        public static Routine FadeOut(this MonoBehaviour mono, TweenSettings tweenSettings, float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, 0.0f, tweenSettings, delay, setAlpha, false, withoutHost, completeAction);
        }

        public static Routine FadeIn(this RectTransform mono, TweenSettings tweenSettings, float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, tweenSettings, 1.0f, delay, setAlpha, true, withoutHost, completeAction);
        }

        public static Routine FadeOut(this RectTransform mono, TweenSettings tweenSettings, float delay = 0.0f,
            float? setAlpha = null, bool withoutHost = false, Action completeAction = null)
        {
            return Fade(mono, tweenSettings, 0.0f, delay, setAlpha, false, withoutHost, completeAction);
        }


        public static Routine ScaleTransformPingPong(this MonoBehaviour mono, float scaleTarget,
            float scaleDuration = 0.35f,
            float delay = 0.0f, Action startCallback = null) => ScaleTransformPingPong(mono.transform as RectTransform,
            scaleTarget, scaleDuration, delay, startCallback);

        public static Routine ScaleTransformPingPong(this RectTransform mono, float scaleTarget,
            float scaleDuration = 0.35f,
            float delay = 0.0f, Action startCallback = null)
        {
            var tb = mono.GetComponent<TrickVisualHelper>();
            if (tb == null) tb = mono.gameObject.AddComponent<TrickVisualHelper>();
            tb.TryInitializeUI();
            tb.LocalScale ??= mono.localScale;
            tb.LocalScale = tb.LocalScale.Value;

            return tb.ScaleRoutine.Replace(mono.ScaleTo(tb.LocalScale.Value * scaleTarget, new TweenSettings()
                {
                    Time = scaleDuration,
                    Curve = Curve.Smooth
                }).DelayBy(delay).OnStart(startCallback).Yoyo()
                .OnComplete(() => { mono.localScale = tb.LocalScale.Value; }).Play(tb));
        }

        public static void ResetParticleSystemColor(this ParticleSystem particleSystem, Color color,
            bool injectChildSystems)
        {
            if (particleSystem == null) return;
            var visualHelper = particleSystem.GetComponent<TrickVisualHelper>();
            if (visualHelper == null) visualHelper = particleSystem.gameObject.AddComponent<TrickVisualHelper>();
            visualHelper.TryInitializePS();

            void ResetParticleColor(ParticleSystem ps)
            {
                if (visualHelper.InitMainModuleStartColor != null)
                {
                    if (visualHelper.particleSystemInjectStartColor)
                    {
                        var mainModule = ps.main;
                        mainModule.startColor = visualHelper.InitMainModuleStartColor.Value;
                    }

                    if (visualHelper.particleSystemInjectColorOverTime)
                    {
                        var colorOverTimeModule = ps.colorOverLifetime;
                        if (colorOverTimeModule.enabled && visualHelper.InitColorOverTimeColor != null)
                            colorOverTimeModule.color = visualHelper.InitColorOverTimeColor.Value;
                    }

                    if (visualHelper.particleSystemInjectColorBySpeed)
                    {
                        var colorBySpeedModule = ps.colorBySpeed;
                        if (colorBySpeedModule.enabled && visualHelper.InitColorBySpeed != null)
                            colorBySpeedModule.color = visualHelper.InitColorBySpeed.Value;
                    }
                }
            }

            if (injectChildSystems)
            {
                foreach (var ps in particleSystem.GetComponentsInChildren<ParticleSystem>())
                    SetParticleSystemColor(ps, color, false);
            }
            else
            {
                ResetParticleColor(particleSystem);
            }
        }

        public static void SetParticleSystemColor(this ParticleSystem particleSystem, Color color,
            bool injectChildSystems)
        {
            if (particleSystem == null) return;
            var visualHelper = particleSystem.GetComponent<TrickVisualHelper>();
            if (visualHelper == null) visualHelper = particleSystem.gameObject.AddComponent<TrickVisualHelper>();
            visualHelper.TryInitializePS();

            ParticleSystem.MinMaxGradient GetProcessedMinMaxGradient(ParticleSystem.MinMaxGradient minMaxGradient)
            {
                var newGradient = new ParticleSystem.MinMaxGradient()
                {
                    mode = minMaxGradient.mode,
                    colorMin = minMaxGradient.colorMin,
                    colorMax = minMaxGradient.colorMax,
                    gradientMin = minMaxGradient.gradientMin != null
                        ? new Gradient()
                        {
                            mode = minMaxGradient.gradientMin.mode,
                            alphaKeys = minMaxGradient.gradientMin.alphaKeys.ToArray(),
                            colorKeys = minMaxGradient.gradientMin.colorKeys.ToArray(),
                        }
                        : minMaxGradient.gradientMin,
                    gradientMax = minMaxGradient.gradientMax != null
                        ? new Gradient()
                        {
                            mode = minMaxGradient.gradientMax.mode,
                            alphaKeys = minMaxGradient.gradientMax.alphaKeys.ToArray(),
                            colorKeys = minMaxGradient.gradientMax.colorKeys.ToArray(),
                        }
                        : minMaxGradient.gradientMax,
                };
                switch (newGradient.mode)
                {
                    case ParticleSystemGradientMode.Color:
                        newGradient.color = color;
                        break;
                    case ParticleSystemGradientMode.Gradient:
                    {
                        var arr = newGradient.gradient.colorKeys.ToArray();
                        for (int i = 0; i < arr.Length; i++)
                        {
                            var temp = arr[i];
                            temp.color = color;
                            arr[i] = temp;
                        }

                        newGradient.gradient.colorKeys = arr;
                        break;
                    }
                    case ParticleSystemGradientMode.TwoColors:
                        newGradient.colorMin = color;
                        newGradient.colorMax = color;
                        break;
                    case ParticleSystemGradientMode.TwoGradients:
                    {
                        if (newGradient.gradientMin != null)
                        {
                            var minArr = newGradient.gradientMin.colorKeys.ToArray();
                            for (int i = 0; i < minArr.Length; i++)
                            {
                                var temp = minArr[i];
                                temp.color = color;
                                minArr[i] = temp;
                            }

                            newGradient.gradientMin.colorKeys = minArr;
                        }

                        if (newGradient.gradientMax != null)
                        {
                            var maxArr = newGradient.gradientMax.colorKeys.ToArray();
                            for (int i = 0; i < maxArr.Length; i++)
                            {
                                var temp = maxArr[i];
                                temp.color = color;
                                maxArr[i] = temp;
                            }

                            newGradient.gradientMax.colorKeys = maxArr;
                        }

                        break;
                    }
                    case ParticleSystemGradientMode.RandomColor:
                        newGradient.color = UnityEngine.Random.ColorHSV();
                        break;
                }

                return newGradient;
            }

            void SetParticleColor(ParticleSystem ps)
            {
                if (visualHelper.particleSystemInjectStartColor)
                {
                    var mainModule = ps.main;
                    visualHelper.InitMainModuleStartColor ??= mainModule.startColor;

                    var minMaxGradient = visualHelper.InitMainModuleStartColor.Value;
                    mainModule.startColor = GetProcessedMinMaxGradient(minMaxGradient);
                }

                if (visualHelper.particleSystemInjectColorOverTime)
                {
                    var colorOverTimeModule = ps.colorOverLifetime;
                    if (colorOverTimeModule.enabled)
                    {
                        visualHelper.InitColorOverTimeColor ??= colorOverTimeModule.color;
                        colorOverTimeModule.color =
                            GetProcessedMinMaxGradient(visualHelper.InitColorOverTimeColor.Value);
                    }
                }

                if (visualHelper.particleSystemInjectColorBySpeed)
                {
                    var colorBySpeedModule = ps.colorBySpeed;
                    if (colorBySpeedModule.enabled)
                    {
                        visualHelper.InitColorBySpeed ??= colorBySpeedModule.color;
                        colorBySpeedModule.color = GetProcessedMinMaxGradient(visualHelper.InitColorBySpeed.Value);
                    }
                }
            }

            if (injectChildSystems)
            {
                foreach (var ps in particleSystem.GetComponentsInChildren<ParticleSystem>())
                    SetParticleSystemColor(ps, color, false);
            }
            else
            {
                SetParticleColor(particleSystem);
            }
        }


        /// <summary>
        /// Shake the object by rotating it
        /// </summary>
        /// <param name="mono">The object to shake</param>
        /// <param name="b">Enabled or not</param>
        /// <param name="shake">The amount of shakiness</param>
        /// <param name="pauseDuration">The pause duration between a shake</param>
        /// <param name="longPauseDuration">The long pause duration for a shake</param>
        /// <param name="tweenDuration">The shake tweenSettings</param>
        /// <param name="tweenCurve">The shake tweenSettings</param>
        /// <param name="loops">Amount of loops before we complete. -1 is for continuous</param>
        public static void Shake(this MonoBehaviour mono, bool b, float shake = 20.0f, float pauseDuration = 0.05f,
            float longPauseDuration = 0.85f, float tweenDuration = 0.35f, Curve tweenCurve = Curve.BounceInOut,
            int loops = -1)
        {
            Shake(mono, b, shake, pauseDuration, longPauseDuration, new TweenSettings(tweenDuration, tweenCurve),
                loops);
        }

        /// <summary>
        /// Shake the object by rotating it
        /// </summary>
        /// <param name="mono">The object to shake</param>
        /// <param name="b">Enabled or not</param>
        /// <param name="shake">The amount of shakiness</param>
        /// <param name="pauseDuration">The pause duration between a shake</param>
        /// <param name="longPauseDuration">The long pause duration for a shake</param>
        /// <param name="tweenSettings">The shake tweenSettings</param>
        /// <param name="loops">Amount of loops before we complete. -1 is for continuous</param>
        public static void Shake(this MonoBehaviour mono, bool b, float shake = 20.0f, float pauseDuration = 0.05f,
            float longPauseDuration = 0.85f, TweenSettings tweenSettings = default, int loops = -1)
        {
            IEnumerator HandleShake()
            {
                var tr = mono.transform;
                var go = mono.gameObject;
                int i = 0;
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (go != null)
                {
                    yield return Routine.WaitCondition(() => tr == null || go.activeSelf, 0.1f);
                    if (tr == null) yield break;
                    yield return tr.RotateTo(Vector3.one * shake, tweenSettings, Axis.Z);
                    yield return Routine.WaitSeconds(pauseDuration);
                    if (tr == null) yield break;
                    yield return tr.RotateTo(Vector3.one * -shake, tweenSettings,
                        Axis.Z);

                    if (i % 3 == 0)
                    {
                        yield return tr.RotateTo(Vector3.zero, tweenSettings, Axis.Z);
                        if (loops != -1)
                        {
                            loops--;
                            if (loops == 0) yield break;
                        }

                        yield return Routine.WaitSeconds(longPauseDuration);
                    }

                    i++;
                }
            }

            var visualHelper = mono.GetComponent<TrickVisualHelper>();
            if (visualHelper == null) visualHelper = mono.gameObject.AddComponent<TrickVisualHelper>();
            visualHelper.TryInitializeUI();

            if (b)
            {
                visualHelper.ShakeRoutine =
                    visualHelper.ShakeRoutine.Replace(HandleShake()).SetPhase(RoutinePhase.Update);
            }
            else
            {
                visualHelper.transform.SetRotation(Vector3.zero, Axis.Z);
                visualHelper.ShakeRoutine.Stop();
            }
        }

        public static Routine TransitionIn(this RectTransform registerRoot, TweenSettings tweenSettings,
            MenuTransitionDirection direction, float delay, Action completeCallback = null)
        {
            if (registerRoot == null) return Routine.Null;
            var visualHelper = registerRoot.GetComponent<TrickVisualHelper>();
            if (visualHelper == null) visualHelper = registerRoot.gameObject.AddComponent<TrickVisualHelper>();
            visualHelper.TryInitializeUI();

            var rt = registerRoot;
            var siz = rt.rect.size;
            var anchor = rt.anchoredPosition;
            visualHelper.OriginalAnchorPosition ??= anchor;
            switch (direction)
            {
                case MenuTransitionDirection.None:
                    completeCallback?.Invoke();
                    break;
                case MenuTransitionDirection.Left:
                    rt.anchoredPosition =
                        new Vector2(-siz.x * 2 * rt.pivot.x, visualHelper.OriginalAnchorPosition.Value.y);
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(visualHelper.OriginalAnchorPosition.Value, tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play().OnStop(completeCallback).OnException(Debug.LogException));
                    break;
                case MenuTransitionDirection.Top:
                    rt.anchoredPosition =
                        new Vector2(visualHelper.OriginalAnchorPosition.Value.x, siz.y * 2 * rt.pivot.y);
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(visualHelper.OriginalAnchorPosition.Value, tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play().OnStop(completeCallback).OnException(Debug.LogException));
                    break;
                case MenuTransitionDirection.Right:
                    rt.anchoredPosition =
                        new Vector2(siz.x * 2 * rt.pivot.x, visualHelper.OriginalAnchorPosition.Value.y);
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(visualHelper.OriginalAnchorPosition.Value, tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play().OnStop(completeCallback).OnException(Debug.LogException));
                    break;
                case MenuTransitionDirection.Bottom:
                    rt.anchoredPosition =
                        new Vector2(visualHelper.OriginalAnchorPosition.Value.x, -siz.y * 2 * rt.pivot.y);
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(visualHelper.OriginalAnchorPosition.Value, tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play().OnStop(completeCallback).OnException(Debug.LogException));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return visualHelper.TransitionRoutine;
        }

        public static Routine TransitionIn(this MonoBehaviour registerRoot, TweenSettings tweenSettings,
            MenuTransitionDirection direction, float delay, Action completeCallback = null)
        {
            return TransitionIn((RectTransform)registerRoot.transform, tweenSettings, direction, delay,
                completeCallback);
        }

        public static Routine TransitionOut(this RectTransform registerRoot, TweenSettings tweenSettings,
            MenuTransitionDirection direction, float delay, Action completeCallback = null)
        {
            if (registerRoot == null) return Routine.Null;

            var visualHelper = registerRoot.GetComponent<TrickVisualHelper>();
            if (visualHelper == null) visualHelper = registerRoot.gameObject.AddComponent<TrickVisualHelper>();
            visualHelper.TryInitializeUI();

            var rt = registerRoot;
            var siz = rt.rect.size;
            var anchor = rt.anchoredPosition;
            visualHelper.OriginalAnchorPosition ??= anchor;
            switch (direction)
            {
                case MenuTransitionDirection.None:
                    completeCallback?.Invoke();
                    break;
                case MenuTransitionDirection.Left:
                    rt.anchoredPosition = visualHelper.OriginalAnchorPosition.GetValueOrDefault();
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(new Vector2(-siz.x * 2 * rt.pivot.x, visualHelper.OriginalAnchorPosition.Value.y),
                            tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play());
                    break;
                case MenuTransitionDirection.Top:
                    rt.anchoredPosition = visualHelper.OriginalAnchorPosition.GetValueOrDefault();
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(new Vector2(visualHelper.OriginalAnchorPosition.Value.x, siz.y * 2 * rt.pivot.y),
                            tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play());
                    break;
                case MenuTransitionDirection.Right:
                    rt.anchoredPosition = visualHelper.OriginalAnchorPosition.GetValueOrDefault();
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(new Vector2(siz.x * 2 * rt.pivot.x, visualHelper.OriginalAnchorPosition.Value.y),
                            tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play());
                    break;
                case MenuTransitionDirection.Bottom:
                    rt.anchoredPosition = visualHelper.OriginalAnchorPosition.GetValueOrDefault();
                    visualHelper.TransitionRoutine.Replace(rt
                        .AnchorPosTo(new Vector2(visualHelper.OriginalAnchorPosition.Value.x, -siz.y * 2 * rt.pivot.y),
                            tweenSettings)
                        .OnComplete(completeCallback).DelayBy(delay).Play());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return visualHelper.TransitionRoutine;
        }

        public static Routine TransitionOut(this MonoBehaviour registerRoot, TweenSettings tweenSettings,
            MenuTransitionDirection direction, float delay, Action completeCallback = null)
        {
            return TransitionOut((RectTransform)registerRoot.transform, tweenSettings, direction, delay,
                completeCallback);
        }
    }
}