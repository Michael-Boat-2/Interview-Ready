using System.Collections;
using UnityEngine;

namespace Managers
{
    public class UIShake : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Vector2 originalPosition;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
        }

        public void Shake(float intensity = 10f, float duration = 0.25f)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                rectTransform.anchoredPosition = originalPosition + new Vector2(x, y);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}