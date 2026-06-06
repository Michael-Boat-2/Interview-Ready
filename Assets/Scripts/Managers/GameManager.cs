using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Interview;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
    
        [Header("References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private PlayerConfidence playerConfidence;
        [SerializeField] private InterviewerDoubt interviewerDoubt;
    
    
        [Header("Debug UI")]
        [SerializeField] private Image confidenceFill;
        [SerializeField] private Image doubtFill;

        [Header("Enemy Settings")]
        [SerializeField] private int enemyMinDamage = 3;
        [SerializeField] private int enemyMaxDamage = 7;
    
        [Header("Hand Settings")]
        [SerializeField] private int maxSelectedCards = 5;
    
      
        [Header("Test Cards")]
        [SerializeField] private List<SkillCardData> testStartingDeck = new List<SkillCardData>();
    
        // Events for UI
        public System.Action OnInterviewStarted;
        public System.Action OnInterviewWon;
        public System.Action OnInterviewLost;
        public System.Action<string> OnBattleMessage;
    
    
        // Fired whenever the selected hand changes — UI listens to this to highlight cards
        public System.Action<List<SkillCardData>> OnSelectedHandChanged;

        private List<int> selectedHandIndices = new List<int>(); // indices into hand, not references
    
    
        private bool isBattleActive = false;
    
        //autostart
        [SerializeField]private bool autoStart = true;

    
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // Find references if not assigned
            if (!turnManager)
                turnManager = FindFirstObjectByType<TurnManager>();

            if (!deckManager)
            {
                deckManager = FindFirstObjectByType<DeckManager>();
            }




            if (!playerConfidence)
            {
                Debug.LogWarning("No player confidence found.");
            }


            if (!interviewerDoubt)
            {
                Debug.LogWarning("No interviewer doubt.");
            }
           

            // Subscribe to events
            if (turnManager != null)
            {
                turnManager.OnPlayerTurnStart += OnPlayerTurnStarted;
                turnManager.OnEnemyTurnStart += OnEnemyTurnStarted;
            }

            if (playerConfidence != null)
                playerConfidence.OnPlayerDied += OnPlayerDied;

            if (interviewerDoubt != null)
                interviewerDoubt.OnInterviewerDefeated += OnInterviewerDefeated;


            if (autoStart)
            {
                StartInterview();
            }


        }

        // Update is called once per frame
        private void Update()
        {
        
        }


        private void StartInterview()
        {
            Debug.Log("Interview BEGINS");
            OnBattleMessage?.Invoke("Interview starts!");
        
            // Reset all systems
            playerConfidence?.ResetForNewInterview();
            interviewerDoubt?.ResetForNewInterview();
        
            // Hook filled images up to stat events
            if (confidenceFill && playerConfidence)
            {
                confidenceFill.fillAmount = playerConfidence.ConfidencePercentage;
                playerConfidence.OnConfidenceChanged += () => confidenceFill.fillAmount = playerConfidence.ConfidencePercentage;
            }

            if (doubtFill && interviewerDoubt)
            {
                doubtFill.fillAmount = interviewerDoubt.DoubtPercentage;
                interviewerDoubt.OnDoubtChanged += () => doubtFill.fillAmount = interviewerDoubt.DoubtPercentage;
            }

            // Setup deck with test cards
            if (deckManager && testStartingDeck.Count > 0)
            {
                deckManager.SetupDeck(testStartingDeck);
                deckManager.DrawStartingHand();
            }
            else
            {
                Debug.LogWarning("No test cards found! Create test cards or assign them in Inspector.");
            }
        
            isBattleActive = true;
            OnInterviewStarted?.Invoke();
        
            // Start with player turn
            turnManager?.StartPlayerTurn();
        }
    
    
    
        //Called by deck display
        // Toggle a card in or out of the selected hand.
        // Returns true if the card is now selected, false if deselected or rejected.
    
        public bool ToggleCardSelection(int handIndex)
        {
            if (!isBattleActive || turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                OnBattleMessage?.Invoke("It's not your turn!");
                return false;
            }

            if (handIndex < 0 || handIndex >= deckManager.Hand.Count)
            {
                Debug.LogWarning("Invalid hand index.");
                return false;
            }

            SkillCardData card = deckManager.Hand[handIndex];

            if (selectedHandIndices.Contains(handIndex))
            {
                // Deselect
                selectedHandIndices.Remove(handIndex);
                OnSelectedHandChanged?.Invoke(GetSelectedCards());
                OnBattleMessage?.Invoke($"{card.cardName} removed from selection.");
                return false;
            }
            else
            {
                if (selectedHandIndices.Count >= maxSelectedCards)
                {
                    OnBattleMessage?.Invoke($"You can only select up to {maxSelectedCards} cards!");
                    return false;
                }

                selectedHandIndices.Add(handIndex);
                OnSelectedHandChanged?.Invoke(GetSelectedCards());
                OnBattleMessage?.Invoke($"{card.cardName} added to selection. ({selectedHandIndices.Count}/{maxSelectedCards})");
                return true;
            }
        }

        public bool IsIndexSelected(int handIndex) => selectedHandIndices.Contains(handIndex);
        public int SelectedCount => selectedHandIndices.Count;
        public int MaxSelectedCards => maxSelectedCards;

        private List<SkillCardData> GetSelectedCards()
        {
            List<SkillCardData> selected = new List<SkillCardData>();
            foreach (int i in selectedHandIndices)
            {
                if (i < deckManager.Hand.Count)
                    selected.Add(deckManager.Hand[i]);
            }
            return selected;
        }
    
        public void PlaySelectedHand()
        {
            if (!isBattleActive)
            {
                Debug.Log("No active interview");
                return;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                OnBattleMessage?.Invoke("Wait for your turn");
                return;
            }

            if (selectedHandIndices.Count == 0)
            {
                OnBattleMessage?.Invoke("Select at least one card to play");
                return;
            }

            // Total up all effects
            var totalDoubtDamage = 0;
            var totalComposureGain = 0;
            var playedNames = new List<string>();

            var selectedCards = GetSelectedCards();
            
            
            //adding names of all selected cards
            foreach (var card in selectedCards)
            {
                playedNames.Add(card.cardName);
                
                //pass by reference
                AccumulateCardEffect(card, ref totalDoubtDamage, ref totalComposureGain);
            }
            
            
            // Apply totalled effects
            
            //summary string of all played cards
            var summary = $"Played: {string.Join(", ", playedNames)}";

            //reduce interviewer doubt by total doubt damage amount
            if (totalDoubtDamage > 0)
            {
                interviewerDoubt?.ReduceDoubt(totalDoubtDamage);
                summary += $"  && reduced interviewer doubt by {totalDoubtDamage} ";
            }

            if (totalComposureGain > 0)
            {
                playerConfidence?.AddComposure(totalComposureGain);
                summary += $"  && gained Composure of {totalComposureGain} ";
            }

            //battle message event evoked
            OnBattleMessage?.Invoke(summary);
            Debug.Log(summary);
            
            
            // Discard played cards into the discard pile,
            // sort descending so removing higher indices first doesn't shift lower ones
            selectedHandIndices.Sort((a, b) => b.CompareTo(a));
            
            foreach (var i in selectedHandIndices)
                deckManager.PlayCardAt(i);

            selectedHandIndices.Clear();
            OnSelectedHandChanged?.Invoke(new List<SkillCardData>());

            CheckBattleState();

            // Hand was accepted, switch to enemy turn
            if (isBattleActive)
                turnManager?.EndPlayerTurn();
        }

        
        
        //Adds card effects
        private void AccumulateCardEffect(SkillCardData card, ref int doubtDamage, ref int composureGain)
        {
            switch (card.cardType)
            {
                case CardType.Technical:
                    doubtDamage += card.value;
                    break;

                case CardType.Soft:
                    //only add to composure for now
                    composureGain += card.value;
                    //doubtDamage += Mathf.Max(1, card.value / 2); // Soft skills still chip away at doubt
                    break;

                case CardType.Access:
                    // Networking / access cards split the value between both stats
                    doubtDamage += Mathf.CeilToInt(card.value * 0.6f);
                    composureGain += Mathf.FloorToInt(card.value * 0.4f);
                    break;
            }
        }


        public void DiscardSelectedHand()
        {
            
            //Discards will have an extra layer of control, like max discards per turn or something
            
             if (!isBattleActive)
             {
                 Debug.Log("No active interview!");
                 return;
             }

             if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
             {
                 OnBattleMessage?.Invoke("Wait for your turn!");
                 return;
             }

             if (selectedHandIndices.Count == 0)
             {
                 OnBattleMessage?.Invoke("Select at least one card to discard");
                 return;
             }

             var selectedCards = GetSelectedCards();
             var discardedNames = selectedCards.Select(card => card.cardName).ToList();

             //Discard
             var summary = $"Discarded: {string.Join(", ", discardedNames)}";

     
             OnBattleMessage?.Invoke(summary);
             Debug.Log(summary);

             // Discard played cards
             selectedHandIndices.Sort((a, b) => b.CompareTo(a));
             foreach (var i in selectedHandIndices)
                 deckManager.DiscardCardAt(i);

             selectedHandIndices.Clear();
             OnSelectedHandChanged?.Invoke(new List<SkillCardData>());

             CheckBattleState();

             // Discard was made
            
        }
    
        private void OnPlayerTurnStarted()
        {
            if (!isBattleActive) return;

            selectedHandIndices.Clear();
            OnSelectedHandChanged?.Invoke(new List<SkillCardData>());

            // Refill hand at the start of each player turn
            if (deckManager != null)
            {
                while (deckManager.HandCount < 5)
                {
                    if (!deckManager.DrawCard()) break;
                }
            }

            Debug.Log("Your turn — select up to 5 cards and play your hand!");
            OnBattleMessage?.Invoke("Your turn. Build your hand, then play it.");
        }
    
    
        private void OnEnemyTurnStarted()
        {
            if (!isBattleActive) return;
        
            Debug.Log("Interviewer's turn - Challenging your answers!");
            OnBattleMessage?.Invoke("Interviewer questions your experience...");
        
            // Enemy attacks
            StartCoroutine(EnemyTurnRoutine());
        }
    
        private IEnumerator EnemyTurnRoutine()
        {
            yield return new WaitForSeconds(0.5f);
        
            if (!isBattleActive) yield break;
        
            // Calculate random damage
            int damage = Random.Range(enemyMinDamage, enemyMaxDamage + 1);
        
            OnBattleMessage?.Invoke($"Interviewer challenges you! -{damage} Confidence");
            Debug.Log($"Interviewer deals {damage} damage");
        
            // Apply damage to player
            playerConfidence?.TakeDamage(damage);
        
            yield return new WaitForSeconds(0.3f);

            if (isBattleActive && playerConfidence.CurrentConfidence > 0)
                turnManager?.EndEnemyTurn();
        }

    
        public void EndPlayerTurn()
        {
            if (!isBattleActive) return;
        
            if (turnManager != null && turnManager.CurrentPhase == TurnPhase.PlayerTurn)
            {
                turnManager.EndPlayerTurn();
            }
        }
    
        private void CheckBattleState()
        {
            if (interviewerDoubt && interviewerDoubt.CurrentDoubt <= 0)
            {
                WinInterview();
            }
            else if (playerConfidence && playerConfidence.CurrentConfidence <= 0)
            {
                LoseInterview();
            }
        }
    
        private void OnInterviewerDefeated()
        {
            WinInterview();
        }
    
        private void OnPlayerDied()
        {
            LoseInterview();
        }
    
        private void WinInterview()
        {
            if (!isBattleActive) return;
        
            isBattleActive = false;
            Debug.Log("YOU GOT THE JOB");
            OnBattleMessage?.Invoke("Congratulations! The interviewer is impressed. You got the job!");
            OnInterviewWon?.Invoke();
        
            // Disable further turn actions
            if (turnManager)
            {
                // You might want to disable turn switching here
            }
        }
    
        private void LoseInterview()
        {
            if (!isBattleActive) return;
        
            isBattleActive = false;
            Debug.Log("YOU DID NOT GET THE JOB");
            OnBattleMessage?.Invoke("You didn't get the job. Keep building your skills and try again!");
            OnInterviewLost?.Invoke();
        
            // Disable further turn actions
            if (turnManager)
            {
                // You might want to disable turn switching here
            }
        }
    
 
    
        // For UI buttons
        public bool IsPlayerTurn()
        {
            return isBattleActive && turnManager != null && turnManager.CurrentPhase == TurnPhase.PlayerTurn;
        }
    
        public bool IsBattleActive()
        {
            return isBattleActive;
        }
    
        public List<SkillCardData> GetCurrentHand()
        {
            if (deckManager == null) return new List<SkillCardData>();
            return deckManager.Hand;
        }

        private void OnDestroy()
        {
            if (turnManager)
            {
                turnManager.OnPlayerTurnStart -= OnPlayerTurnStarted;
                turnManager.OnEnemyTurnStart -= OnEnemyTurnStarted;
            }
        
            if (playerConfidence)
                playerConfidence.OnPlayerDied -= OnPlayerDied;
        
            if (interviewerDoubt)
                interviewerDoubt.OnInterviewerDefeated -= OnInterviewerDefeated;
        }
    
    
    }
}