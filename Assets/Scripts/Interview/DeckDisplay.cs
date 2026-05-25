using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cards;

namespace Interview
{
    public class DeckDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private Transform handPanel; // Parent object for the 8 card buttons
        
        [Header("Button Prefab")]
        [SerializeField] private GameObject cardButtonPrefab; // Button with Image component
        
        [Header("Hover Panel")]
        [SerializeField] private MouseUI mouseUIPanel; // Reference to the mouse UI script
        
        private List<Button> cardButtons = new List<Button>();
        private List<SkillCardData> currentHand = new List<SkillCardData>();
        
        void Start()
        {
            if (deckManager == null)
                deckManager = FindObjectOfType<DeckManager>();
            
            if (deckManager != null)
            {
                deckManager.OnHandChanged += UpdateDeckDisplay;
            }
        }
        
        void UpdateDeckDisplay(List<SkillCardData> hand)
        {
            currentHand = hand;
            
            // Clear existing buttons
            foreach (Button btn in cardButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            cardButtons.Clear();
            
            // Create buttons for each card in hand
            for (int i = 0; i < hand.Count && i < 8; i++) // Max 8 buttons
            {
                SkillCardData card = hand[i];
                GameObject newButtonObj = Instantiate(cardButtonPrefab, handPanel);
                Button newButton = newButtonObj.GetComponent<Button>();
                Image fillImage = newButtonObj.GetComponent<Image>();
                
                // Set button color based on card type
                if (fillImage != null)
                {
                    fillImage.color = GetCardColor(card.cardType);
                }
                
                // Store card reference on button
                CardButtonData buttonData = newButtonObj.GetComponent<CardButtonData>();
                if (buttonData == null)
                    buttonData = newButtonObj.AddComponent<CardButtonData>();
                buttonData.SetCard(card);
                
                // Add click listener
                int cardIndex = i; // Capture for lambda
                newButton.onClick.AddListener(() => OnCardClicked(cardIndex));
                
                // Add hover events
                AddHoverEvents(newButtonObj, card);
                
                cardButtons.Add(newButton);
            }
        }
        
        
        
        /*
        private void OnCardClicked(int cardIndex)
        {
            if (currentHand.Count > cardIndex)
            {
                SkillCardData card = currentHand[cardIndex];
                Debug.Log($"Card clicked: {card.cardName}");
                
                // Find GameManager and play card
                GameManager gm = FindObjectOfType<GameManager>();
                if (gm != null)
                {
                    gm.PlayCard(card);
                }
            }
        }
        */
        
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
        
        private void AddHoverEvents(GameObject buttonObj, SkillCardData card)
        {
            
            // Add EventTrigger if it doesn't exist
            UnityEngine.EventSystems.EventTrigger trigger = buttonObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = buttonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // Pointer enter event
            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener((_) => mouseUIPanel?.ShowCardInfo(card));
            trigger.triggers.Add(enterEntry);

            // Pointer exit event
            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener((_) => mouseUIPanel?.Hide());
            trigger.triggers.Add(exitEntry);
            
        }
        
        
        
        private Color GetCardColor(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Technical:
                    return new Color(0.2f, 0.6f, 1f); // Blue
                case CardType.Soft:
                    return new Color(0.2f, 0.8f, 0.4f); // Green
                case CardType.Access:
                    return new Color(0.9f, 0.6f, 0.2f); // Orange
                default:
                    return Color.gray;
            }
        }
     
        
        void OnDestroy()
        {
            if (deckManager != null)
            {
                deckManager.OnHandChanged -= UpdateDeckDisplay;
            }
        }
    }
    
    // Simple component to store card data on button
    public class CardButtonData : MonoBehaviour
    {
        public SkillCardData card;
        
        public void SetCard(SkillCardData newCard)
        {
            card = newCard;
        }
    }
}