using UnityEngine;
using UnityEngine.EventSystems;

namespace Tween
{
    /// <summary>
    /// Attach to the CardButton root. Tilt only the referenced visual target (e.g., Frame Holder).
    /// </summary>
    public class CardTiltEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual Target (e.g., Frame Holder)")]
        [SerializeField] private Transform tiltTarget;

        [Header("Tilt Settings")]
        [SerializeField] private float maxTiltAngle = 15f;
        [SerializeField] private float tiltSmoothSpeed = 8f;
        [SerializeField] private bool useUnscaledTime = true;

        private bool _isHovering = false;
        private RectTransform _parentRect;
        private Quaternion _targetRotation;

        void Start()
        {
            // Get the rect of this button (used to calculate mouse offset)
            _parentRect = GetComponent<RectTransform>();
            if (!tiltTarget)
            {
                Debug.LogWarning("CardTiltEffect: tiltTarget not assigned. Please assign the Frame Holder.");
                enabled = false;
            }
            _targetRotation =  Quaternion.identity;
        }

        void Update()
        {
            if (!_isHovering || !tiltTarget)
            {
                // Return to flat
                _targetRotation = Quaternion.identity;
                ApplyRotation();
                return;
            }

            // Mouse position relative to the button's rect
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                Input.mousePosition,
                null,
                out Vector2 localPoint
            );

            Vector2 size = _parentRect.rect.size;
            
            // Normalise to -0.5..0.5 (centre = 0)
            float normX = Mathf.Clamp01(localPoint.x / size.x) - 0.5f;
            float normY = Mathf.Clamp01(localPoint.y / size.y) - 0.5f;

            // Invert both axes – card tilts *away* from the cursor
            //normX = -normX;
            normY = -normY;

            // Target rotation: Yaw on Y axis, Pitch on X axis
            _targetRotation = Quaternion.Euler(
                normY * maxTiltAngle * 2f,
                normX * maxTiltAngle * 2f,
                0f
            );

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            float t = 1f - Mathf.Exp(-tiltSmoothSpeed * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
            tiltTarget.localRotation = Quaternion.Lerp(tiltTarget.localRotation, _targetRotation, t);
        }

        public void OnPointerEnter(PointerEventData eventData) => _isHovering = true;
        public void OnPointerExit(PointerEventData eventData) => _isHovering = false;
    }
}