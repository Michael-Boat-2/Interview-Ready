using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cards;

namespace Interview
{
    public class MouseUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI cardValueText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private Image cardIconImage;
        [SerializeField] private Image cardBackgroundImage;

        [Header("Position Settings")]
        [SerializeField] private Vector2 offset = new Vector2(15, -15);

        private Canvas parentCanvas;

        void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                parentCanvas = FindObjectOfType<Canvas>();

            panel.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (panel.gameObject.activeSelf)
                UpdatePanelPosition();
        }

        private void UpdatePanelPosition()
        {
            if (parentCanvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out Vector2 mousePos
            );

            panel.anchoredPosition = mousePos + offset;
        }

        public void ShowCardInfo(SkillCardData card)
        {
            if (card == null) return;

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

            panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            panel.gameObject.SetActive(false);
        }

        private Color GetCardColor(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Technical: return new Color(0.2f, 0.6f, 1f, 0.9f);
                case CardType.Soft:      return new Color(0.2f, 0.8f, 0.4f, 0.9f);
                case CardType.Access:    return new Color(0.9f, 0.6f, 0.2f, 0.9f);
                default:                 return Color.gray;
            }
        }
    }
}