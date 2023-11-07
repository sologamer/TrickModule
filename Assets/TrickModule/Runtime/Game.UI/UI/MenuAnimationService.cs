using System;
using System.Collections.Generic;
using System.Threading;
using BeauRoutine;
using UnityEngine;

namespace TrickModule.Game
{
    [Serializable]
    public class MenuAnimationService : IMenuService
    {
        [Header("Transitions")] [SerializeField] private List<TransitionGroup> transitions = new List<TransitionGroup>();

        [Header("Fade")] [SerializeField] private MenuFadeEnableType menuFading;
        [SerializeField] private TweenSettings showFadeInSettings;
        [SerializeField] private TweenSettings hideFadeOutSettings;

        private UIMenu _menu;
        private bool _isOpen;
        public bool IsTransitioning { get; private set; } = false;

        private readonly Queue<UIMenu.MenuAction> _actionQueue = new Queue<UIMenu.MenuAction>();
        private UIMenu.MenuAction? _lastEnqueuedAction = null;
        private CancellationTokenSource _showTokenSource;
        private CancellationTokenSource _hideTokenSource;
        private List<Routine> _transitionRoutines = new List<Routine>();
        private int _numFadeOuts;
        private Routine _fadeRoutine;

        public float LastShowTime { get; set; }
        public bool IsOpen => _isOpen;

        public void ExecuteInit(UIMenu menu)
        {
            _menu = menu;
        }

        public void ExecuteShow(UIMenu menu)
        {
            EnqueueAction(UIMenu.MenuAction.Show);
        }

        public void ExecuteHide(UIMenu menu)
        {
            EnqueueAction(UIMenu.MenuAction.Hide);
        }

        private void EnqueueAction(UIMenu.MenuAction action)
        {
            if (_lastEnqueuedAction == action)
                return;

            _actionQueue.Enqueue(action);
            _lastEnqueuedAction = action;
            ProcessNextAction();
        }

        private void ProcessNextAction()
        {
            while (true)
            {
                if (IsTransitioning || _actionQueue.Count == 0) return;

                UIMenu.MenuAction nextAction = _actionQueue.Dequeue();

                if (nextAction == UIMenu.MenuAction.Show && !_isOpen)
                    ActualShow();
                else if (nextAction == UIMenu.MenuAction.Hide && _isOpen)
                    ActualHide();
                else
                    continue;
                break;
            }
        }

        private void StopOngoingTransitions()
        {
            _transitionRoutines.ForEach(routine => routine.Stop());
            _transitionRoutines.Clear();
        }

        private void ActualShow()
        {
            if (_isOpen) return;

            _showTokenSource = new CancellationTokenSource();
            _menu.gameObject.SetActive(true);
            _hideTokenSource?.Cancel();
            StopOngoingTransitions();
            _isOpen = true;

            IsTransitioning = true;

            _menu.Manager.PreMenuShowEvent?.Invoke(_menu);

            if (transitions != null && transitions.Count > 0)
            {
                int completed = 0;
                foreach (var transition in transitions)
                {
                    if (transition.TransitionPanelTransform == null)
                    {
                        if (++completed == transitions.Count) InternalShow();
                        continue;
                    }

                    transition.TransitionPanelTransform.SetCanvasGroupInteractable(null, false);

                    _transitionRoutines.Add(transition.TransitionPanelTransform.TransitionIn(transition.TransitionInSettings, transition.TransitionDirectionIn, transition.Delay, () =>
                    {
                        if (_showTokenSource.IsCancellationRequested) return;

                        transition.TransitionPanelTransform.SetCanvasGroupInteractable(null, true);

                        if (++completed == transitions.Count) InternalShow();
                    }));

                    if (transition.PanelFading.HasFlag(MenuFadeEnableType.In))
                        transition.TransitionPanelTransform.FadeIn(transition.FadeInSettings, delay: transition.Delay);
                }
            }
            else
            {
                InternalShow();
            }

            if (menuFading.HasFlag(MenuFadeEnableType.In))
                FadeIn(showFadeInSettings);

            return;

            void InternalShow()
            {
                LastShowTime = Time.realtimeSinceStartup;

                if (_menu.Manager.MenuDebug != MenuDebugMode.Off) Debug.Log($"[UIMenu] SHOW {_menu}");

                _menu.Manager.PostMenuShowEvent?.Invoke(_menu);
                _menu.Show();
                IsTransitioning = false;
                ProcessNextAction();
            }
        }

        /// <summary>
        /// Hides the current menu
        /// </summary>
        private void ActualHide()
        {
            if (!_isOpen) return;

            _hideTokenSource = new CancellationTokenSource();
            _showTokenSource?.Cancel();
            StopOngoingTransitions();
            _menu.Manager.PreMenuHideEvent?.Invoke(_menu);
            _isOpen = false;

            if (transitions != null && transitions.Count > 0)
            {
                int completed = 0;
                foreach (var transition in transitions)
                {
                    if (transition.TransitionPanelTransform == null)
                    {
                        if (++completed == transitions.Count) InternalHide();
                        continue;
                    }

                    transition.TransitionPanelTransform.SetCanvasGroupInteractable(null, false);

                    _transitionRoutines.Add(transition.TransitionPanelTransform.TransitionOut(transition.TransitionOutSettings, transition.TransitionDirectionOut, transition.Delay, () =>
                    {
                        if (_hideTokenSource.IsCancellationRequested) return;

                        if (++completed == transitions.Count) InternalHide();
                        transition.TransitionPanelTransform.SetCanvasGroupInteractable(null, true);
                    }));

                    if (transition.PanelFading.HasFlag(MenuFadeEnableType.Out))
                    {
                        transition.TransitionPanelTransform.FadeOut(transition.FadeOutSettings, delay: transition.Delay);
                    }
                }

                if (menuFading.HasFlag(MenuFadeEnableType.Out))
                    FadeOut(hideFadeOutSettings);
            }
            else
            {
                if (menuFading.HasFlag(MenuFadeEnableType.Out))
                    FadeOut(hideFadeOutSettings, null, InternalHide);
                else
                    InternalHide();
            }

            void InternalHide()
            {
                _menu.gameObject.SetActive(false);

                if (_menu.Manager.MenuDebug != MenuDebugMode.Off) Debug.Log($"[UIMenu] HIDE {_menu}");

                _menu.Manager.PostMenuHideEvent?.Invoke(_menu);
                _menu.Hide();
                IsTransitioning = false;
                ProcessNextAction();
            }
        }

        public virtual Routine FadeIn(TweenSettings fadeTime, float? alpha = null, Action callback = null)
        {
            if (alpha != null) SetAlpha(alpha.Value);

            _numFadeOuts--;
            if (_numFadeOuts < 0) _numFadeOuts = 0;
            if (_numFadeOuts == 0)
            {
                return _fadeRoutine.Replace(_menu.CanvasGroup != null
                    ? _menu.CanvasGroup.FadeTo(1.0f, fadeTime).OnStart(() =>
                    {
                        _menu.CanvasGroup.blocksRaycasts = true;
                        _menu.CanvasGroup.interactable = true;
                    }).OnComplete(callback).Play()
                    : default);
            }

            return _fadeRoutine;
        }

        public virtual Routine FadeIn(float fadeTime = 0.25f, float? alpha = null, Action callback = null) =>
            FadeIn(new TweenSettings(fadeTime), alpha, callback);

        public virtual Routine FadeOut(TweenSettings fadeTime, float? alpha = null, Action callback = null)
        {
            if (alpha != null) SetAlpha(alpha.Value);
            _numFadeOuts++;
            if (_numFadeOuts == 1)
            {
                return _fadeRoutine.Replace(_menu.CanvasGroup != null
                    ? _menu.CanvasGroup.FadeTo(0.0f, fadeTime).OnComplete(() =>
                    {
                        _menu.CanvasGroup.blocksRaycasts = false;
                        _menu.CanvasGroup.interactable = false;
                        callback?.Invoke();
                    }).Play()
                    : default);
            }

            return _fadeRoutine;
        }

        public virtual Routine FadeOut(float fadeTime = 0.25f, float? alpha = null, Action callback = null) =>
            FadeOut(new TweenSettings(fadeTime), alpha, callback);

        /// <summary>
        /// Sets the depth of the fadeout
        /// </summary>
        /// <param name="depth"></param>
        public void SetFadeOutDepth(int depth) => _numFadeOuts = depth;

        /// <summary>
        /// Gets the current alpha of the menu
        /// </summary>
        /// <returns></returns>
        public float GetAlpha() => _menu.CanvasGroup == null ? 0.0f : _menu.CanvasGroup.alpha;

        /// <summary>
        /// Set's the alpha of the menu
        /// </summary>
        /// <param name="alpha"></param>
        public void SetAlpha(float alpha)
        {
            if (_menu.CanvasGroup == null) return;
            _menu.CanvasGroup.alpha = alpha;
            _menu.CanvasGroup.blocksRaycasts = alpha > 0;
            _menu.CanvasGroup.interactable = alpha > 0;
        }
    }
}