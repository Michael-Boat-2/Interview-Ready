using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Cards;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Interview
{
    public class DeckDisplay : MonoBehaviour
    {
        
        //References to game and deck manager
        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private GameManager gameManager;

        // Parent for card buttons
        [Header("Hand Panel")]
        [SerializeField] private Transform handPanel;     
        [SerializeField] private GameObject cardButtonPrefab;

        
        //Confirm a hand combination
        [Header("Play Hand Button")]
        [SerializeField] private Button playHandButton;  
        [SerializeField] private TMPro.TextMeshProUGUI playHandButtonLabel;
        
        // Discard button
        [Header("Discard Button")]
        [SerializeField] private Button discardButton;    
        [SerializeField] private TMPro.TextMeshProUGUI discardButtonLabel; 
        
        //Discard Counting
        [Header("Discard Counter")]
        [SerializeField] private TMPro.TextMeshProUGUI discardCounterLabel;
        
        
        //Manually end a turn
        [Header("End Turn Button")]
        [SerializeField] private Button endTurnButton;
        
        //Mouse panel for extra information
        [Header("Hover Panel")]
        [SerializeField] private MouseUI mouseUIPanel;

        [Header("Selection Visuals")]
        [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.9f, 0.1f); // Yellow highlight
        [SerializeField] private Color defaultOutlineColor = Color.clear;

        [SerializeField]private List<Button> cardButtons = new List<Button>();
        private List<SkillCardData> currentHand = new List<SkillCardData>();
        
        
        private bool _isAnimating = false;

        private void Awake()
        {
            if (!deckManager)
                deckManager = FindFirstObjectByType<DeckManager>();

            if (!gameManager)
                gameManager = FindFirstObjectByType<GameManager>();

            if (deckManager)
                deckManager.OnHandChanged += UpdateDeckDisplay;

            if (gameManager)
            {
                gameManager.OnSelectedHandChanged += OnSelectionChanged;
                gameManager.OnPlayerTurnBegan += AnimateHand; 
            }
                

            // Wire up the Play Hand button, to answer questions after selecting some cards
            if (playHandButton)
                playHandButton.onClick.AddListener(() => StartCoroutine(PlayWithAnimation()));
            
            if(discardButton)
                discardButton.onClick.AddListener(() => StartCoroutine(DiscardWithAnimation()));
            
            if (endTurnButton)
                endTurnButton.onClick.AddListener(OnEndTurnClicked);

            // Start with button disabled until player selects cards
            SetPlayHandButtonState(false);
            SetDiscardButtonState(false);
        }

        private void AnimateHand()
        {
            StartCoroutine(AnimateHandIn());
        }


        //Update endTurnButton here
        private void Update()
        {
            if (endTurnButton && gameManager)
                endTurnButton.interactable = gameManager.IsPlayerTurn() && gameManager.IsBattleActive();
            
            
            if (discardCounterLabel && gameManager)
            {
                int used = gameManager.DiscardsUsedThisTurn;
                int max = gameManager.MaxDiscardsPerTurn; // we need to expose this too
                discardCounterLabel.text = $"Discards: {used}/{max}";
            }
            
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
                
                //colors
                //var baseColor = GetCardColor(card.cardType);
                var baseColor = card.cardColor;
                var darkColor = Darken(baseColor, 0.2f);


                if (fillImage)
                {
                    //darker icon
                    fillImage.color = darkColor;
                    fillImage.sprite = card.cardIcon;
                }
                
                if (buttonData.cardBackground)
                    buttonData.cardBackground.color = baseColor;

                if (buttonData.cardNameText)
                {
                    buttonData.cardNameText.text = card.cardName;
                    buttonData.cardNameText.color = (baseColor.grayscale > 0.5f) ? Color.black : Color.white;
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
            SetDiscardButtonState(selected.Count == 1);

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
        
        private void OnEndTurnClicked()
        {
            if (gameManager)
                gameManager.EndPlayerTurn();
        }

        
        //Disable and enable answer question and discard idea buttons
        private void SetPlayHandButtonState(bool interactable)
        {
            if (!playHandButton) return;
            
            playHandButton.interactable = interactable;
            //discardButton.interactable = interactable;
        }


        private void SetDiscardButtonState(bool interactable)
        {
            if (!discardButton) return;
            
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
            
            //Add animator effects
            var anim = buttonObj.GetComponent<CardAnimator>();

            if (!anim)
            {
                anim = buttonObj.AddComponent<CardAnimator>();
            }

            //wire up on entry events
            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
                
            };
            
            enterEntry.callback.AddListener(_ =>
            {
                mouseUIPanel?.ShowCardInfo(card);
                anim?.PlayHoverEnter();
            });
            
            trigger.triggers.Add(enterEntry);
            

            //exit events
            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            
            exitEntry.callback.AddListener(_ =>
            {
                mouseUIPanel?.Hide();
                anim?.PlayHoverExit();
            });
            
            trigger.triggers.Add(exitEntry);
        }


        // Called when player clicks "Play Hand". Animates selected cards out, then plays
        private IEnumerator PlayWithAnimation()
        {
            
            if (_isAnimating) yield break;
            _isAnimating = true;
            
            // Find which cards are selected
            var selectedIndices = new List<int>();
            for (int i = 0; i < currentHand.Count; i++)
            {
                if (gameManager && gameManager.IsIndexSelected(i))
                    selectedIndices.Add(i);
            }
            
            
            // Fly‑out each selected card
            foreach (int idx in selectedIndices)
            {
                if (idx < cardButtons.Count)
                {
                    CardAnimator anim = cardButtons[idx].GetComponent<CardAnimator>();
                    if (anim)
                    {
                        // starts shrink + fade (0.3s)
                        anim.PlayFlyOut();   
                    }
                         
                }
            }
            
            
            yield return new WaitForSeconds(0.3f);
            
            gameManager?.PlaySelectedHand();
            _isAnimating = false;
            
            
        }
        
        // Called when player clicks "Discard". Animates the discarded card out, then discards
        private IEnumerator DiscardWithAnimation()
        {
            
            if (_isAnimating) yield break;
            _isAnimating = true;
            
            int selectedIndex = -1;
            for (int i = 0; i < currentHand.Count; i++)
            {
                if (gameManager != null && gameManager.IsIndexSelected(i))
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            
            if (selectedIndex >= 0 && selectedIndex < cardButtons.Count)
            {
                CardAnimator anim = cardButtons[selectedIndex].GetComponent<CardAnimator>();
                if (anim)
                {
                    anim.PlayFlyOut();   
                }
                   
            }
            
            //wait for animation 
            yield return new WaitForSeconds(0.3f);

            // then execute real discard
            gameManager?.DiscardSelectedHand();
            _isAnimating = false;
        }


        private IEnumerator AnimateHandIn()
        {
            yield return null;
            
            
            foreach (var cardBtn in cardButtons)
            {
                var anim = cardBtn.GetComponent<CardAnimator>();
                if (anim)
                {
                    // Tween from 0 to 1
                    anim.PlayReveal();  
                }
                
                // Wait for delay in between cards
                yield return new WaitForSeconds(0.05f);
            }
            
            
            
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
        
        
        public static Color Darken(Color color, float amount = 0.2f)
        {
            return new Color(
                Mathf.Clamp01(color.r - amount),
                Mathf.Clamp01(color.g - amount),
                Mathf.Clamp01(color.b - amount),
                color.a
            );
        }

        private void OnDestroy()
        {
            if (deckManager)
                deckManager.OnHandChanged -= UpdateDeckDisplay;

            if (gameManager)
            {
                gameManager.OnSelectedHandChanged -= OnSelectionChanged;
                gameManager.OnPlayerTurnBegan -= AnimateHand;
            }
                
        }
    }


   
}