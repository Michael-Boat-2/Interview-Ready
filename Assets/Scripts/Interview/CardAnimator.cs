using UnityEngine;
using System.Collections;

namespace Interview
{
    public class CardAnimator : MonoBehaviour
    {
        
        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float selectionPunchScale = 1.1f;
        [SerializeField] private float tweenDuration = 0.15f;
    
    
        private CanvasGroup _canvasGroup;
        private Coroutine _currentTween;
        private Vector3 _originalScale;
        
        
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalScale = transform.localScale;
        }
        
        
        public void PlayHoverEnter()
        {
            TweenScale(_originalScale * hoverScale, tweenDuration);
        }

        public void PlayHoverExit()
        {
            TweenScale(_originalScale, tweenDuration);
        }

        
        public void PlaySelectionBounce()
        {
            StartCoroutine(BounceRoutine());
        }

        private IEnumerator BounceRoutine()
        {
            // punch up
            TweenScale(_originalScale * selectionPunchScale, tweenDuration * 0.5f);
            yield return new WaitForSeconds(tweenDuration * 0.5f);
            // return
            TweenScale(_originalScale, tweenDuration * 0.5f);
        }
        
        public void PlayFlyOut(System.Action onComplete = null)
        {
            StartCoroutine(FlyOutRoutine(onComplete));
        }
        
        public void PlayReveal()
        {
            transform.localScale = Vector3.zero;
            TweenScale(_originalScale, 0.2f);
        }
    
        
        private IEnumerator FlyOutRoutine(System.Action onComplete)
        {
            // Shrink + fade simultaneously
            float elapsed = 0f;
            float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t);
                if (_canvasGroup)
                    _canvasGroup.alpha = 1f - t;
                yield return null;
            }
            onComplete?.Invoke();
            Destroy(gameObject);  // or return to pool
        }
        
        
        private void TweenScale(Vector3 targetScale, float duration)
        {
            if (_currentTween != null)
                StopCoroutine(_currentTween);
            _currentTween = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, targetScale, elapsed / duration);
                yield return null;
            }
            transform.localScale = targetScale;
        }
        
        
    
    }
}
