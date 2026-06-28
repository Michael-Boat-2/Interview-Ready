using UnityEngine;

namespace Tween
{
    /// <summary>
    /// Makes a UI element gently bob up and down in place. 
    /// Attach to any RectTransform (text, panel, image, etc.).
    /// </summary>
    public class UIFloatEffect : MonoBehaviour
    {
        [Header("Motion")]
        // pixels up/down
        [SerializeField] private float amplitude = 5f;
        // cycles per second
        [SerializeField] private float frequency = 1.5f;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Direction")]
        [SerializeField] private bool floatVertical = true;
        [SerializeField] private bool floatHorizontal = false;

        private Vector2 _startPosition;
        private RectTransform _rectTransform;

        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform)
                _startPosition = _rectTransform.anchoredPosition;
        }

        void Update()
        {
            if (!_rectTransform) return;

            float t = (useUnscaledTime ? Time.unscaledTime : Time.time) * frequency;
            float offset = Mathf.Sin(t * 2f * Mathf.PI) * amplitude;

            Vector2 newPos = _startPosition;
            if (floatVertical)
                newPos.y += offset;
            if (floatHorizontal)
                newPos.x += offset;  

            _rectTransform.anchoredPosition = newPos;
        }
    }
}