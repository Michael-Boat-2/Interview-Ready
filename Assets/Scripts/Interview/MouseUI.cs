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

        [SerializeField] private TextMeshProUGUI tagsText;

        [Header("Position Settings")]
        [SerializeField] private Vector2 offset = new Vector2(15, -15);

        private Canvas parentCanvas;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (!parentCanvas)
                parentCanvas = FindFirstObjectByType<Canvas>();

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
            if (!card) return;

            if (cardNameText)
                cardNameText.text = card.cardName;

            if (cardTypeText)
                cardTypeText.text = card.cardType.ToString();

            if (cardValueText)
            {
                string valueLabel = card.cardType == CardType.Technical ? "Damage" :
                                   (card.cardType == CardType.Soft ? "Composure" : "Boost");
                cardValueText.text = $"{valueLabel}: {card.value}";
            }

            if (cardDescriptionText)
                cardDescriptionText.text = card.description;
            
            //Show synergy tags
            if (tagsText)
            {
                if (card.tags is { Length: > 0 })
                {
                    tagsText.text = string.Join("  ~  ", card.tags);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(tagsText.rectTransform);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(tagsText.transform.parent as RectTransform);
                    tagsText.gameObject.SetActive(true);
                }
                else
                {
                    tagsText.text = "";
                    tagsText.gameObject.SetActive(false);   // hide if no tags
                }
            }
            
            
            
            //Color work
            Color baseColor = card.cardColor;
            Color darkColor = DeckDisplay.Darken(baseColor, 0.2f);

            if (cardIconImage && card.cardIcon)
                cardIconImage.sprite = card.cardIcon;

            // should use actual card color
            if (cardBackgroundImage)
                cardBackgroundImage.color = baseColor;

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