using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace Tween
{
    /// <summary>
    /// Gives UI elements a subtle hover effect
    /// </summary>
    public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scale")]
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float animationDuration = 0.15f;

        [Header("Color Tint (Optional)")]
        [SerializeField] private bool tintOnHover = false;
        [SerializeField] private Color hoverColor = Color.white;
        [SerializeField] private Graphic targetGraphic;   // Image or TextMeshProUGUI

        private Vector3 _originalScale;
        private Color _originalColor;
        private Coroutine _currentTween;

        void Start()
        {
            _originalScale = transform.localScale;
            if (targetGraphic)
                _originalColor = targetGraphic.color;
            else if (tintOnHover)
                Debug.LogWarning($"UIHoverEffect on {name}: tintOnHover is true but no targetGraphic assigned.");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TweenScale(_originalScale * hoverScale, animationDuration);
            if (tintOnHover && targetGraphic)
                targetGraphic.CrossFadeColor(hoverColor, animationDuration, true, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TweenScale(_originalScale, animationDuration);
            if (tintOnHover && targetGraphic)
                targetGraphic.CrossFadeColor(_originalColor, animationDuration, true, true);
        }

        private void TweenScale(Vector3 targetScale, float duration)
        {
            if (_currentTween != null)
                StopCoroutine(_currentTween);
            _currentTween = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 target, float duration)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            transform.localScale = target;
        }
    }
}