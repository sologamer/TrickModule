using System.Collections;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace TrickModule.Game
{
    public class TrickVisualMono : MonoBehaviour
    {
        public Vector2 HighlightBorderAlphaRange = new Vector2(0.0f, 1.0f);
        public HighlightState CurrentHighlightState { get; set; }
        public Image HighlightBorderImage { get; set; }

        private Routine _highlightRoutine;

        public void SetHighlighted(Image highlightImage, HighlightState state, Color? highlightColor = null)
        {
            if (highlightImage != null) HighlightBorderImage = highlightImage;

            if (HighlightBorderImage == null) return;

            IEnumerator Blink()
            {
                var go = gameObject;
                while (this != null)
                {
                    yield return Routine.WaitCondition(() => go == null || go.activeSelf, 0.1f);
                    if (go == null) yield break;
                    if (HighlightBorderImage != null)
                    {
                        yield return HighlightBorderImage.Fade(HighlightBorderAlphaRange.x, 0.3f, Curve.Linear, 0.0f,
                            null,
                            true);
                        yield return Routine.WaitSeconds(1.0f);
                        if (go == null) yield break;
                        yield return HighlightBorderImage.Fade(HighlightBorderAlphaRange.y, 0.5f, Curve.Linear, 0.0f,
                            null,
                            true);
                    }
                    else
                    {
                        yield break;
                    }
                }
            }

            if (highlightColor != null) HighlightBorderImage.color = highlightColor.GetValueOrDefault();

            switch (state)
            {
                case HighlightState.Off:
                    if (HighlightBorderImage != null) HighlightBorderImage.gameObject.SetActive(false);
                    _highlightRoutine.Stop();
                    break;
                case HighlightState.AlwaysOn:
                    if (HighlightBorderImage != null) HighlightBorderImage.gameObject.SetActive(true);
                    _highlightRoutine.Stop();
                    break;
                case HighlightState.Blinking:
                    if (HighlightBorderImage != null) HighlightBorderImage.gameObject.SetActive(true);
                    _highlightRoutine.Replace(Blink());
                    break;
            }

            CurrentHighlightState = state;
        }
    }
}