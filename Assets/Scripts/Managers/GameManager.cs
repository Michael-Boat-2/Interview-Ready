using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Interview;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

namespace Managers
{
    public enum InterviewEndReason
    {
        Won,                 // Doubt reached 0
        LostConfidence,      // Confidence reached 0
        TimedOut             // Max rounds reached without winning
    }
    
    
    public class GameManager : MonoBehaviour
    {
        
        [Header("Round Settings")]
        [SerializeField] private int maxRounds = 10;
        [SerializeField] private float[] questionThresholds = { 0.75f, 0.5f, 0.25f };
        private int _currentRound = 0;
        private int _currentQuestionIndex = 0;
        
        [Header("Interview Questions")]
        [SerializeField] private List<string> interviewQuestions = new List<string>();
        
        [Header("Question Display")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TextMeshProUGUI roundText;
    
        [Header("References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private PlayerConfidence playerConfidence;
        [SerializeField] private InterviewerDoubt interviewerDoubt;
    
    
        [Header("Battle UI")]
        [SerializeField] private PlayerProfileData profileData;
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private Image confidenceFill;
        [SerializeField] private Image composureFill;
        [SerializeField] private Image doubtFill;
        
        //Experience Data
        [SerializeField] private PlayerExperienceData experienceData;
        //[SerializeField] private TMP_Text experienceLevel;
        //[SerializeField] private Image experienceFill;

        [Header("Enemy Settings")]
        [SerializeField] private int enemyMinDamage = 3;
        [SerializeField] private int enemyMaxDamage = 7;
    
        [Header("Hand Settings")]
        [SerializeField] private int maxSelectedCards = 3;

        [Header("Discard Settings")] 
        [SerializeField]private int maxDiscardsPerTurn = 1;
        public int MaxDiscardsPerTurn => maxDiscardsPerTurn;
        private int _discardsUsedThisTurn;
        public int DiscardsUsedThisTurn => _discardsUsedThisTurn;
        
        
        [Header("Test Cards")]
        [SerializeField] private List<SkillCardData> testStartingDeck = new List<SkillCardData>();
    
        [Header("Camera Shake")]
        [SerializeField] private UIShake playerShake;
        [SerializeField] private UIShake interviewerShake;
        
        
        //Interview ended reason
        public Action<InterviewEndReason> OnInterviewEnded;
        
        
        // Events for UI
        public Action OnInterviewStarted;
        public Action OnInterviewWon;
        public Action OnInterviewLost;
        public Action<string> OnBattleMessage;
        
        //Fires every time hand is refilled
        public Action OnPlayerTurnBegan; 
        
        
        // Fired whenever the selected hand changes — UI listens to this to highlight cards
        public Action<List<SkillCardData>> OnSelectedHandChanged;

        private List<int> selectedHandIndices = new List<int>(); // indices into hand, not references
    
    
        private bool isBattleActive = false;
        
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip cardPlaySound;
        [SerializeField] private AudioClip cardDiscardSound;
        [SerializeField] private AudioClip cardDrawSound; 
        [SerializeField] private AudioClip enemyAttackSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
    
        //autostart
        [SerializeField]private bool autoStart = true;

    
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
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
            if (turnManager)
            {
                turnManager.OnPlayerTurnStart += OnPlayerTurnStarted;
                turnManager.OnEnemyTurnStart += OnEnemyTurnStarted;
            }

            if (playerConfidence)
                playerConfidence.OnPlayerDied += OnPlayerDied;

            if (interviewerDoubt)
                interviewerDoubt.OnInterviewerDefeated += OnInterviewerDefeated;
            
            
            DeckDisplay.OnHandDrawn += OnHandDrawn;

            /*if (!cameraShake)
                cameraShake = FindFirstObjectByType<UIShake>();*/

            if (autoStart)
            {
                StartInterview();
            }


        }
        
        
        

        private void Update()
        {
            //Timer text
            roundText.text = $"{(maxRounds - _currentRound) * 6}:00";
        }

   

        private void StartInterview()
        {
            Debug.Log("Interview BEGINS");
            OnBattleMessage?.Invoke("Interview starts!");
            
            
            // Reset all systems
            playerConfidence?.ResetForNewInterview();
            interviewerDoubt?.ResetForNewInterview();
            
            
            _currentRound = 0;
            _currentQuestionIndex = 0;
            
            if (questionText && interviewQuestions.Count > 0)
                questionText.text = $"Question 1:\n\n{interviewQuestions[0]}";
            OnBattleMessage?.Invoke($"The interview begins with: \"{interviewQuestions[0]}\"");
        
            //Player Name Label
            if (playerNameLabel && profileData)
                playerNameLabel.text = profileData.playerName;
            
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
            
            
            if (composureFill && playerConfidence)
            {
                composureFill.fillAmount = (float)playerConfidence.CurrentComposure / playerConfidence.MaxComposure;
                playerConfidence.OnComposureChanged += () =>
                    composureFill.fillAmount = (float)playerConfidence.CurrentComposure / playerConfidence.MaxComposure;
            }
            

            //var ownedCards = DeckManager.Instance ? DeckManager.Instance.GetAllOwnedCards() : null;
            var ownedCards = deckManager ? deckManager.GetAllOwnedCards() : null;
            
            // Setup deck with test cards
            if (ownedCards is { Count: > 0 })
            {
                deckManager.SetupDeck(ownedCards);
                deckManager.DrawStartingHand();
            }
            else if (testStartingDeck is { Count: > 0 })
            {
                deckManager.SetupDeck(testStartingDeck);
                deckManager.DrawStartingHand();
                Debug.Log("Using test starting deck.");
            }
            else
            {
                Debug.LogWarning("No owned cards found! Create test cards or assign them in Inspector.");
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

            var selectedCards = GetSelectedCards();
            
            //calculate card playing effects
            CalculatePlayPreview(selectedCards, out var totalDoubtDamage, out var totalComposureGain);
            
            
            //summary string of all played card
            var playedNames = selectedCards.Select(c => c.cardName).ToList();
            var summary = $"Played: {string.Join(", ", playedNames)}";

            //reduce interviewer doubt by total doubt damage amount
            if (totalDoubtDamage > 0)
            {
                interviewerDoubt?.ReduceDoubt(totalDoubtDamage);
                summary += $"  && reduced interviewer doubt by {totalDoubtDamage}";
            }

            if (totalComposureGain > 0)
            {
                playerConfidence?.AddComposure(totalComposureGain);
                summary += $"  && gained Composure of {totalComposureGain}";
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
            
            //Sound fx
            SoundManager.Instance?.PlaySFX(cardPlaySound);
            
            //light camera shake
            interviewerShake?.Shake(8f, 0.15f); 

            CheckBattleState();

            // Hand was accepted, switch to enemy turn and advance question
            if (isBattleActive)
            {
                StartCoroutine(AdvanceQuestionsSequence());
            }
        }

        
        public void CalculatePlayPreview(List<SkillCardData> selectedCards, out int totalDoubtDamage, out int totalComposureGain)
        {
            
            //Calculates total damage and composure gain for a given selection
            var techCount = selectedCards.Count(c =>
                c.cardType is CardType.Technical or CardType.Access);
            var softCount = selectedCards.Count(c =>
                c.cardType is CardType.Soft or CardType.Access);

            var techMult = GetSynergyMultiplier(techCount);
            var softMult = GetSynergyMultiplier(softCount);

            totalDoubtDamage = 0;
            totalComposureGain = 0;
            
            // Multiplication based on counts
            foreach (var card in selectedCards)
            {
                switch (card.cardType)
                {
                    case CardType.Technical:
                        totalDoubtDamage += Mathf.RoundToInt(card.value * techMult);
                        break;
                    case CardType.Soft:
                        totalComposureGain += Mathf.RoundToInt(card.value * softMult);
                        break;
                    case CardType.Access:
                        // Access cards always give base split, no multiplier
                        totalDoubtDamage += Mathf.CeilToInt(card.value * 0.5f);
                        totalComposureGain += Mathf.FloorToInt(card.value * 0.5f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            //tag matches add to answer performance
            //collecting all tags into a bag
            Dictionary<SkillTag, int> tagCounts = new Dictionary<SkillTag, int>();
            foreach (var card in selectedCards)
            {
                if (card.tags == null) continue;
                foreach (var skillTag in card.tags)
                {
                    tagCounts.TryAdd(skillTag, 0);
                    tagCounts[skillTag]++;
                }
            }
            
            
            //for each tag appearing at least twice, add bonus
            int tagBonusDamage = 0, tagBonusComposure = 0;
            foreach (var kvp in tagCounts)
            {
                if (kvp.Value >= 2)
                {
                    // simple plus 5 bonus for each matching tag
                    tagBonusDamage += 5;
                    tagBonusComposure += 5;
                }
            }
            
            totalDoubtDamage += tagBonusDamage;
            totalComposureGain += tagBonusComposure;
            
            
            
            
            // apply interview experience level multiplier
            //makes interviews easier as our player levels up
            if (experienceData)
            {
                totalDoubtDamage = Mathf.RoundToInt(totalDoubtDamage * experienceData.LevelMultiplier);
                totalComposureGain = Mathf.RoundToInt(totalComposureGain * experienceData.LevelMultiplier);
            }
            
            
            
        }

        private float GetSynergyMultiplier(int count)
        {
            if (count <= 1) return 1f;
            
            // 1 => 1.5 => 2
            return 1f + (count - 1) * 0.5f;
        }
        

        public void DiscardSelectedHand()
        {
            
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
             
             
             if (_discardsUsedThisTurn >= maxDiscardsPerTurn)
             {
                 OnBattleMessage?.Invoke("You've already discarded this turn!");
                 return;
             }

             if (selectedHandIndices.Count != 1)
             {
                 OnBattleMessage?.Invoke("Select only one card to discard");
                 return;
             }
             
             var index = selectedHandIndices[0];
             var discardedCard = deckManager.Hand[index];

             // Discard the card and pay composure cost of 2
             deckManager.DiscardCardAt(index);
             playerConfidence?.TakeDamage(2);  
             
             _discardsUsedThisTurn++;
             
       
             //Draw a new card
             deckManager.DrawCard();
             
             
             //Discard sound effect
             SoundManager.Instance?.PlaySFX(cardDiscardSound);
            
             
             //Clear selections
             selectedHandIndices.Clear();
             OnSelectedHandChanged?.Invoke(new List<SkillCardData>());

             CheckBattleState();

             
             //Discard Debug
             var summary = $"Discarded: {string.Join(", ", discardedCard.cardName)}";
             OnBattleMessage?.Invoke(summary);
             Debug.Log(summary);
             
            
        }
    
        private void OnPlayerTurnStarted()
        {
            if (!isBattleActive) return;
            
            if (isBattleActive && _currentRound < interviewQuestions.Count)
            {
                // Display next interview question
                /*if (questionText)
                 questionText.text = $"Question {currentRound+ 1}: \n \n {interviewQuestions[currentRound]}";*/
            }
            
            selectedHandIndices.Clear();
            OnSelectedHandChanged?.Invoke(new List<SkillCardData>());


            //discards limit kept at one
            _discardsUsedThisTurn = 0;

            // Refill hand at the start of each player turn
            if (deckManager)
            {
                while (deckManager.HandCount < 5)
                {
                    if (!deckManager.DrawCard()) break;
                }
            }
            
            OnPlayerTurnBegan?.Invoke(); 
            OnBattleMessage?.Invoke($"Your turn. Think about it and answer the question");
            
        }
        
       //Question sequence checking
        private IEnumerator AdvanceQuestionsSequence()
        {
            if (!isBattleActive || !interviewerDoubt) yield break;

            // Calculate current question based on current doubt
            int targetIndex = GetQuestionIndexFromDoubt(interviewerDoubt.CurrentDoubt);

            // Advance one question at a time, showing each briefly
            while (_currentQuestionIndex < targetIndex && _currentQuestionIndex < interviewQuestions.Count - 1)
            {
                _currentQuestionIndex++;
                if (questionText)
                    questionText.text = $"Question {_currentQuestionIndex + 1}:\n\n{interviewQuestions[_currentQuestionIndex]}";
                
                OnBattleMessage?.Invoke($"The interviewer asks a new question: \"{interviewQuestions[_currentQuestionIndex]}\"");
                yield return new WaitForSeconds(1.2f); 
            }

            // After all questions have been shown, end the player turn
            if (isBattleActive && turnManager.CurrentPhase == TurnPhase.PlayerTurn)
                turnManager.EndPlayerTurn();
        }

        //Determine question based on current doubt
        private int GetQuestionIndexFromDoubt(int currentDoubt)
        {
            float maxDoubt = interviewerDoubt.MaxDoubt;
            if (maxDoubt <= 0) return 0;

            //var fraction = (float)currentDoubt / (float)maxDoubt;
            
          
            float fraction = (float)currentDoubt / maxDoubt;

            int crossed = 0;
            for (int i = 0; i < questionThresholds.Length; i++)
            {
                if (fraction <= questionThresholds[i])
                    crossed++;
            }

            // The question index is the number of thresholds crossed, but capped to the last question
            return Mathf.Min(crossed, interviewQuestions.Count - 1);
        }
    
        //hand drawing sound effect
        private void OnHandDrawn()
        {
            if (cardDrawSound)
                SoundManager.Instance?.PlaySFX(cardDrawSound);
        }

    
        private void OnEnemyTurnStarted()
        {
            if (!isBattleActive) return;
            
            OnBattleMessage?.Invoke("Interviewer questions your experience...");
        
            // Enemy attacks
            StartCoroutine(EnemyTurnRoutine());
        }
    
        private IEnumerator EnemyTurnRoutine()
        {
            yield return new WaitForSeconds(2f);
        
            if (!isBattleActive) yield break;
        
            
            // Calculate damage to deal
            var damage = _currentRound switch
            {
                // Round 1
                0 => Random.Range(3, 6),
                // Round 2
                1 => Random.Range(5, 9),
                // Round 3 and above 
                _ => Random.Range(7, 13)
            };

            OnBattleMessage?.Invoke($" -{damage} Confidence");
        
            // Apply damage to player, play attack sound
            playerConfidence?.TakeDamage(damage);
            SoundManager.Instance?.PlaySFX(enemyAttackSound);
            
            //some camera shake
            playerShake?.Shake(10f, 0.25f);
        
            yield return new WaitForSeconds(2f);

            if (isBattleActive && playerConfidence && playerConfidence.CurrentConfidence > 0)
            {
                // increment round
                _currentRound++;
                
                
                if (_currentRound >= maxRounds)
                {

                    if (questionText)
                        questionText.text = "The interview is over";
                
                    OnBattleMessage?.Invoke("The interview is over, lets see if you convinced them! ...");
                
                    yield return new WaitForSeconds(0.5f); 
                
                    // Time's up, AND our interviewer still has doubt in our skills, player loses
                    if (interviewerDoubt && interviewerDoubt.CurrentDoubt > 0)
                    {
                        OnBattleMessage?.Invoke("The interviewer was not convinced.");
                        LoseInterview();
                        
                    }
              
                }
                
                
                turnManager?.EndEnemyTurn();
                
            }
               
            
           
            
            
        }

    
        public void EndPlayerTurn()
        {
            if (!isBattleActive) return;
            
            //clear any previous selections
            selectedHandIndices.Clear();
            OnSelectedHandChanged?.Invoke(new List<SkillCardData>());
        
            if (turnManager && turnManager.CurrentPhase == TurnPhase.PlayerTurn)
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
            
            //Gain XP
            experienceData?.AddExperience(50);
            
            //Win SFX
            SoundManager.Instance?.PlaySFX(winSound);
           
            //OnBattleMessage?.Invoke("Congratulations! The interviewer is impressed. You got the job!");
            
            OnInterviewEnded?.Invoke(InterviewEndReason.Won);
            
            /*
            if (questionText)
                questionText.text = "Congratulations! The interviewer is impressed. You got the job!";
                */
            
            
            
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
            
            //Gain XP
            experienceData?.AddExperience(50);
            
            //Losing SFX
            SoundManager.Instance?.PlaySFX(loseSound);
           
            //OnBattleMessage?.Invoke("You didn't get the job. Keep building your skills and try again!");
            
            
            var reason = (playerConfidence && playerConfidence.CurrentConfidence <= 0)
                ? InterviewEndReason.LostConfidence
                : InterviewEndReason.TimedOut;
            
            OnInterviewEnded?.Invoke(reason);
            
            
            /*
            if (questionText)
                questionText.text = "Unfortunately, you didn't get the job. Keep building your skills and try again.";
                */
            
            
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
            return isBattleActive && turnManager && turnManager.CurrentPhase == TurnPhase.PlayerTurn;
        }
    
        public bool IsBattleActive()
        {
            return isBattleActive;
        }
    
        public List<SkillCardData> GetCurrentHand()
        {
            return !deckManager ? new List<SkillCardData>() : deckManager.Hand;
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
            
            DeckDisplay.OnHandDrawn -=  OnHandDrawn;
            
        }
    
    
    }
}