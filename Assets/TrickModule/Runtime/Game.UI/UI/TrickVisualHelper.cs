using BeauRoutine;
using UnityEngine;

namespace TrickModule.Game
{
    public class TrickVisualHelper : MonoBehaviour
    {
        public bool particleSystemInjectStartColor = true;
        public bool particleSystemInjectColorOverTime = true;
        public bool particleSystemInjectColorBySpeed = true;

        internal CanvasGroup CurrentCanvasGroup;

        // ps
        private bool _initializeForPS;
        internal ParticleSystem.MinMaxGradient? InitMainModuleStartColor;
        internal ParticleSystem.MinMaxGradient? InitColorOverTimeColor;
        internal ParticleSystem.MinMaxGradient? InitColorBySpeed;

        // ui
        internal bool InitializeForUI;
        internal Routine FadeRoutine;
        internal Routine TransitionRoutine;
        internal Routine ScaleRoutine;
        internal Routine ShakeRoutine;
        internal Vector3? LocalScale;
        internal Vector2? OriginalAnchorPosition;

        internal void TryInitializePS()
        {
            if (_initializeForPS) return;

            _initializeForPS = true;
        }

        internal void TryInitializeUI()
        {
            if (InitializeForUI) return;
            CurrentCanvasGroup = GetComponent<CanvasGroup>();
            if (CurrentCanvasGroup == null) CurrentCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            InitializeForUI = true;
        }
    }
}