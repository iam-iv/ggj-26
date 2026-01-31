using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Default duration for fade transitions.")]
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Tooltip("If true, sets alpha to 1 on Awake. If false, sets alpha to 0.")]
        [SerializeField] private bool startVisible = false;

        private CanvasGroup _canvasGroup;
        private Coroutine _currentRoutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            SetState(startVisible ? 1f : 0f);
        }

        /// <summary>
        /// Fades the CanvasGroup to 1 (Visible)
        /// </summary>
        public void FadeIn() => FadeIn(fadeDuration, null);

        /// <summary>
        /// Fades the CanvasGroup to 1 (Visible) with callback
        /// </summary>
        public void FadeIn(UnityAction onComplete) => FadeIn(fadeDuration, onComplete);

        public void FadeIn(float duration, UnityAction onComplete = null)
        {
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(FadeRoutine(1f, duration, onComplete));
        }

        /// <summary>
        /// Fades the CanvasGroup to 0 (Invisible)
        /// </summary>
        public void FadeOut() => FadeOut(fadeDuration, null);

        /// <summary>
        /// Fades the CanvasGroup to 0 (Invisible) with callback
        /// </summary>
        public void FadeOut(UnityAction onComplete) => FadeOut(fadeDuration, onComplete);

        public void FadeOut(float duration, UnityAction onComplete = null)
        {
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(FadeRoutine(0f, duration, onComplete));
        }

        /// <summary>
        /// Instantly sets the state without fading
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            SetState(visible ? 1f : 0f);
        }

        private void SetState(float alpha)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            
            _canvasGroup.alpha = alpha;
            bool isVisible = alpha > 0.99f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, UnityAction onComplete)
        {
            
            float startAlpha = _canvasGroup.alpha;
            float time = 0f;

            // If fading out, disable interaction immediately
            if (targetAlpha < startAlpha)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            while (time < duration)
            {
                time += Time.unscaledDeltaTime; // Use unscaled so it works when paused
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;

            // If faded in, enable interaction
            if (targetAlpha >= 0.99f)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            onComplete?.Invoke();
            _currentRoutine = null;
        }
    }
}
