using System.Collections.Generic;
using System.Linq;
using Cards;
using Managers;
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

        [Header("Discard Button")]
        [SerializeField] private Button discardButton;      // Discard button
        [SerializeField] private TMPro.TextMeshProUGUI discardButtonLabel; // Optional label
        
        
        [Header("Hover Panel")]
        [SerializeField] private MouseUI mouseUIPanel;

        [Header("Selection Visuals")]
        [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.9f, 0.1f); // Yellow highlight
        [SerializeField] private Color defaultOutlineColor = Color.clear;

        [SerializeField]private List<Button> cardButtons = new List<Button>();
        private List<SkillCardData> currentHand = new List<SkillCardData>();

        void Awake()
        {
            if (!deckManager)
                deckManager = FindObjectOfType<DeckManager>();

            if (!gameManager)
                gameManager = FindObjectOfType<GameManager>();

            if (deckManager)
                deckManager.OnHandChanged += UpdateDeckDisplay;

            if (gameManager)
                gameManager.OnSelectedHandChanged += OnSelectionChanged;

            // Wire up the Play Hand button, to answer questions after selecting some cards
            if (playHandButton)
                playHandButton.onClick.AddListener(OnPlayHandClicked);
            
            if(discardButton)
                discardButton.onClick.AddListener(OnDiscardClicked);

            // Start with button disabled until player selects cards
            SetPlayHandButtonState(false);
        }

         //Update hand display
         private void UpdateDeckDisplay(List<SkillCardData> hand)
        {
            currentHand = hand;

            // Clear old buttons
            foreach (var btn in cardButtons.Where(btn => btn))
            {
                Destroy(btn.gameObject);
            }
            
            cardButtons.Clear();

            for (var i = 0; i < hand.Count && i < 8; i++)
            {
                SkillCardData card = hand[i];
                var newButtonObj = Instantiate(cardButtonPrefab, handPanel);
                var newButton = newButtonObj.GetComponent<Button>();
                    

                // Store card reference
                var buttonData = newButtonObj.GetComponent<CardButtonData>();
                
                if (!buttonData)
                    buttonData = newButtonObj.AddComponent<CardButtonData>();
                
                var fillImage = buttonData.cardImage;

                if (fillImage)
                {
                    fillImage.color = GetCardColor(card.cardType);
                    fillImage.sprite = card.cardIcon;
                }
                   
                
                buttonData.SetCard(card);

                // Click toggles selection — capture index so duplicates are treated as separate slots
                var capturedIndex = i;
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

            if (playHandButtonLabel)
                playHandButtonLabel.text = selected.Count > 0
                    ? $"Answer Question ({selected.Count}/{gameManager.MaxSelectedCards})"
                    : "Answer Question";
        }

        private void RefreshSelectionVisuals()
        {
            for (int i = 0; i < cardButtons.Count && i < currentHand.Count; i++)
            {
                SkillCardData card = currentHand[i];
                bool isSelected = gameManager != null && gameManager.IsIndexSelected(i);


                if (isSelected)
                {
                    
                    var img = cardButtons[i].GetComponent<CardHighlight>();
                    if (img != null)
                    {
                        //Color base_ = GetCardColor(card.cardType);
                    
                        //Highlight is true
                        img.Highlight(true);
                    
                    }
                    
                }
                else
                {
                    var img = cardButtons[i].GetComponent<CardHighlight>();
                    if (img != null)
                    {
                        //Color base_ = GetCardColor(card.cardType);
                    
                        //Highlight is true
                        img.Highlight(false);
                    
                    }
                }
                
               
            }
        }

        private void OnPlayHandClicked()
        {
            if (gameManager)
                gameManager.PlaySelectedHand();
        }
        
        private void OnDiscardClicked()
        {
            if (gameManager)
            {
                gameManager.DiscardSelectedHand();
            }
        }

        
        //Disable and enable answer question and discard idea buttons
        private void SetPlayHandButtonState(bool interactable)
        {
            if (!playHandButton) return;
            
            playHandButton.interactable = interactable;
            discardButton.interactable = interactable;
        }


      
        
        
        
  
        /// <summary>
        /// Adding hover events for the button
        /// </summary>
   

        private void AddHoverEvents(GameObject buttonObj, SkillCardData card)
        {
            var trigger = buttonObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            
            if (!trigger)
            {
                trigger = buttonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
               

            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            
            enterEntry.callback.AddListener(_ => mouseUIPanel?.ShowCardInfo(card));
            trigger.triggers.Add(enterEntry);
            

            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener(_ => mouseUIPanel?.Hide());
            trigger.triggers.Add(exitEntry);
        }

        /// <summary>
        /// Helper functions
        /// </summary>
  

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

        private void OnDestroy()
        {
            if (deckManager)
                deckManager.OnHandChanged -= UpdateDeckDisplay;

            if (gameManager)
                gameManager.OnSelectedHandChanged -= OnSelectionChanged;
        }
    }


   
}