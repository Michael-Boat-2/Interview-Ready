using System.Collections.Generic;
using Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Interview
{
    public class DeckDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private GameManager gameManager;

        [Header("Hand Panel")]
        [SerializeField] private Transform handPanel;       // Parent for card buttons
        [SerializeField] private GameObject cardButtonPrefab;

        [Header("Play Hand Button")]
        [SerializeField] private Button playHandButton;     // The confirm button 
        [SerializeField] private TMPro.TextMeshProUGUI playHandButtonLabel; // Optional label 

        [Header("Hover Panel")]
        [SerializeField] private MouseUI mouseUIPanel;

        [Header("Selection Visuals")]
        [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.9f, 0.1f); // Yellow highlight
        [SerializeField] private Color defaultOutlineColor = Color.clear;

        [SerializeField]private List<Button> cardButtons = new List<Button>();
        private List<SkillCardData> currentHand = new List<SkillCardData>();

        void Awake()
        {
            if (deckManager == null)
                deckManager = FindObjectOfType<DeckManager>();

            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            if (deckManager != null)
                deckManager.OnHandChanged += UpdateDeckDisplay;

            if (gameManager != null)
                gameManager.OnSelectedHandChanged += OnSelectionChanged;

            // Wire up the Play Hand button
            if (playHandButton != null)
                playHandButton.onClick.AddListener(OnPlayHandClicked);

            // Start with button disabled until player selects cards
            SetPlayHandButtonState(false);
        }

       //Update hand display
        void UpdateDeckDisplay(List<SkillCardData> hand)
        {
            currentHand = hand;

            // Clear old buttons
            foreach (Button btn in cardButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            cardButtons.Clear();

            for (int i = 0; i < hand.Count && i < 8; i++)
            {
                SkillCardData card = hand[i];
                GameObject newButtonObj = Instantiate(cardButtonPrefab, handPanel);
                Button newButton = newButtonObj.GetComponent<Button>();
                Image fillImage = newButtonObj.GetComponent<Image>();

                if (fillImage != null)
                    fillImage.color = GetCardColor(card.cardType);

                // Store card reference
                CardButtonData buttonData = newButtonObj.GetComponent<CardButtonData>();
                if (buttonData == null)
                    buttonData = newButtonObj.AddComponent<CardButtonData>();
                buttonData.SetCard(card);

                // Click toggles selection — capture index so duplicates are treated as separate slots
                int capturedIndex = i;
                newButton.onClick.AddListener(() => OnCardClicked(capturedIndex, newButtonObj));

                AddHoverEvents(newButtonObj, card);

                cardButtons.Add(newButton);
            }

            // Refresh selection visuals in case hand was redrawn mid-selection
            if (gameManager != null)
                RefreshSelectionVisuals();
        }

     
        private void OnCardClicked(int handIndex, GameObject buttonObj)
        {
            if (gameManager == null) return;
            gameManager.ToggleCardSelection(handIndex);
            // Visuals are updated via the OnSelectedHandChanged callback
        }

        private void OnSelectionChanged(List<SkillCardData> selected)
        {
            RefreshSelectionVisuals();
            SetPlayHandButtonState(selected.Count > 0);

            if (playHandButtonLabel != null)
                playHandButtonLabel.text = selected.Count > 0
                    ? $"Play Hand ({selected.Count}/{gameManager.MaxSelectedCards})"
                    : "Play Hand";
        }

        private void RefreshSelectionVisuals()
        {
            for (int i = 0; i < cardButtons.Count && i < currentHand.Count; i++)
            {
                SkillCardData card = currentHand[i];
                bool isSelected = gameManager != null && gameManager.IsIndexSelected(i);

                // Use an Outline component if present, otherwise tint the button
                Outline outline = cardButtons[i].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = isSelected;
                    outline.effectColor = selectedOutlineColor;
                }
                else
                {
                    // Fallback: slightly brighten the image to indicate selection
                    Image img = cardButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        Color base_ = GetCardColor(card.cardType);
                        img.color = isSelected ? base_ * 1.4f : base_;
                    }
                }
            }
        }

        private void OnPlayHandClicked()
        {
            if (gameManager != null)
                gameManager.PlaySelectedHand();
        }

        private void SetPlayHandButtonState(bool interactable)
        {
            if (playHandButton != null)
                playHandButton.interactable = interactable;
        }

        // ─────────────────────────────────────────────
        //  Hover
        // ─────────────────────────────────────────────

        private void AddHoverEvents(GameObject buttonObj, SkillCardData card)
        {
            UnityEngine.EventSystems.EventTrigger trigger = buttonObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = buttonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener((_) => mouseUIPanel?.ShowCardInfo(card));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener((_) => mouseUIPanel?.Hide());
            trigger.triggers.Add(exitEntry);
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────

        private Color GetCardColor(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Technical: return new Color(0.2f, 0.6f, 1f);
                case CardType.Soft:      return new Color(0.2f, 0.8f, 0.4f);
                case CardType.Access:    return new Color(0.9f, 0.6f, 0.2f);
                default:                 return Color.gray;
            }
        }

        void OnDestroy()
        {
            if (deckManager != null)
                deckManager.OnHandChanged -= UpdateDeckDisplay;

            if (gameManager != null)
                gameManager.OnSelectedHandChanged -= OnSelectionChanged;
        }
    }

    // Stores the card reference on each button GameObject
    public class CardButtonData : MonoBehaviour
    {
        public SkillCardData card;
        public void SetCard(SkillCardData newCard) => card = newCard;
    }
}