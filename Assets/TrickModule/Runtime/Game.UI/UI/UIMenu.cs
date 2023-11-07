using UnityEngine;

namespace TrickModule.Game
{
    public class UIMenu : MonoBehaviour
    {
        [SerializeField, Header("Animation")] private MenuAnimationService menuAnimationService;
        [SerializeField, Header("Audio")] private MenuAudioService menuAudioService;
        [SerializeField, Header("Camera")] private MenuCanvasService menuCanvasService;
        private UIManager _manager;
        private RectTransform _rt;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private int _initialSortingOrder;

        public enum MenuAction
        {
            None,
            Show,
            Hide
        }

        private bool _isOpen = false;

        public Canvas Canvas => _canvas;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool IsTransitioning { get; private set; } = false;

        public bool IsOpen => menuAnimationService.IsOpen;
        public UIManager Manager => _manager;

        internal void TryShowMenu()
        {
            menuAnimationService.ExecuteShow(this);
        }

        internal void TryHideMenu()
        {
            menuAnimationService.ExecuteHide(this);
        }

        internal void InternalSetManager(UIManager uiManager)
        {
            _manager = uiManager;
        }

        internal void InternalInit()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _rt = GetComponent<RectTransform>();

            _initialSortingOrder = _canvas != null ? _canvas.sortingOrder : 0;

            MenuInitialize();
        }

        internal void InternalStart()
        {
            MenuStart();
        }

        protected virtual void MenuInitialize()
        {
            menuAnimationService.ExecuteInit(this);
            menuAudioService.ExecuteInit(this);
            menuCanvasService.ExecuteInit(this);
        }

        protected virtual void MenuStart()
        {
        }

        public void HideSilent()
        {
            gameObject.SetActive(false);
        }

        public void ShowSilent()
        {
            gameObject.SetActive(true);
        }

        public virtual UIMenu Show()
        {
            return this;
        }

        public virtual void Hide()
        {
        
        }
    }
}