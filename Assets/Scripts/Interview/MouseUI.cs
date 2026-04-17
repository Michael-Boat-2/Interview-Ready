using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cards;

namespace Interview
{
    public class MouseUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private RectTransform panel; // The panel that pops up
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI cardValueText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private Image cardIconImage;
        [SerializeField] private Image cardBackgroundImage;
        
        [Header("Position Settings")]
        [SerializeField] private Vector2 offset = new Vector2(15, -15); // Offset from mouse
        [SerializeField] private bool followMouse = true;
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        
        private Canvas parentCanvas;
        private bool isVisible = false;
        private CanvasGroup canvasGroup;
        
        void Start()
        {
            // Get or add CanvasGroup for fade effects
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Get parent canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                parentCanvas = FindObjectOfType<Canvas>();
            
            // Start hidden
            panel.gameObject.SetActive(false);
            canvasGroup.alpha = 0f;
        }
        
        void Update()
        {
            if (followMouse && isVisible)
            {
                UpdatePanelPosition();
            }
        }
        
        private void UpdatePanelPosition()
        {
            if (parentCanvas == null) return;
            
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out mousePos
            );
            
            panel.anchoredPosition = mousePos + offset;
        }
        
        public void ShowCardInfo(SkillCardData card)
        {
            if (card == null) return;
            
            // Update UI text
            if (cardNameText != null)
                cardNameText.text = card.cardName;
            
            if (cardTypeText != null)
                cardTypeText.text = card.cardType.ToString();
            
            if (cardValueText != null)
            {
                string valueLabel = card.cardType == CardType.Technical ? "Damage" : 
                                   (card.cardType == CardType.Soft ? "Composure" : "Boost");
                cardValueText.text = $"{valueLabel}: {card.value}";
            }
            
            if (cardDescriptionText != null)
                cardDescriptionText.text = card.description;
            
            if (cardIconImage != null && card.cardIcon != null)
                cardIconImage.sprite = card.cardIcon;
            
            if (cardBackgroundImage != null)
                cardBackgroundImage.color = GetCardColor(card.cardType);
            
            // Show panel
            if (!panel.gameObject.activeSelf)
                panel.gameObject.SetActive(true);
            
            isVisible = true;
            
            // Fade in
            if (canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeIn());
            }
        }
        
        public void Hide()
        {
            isVisible = false;
            
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                panel.gameObject.SetActive(false);
            }
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            while (canvasGroup.alpha < 0.95f)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            while (canvasGroup.alpha > 0.05f)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            panel.gameObject.SetActive(false);
        }
        
        private Color GetCardColor(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Technical:
                    return new Color(0.2f, 0.6f, 1f, 0.9f); // Blue
                case CardType.Soft:
                    return new Color(0.2f, 0.8f, 0.4f, 0.9f); // Green
                case CardType.Access:
                    return new Color(0.9f, 0.6f, 0.2f, 0.9f); // Orange
                default:
                    return Color.gray;
            }
        }
        
        // For testing without card data
        public void ShowTestInfo()
        {
            if (cardNameText != null)
                cardNameText.text = "Test Card";
            if (cardTypeText != null)
                cardTypeText.text = "Technical";
            if (cardValueText != null)
                cardValueText.text = "Damage: 5";
            if (cardDescriptionText != null)
                cardDescriptionText.text = "This is a test card description.";
            
            panel.gameObject.SetActive(true);
            isVisible = true;
        }
    }
}