using System;
using UnityEngine;
using UnityEngine.UI;

namespace TrickModule.Game
{
    [Serializable]
    public class MenuCanvasService : IMenuService
    {
        public bool Enabled;
        public Camera CanvasCamera;
        public bool DisableMainCamera;
        public float PlaneDistance;

        public bool MatchScreenSize = true;
        public float MatchScreenValue = 1.34f;


        private bool _enableState;
        private Camera _mainCamera;
        private Vector2Int _lastScreenSize;

        public void ExecuteInit(UIMenu menu)
        {
            if (!Enabled) return;

            if (CanvasCamera == null) return;
            CanvasCamera.transform.SetParent(null, true);

            Canvas canvas = menu.Canvas;
            canvas.worldCamera = CanvasCamera;
            canvas.planeDistance = PlaneDistance;

            CanvasCamera.transform.SetParent(menu.transform, true);

            if (MatchScreenSize)
            {
                _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
                var scaler = menu.GetComponent<CanvasScaler>();
                scaler.matchWidthOrHeight = (float)_lastScreenSize.x / _lastScreenSize.y <= MatchScreenValue ? 0.0f : 1.0f;
            }
        }

        public void ExecuteShow(UIMenu menu)
        {
            if (!Enabled) return;

            if (!DisableMainCamera) return;
            _mainCamera = Camera.main;
            _enableState = _mainCamera != null && _mainCamera.enabled;
            if (_enableState && _mainCamera != null) _mainCamera.enabled = false;
        }

        public void ExecuteHide(UIMenu menu)
        {
            if (!Enabled) return;

            if (!DisableMainCamera) return;
            if (_mainCamera != null) _mainCamera.enabled = _enableState;
        }
    }
}