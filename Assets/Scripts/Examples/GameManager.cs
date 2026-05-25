using System.Collections;
using System.Collections.Generic;
using Cards;
using Interview;
using Managers;
using UnityEngine;

namespace Examples
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private PlayerConfidence playerConfidence;
        [SerializeField] private InterviewerDoubt interviewerDoubt;

        [Header("Enemy Settings")]
        [SerializeField] private int enemyMinDamage = 3;
        [SerializeField] private int enemyMaxDamage = 7;

        [Header("Hand Settings")]
        [SerializeField] private int maxSelectedCards = 5;

        [Header("Test Cards")]
        [SerializeField] private List<SkillCardData> testStartingDeck = new List<SkillCardData>();

        [Header("Auto Start")]
        [SerializeField] private bool autoStart = true;

        // Events for UI
        public System.Action OnInterviewStarted;
        public System.Action OnInterviewWon;
        public System.Action OnInterviewLost;
        public System.Action<string> OnBattleMessage;

        // Fired whenever the selected hand changes — UI listens to this to highlight cards
        public System.Action<List<SkillCardData>> OnSelectedHandChanged;

        private List<SkillCardData> selectedHand = new List<SkillCardData>();
        private bool isBattleActive = false;

        void Start()
        {
            if (turnManager == null)
                turnManager = FindObjectOfType<TurnManager>();

            if (deckManager == null)
            {
                deckManager = FindObjectOfType<DeckManager>();
                testStartingDeck = deckManager.GetAllOwnedCards();
            }

            if (playerConfidence == null)
                playerConfidence = Resources.Load<PlayerConfidence>("PlayerConfidence");

            if (interviewerDoubt == null)
                interviewerDoubt = Resources.Load<InterviewerDoubt>("InterviewerDoubt");

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
                StartInterview();
        }

        // ─────────────────────────────────────────────
        //  Interview flow
        // ─────────────────────────────────────────────

        public void StartInterview()
        {
            Debug.Log("=== INTERVIEW BEGINS ===");
            OnBattleMessage?.Invoke("Interview starts! Prove your skills!");

            playerConfidence?.ResetForNewInterview();
            interviewerDoubt?.ResetForNewInterview();

            selectedHand.Clear();
            OnSelectedHandChanged?.Invoke(selectedHand);

            if (deckManager != null && testStartingDeck.Count > 0)
            {
                deckManager.SetupDeck(testStartingDeck);
                deckManager.DrawStartingHand();
            }
            else
            {
                Debug.LogWarning("No test cards assigned! Add cards to the inspector or create ScriptableObjects.");
            }

            isBattleActive = true;
            OnInterviewStarted?.Invoke();

            turnManager?.StartPlayerTurn();
        }

        // ─────────────────────────────────────────────
        //  Card selection (called by DeckDisplay)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Toggle a card in or out of the selected hand.
        /// Returns true if the card is now selected, false if deselected or rejected.
        /// </summary>
        public bool ToggleCardSelection(SkillCardData card)
        {
            if (!isBattleActive || turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                OnBattleMessage?.Invoke("It's not your turn!");
                return false;
            }

            if (!deckManager.Hand.Contains(card))
            {
                Debug.LogWarning("Tried to select a card that isn't in hand.");
                return false;
            }

            if (selectedHand.Contains(card))
            {
                // Deselect
                selectedHand.Remove(card);
                OnSelectedHandChanged?.Invoke(new List<SkillCardData>(selectedHand));
                OnBattleMessage?.Invoke($"{card.cardName} removed from selection.");
                return false;
            }
            else
            {
                if (selectedHand.Count >= maxSelectedCards)
                {
                    OnBattleMessage?.Invoke($"You can only select up to {maxSelectedCards} cards!");
                    return false;
                }

                selectedHand.Add(card);
                OnSelectedHandChanged?.Invoke(new List<SkillCardData>(selectedHand));
                OnBattleMessage?.Invoke($"{card.cardName} added to selection. ({selectedHand.Count}/{maxSelectedCards})");
                return true;
            }
        }

        public bool IsCardSelected(SkillCardData card) => selectedHand.Contains(card);
        public int SelectedCount => selectedHand.Count;
        public int MaxSelectedCards => maxSelectedCards;

        // ─────────────────────────────────────────────
        //  Play the selected hand (called by UI button)
        // ─────────────────────────────────────────────

        public void PlaySelectedHand()
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

            if (selectedHand.Count == 0)
            {
                OnBattleMessage?.Invoke("Select at least one card to play!");
                return;
            }

            // Total up all effects
            int totalDoubtDamage = 0;
            int totalComposureGain = 0;
            List<string> playedNames = new List<string>();

            foreach (SkillCardData card in selectedHand)
            {
                playedNames.Add(card.cardName);
                AccumulateCardEffect(card, ref totalDoubtDamage, ref totalComposureGain);
            }

            // Apply totalled effects
            string summary = $"Played: {string.Join(", ", playedNames)}";

            if (totalDoubtDamage > 0)
            {
                interviewerDoubt?.ReduceDoubt(totalDoubtDamage);
                summary += $"  |  -{totalDoubtDamage} Interviewer Doubt";
            }

            if (totalComposureGain > 0)
            {
                playerConfidence?.AddComposure(totalComposureGain);
                summary += $"  |  +{totalComposureGain} Composure";
            }

            OnBattleMessage?.Invoke(summary);
            Debug.Log(summary);

            // Discard played cards
            foreach (SkillCardData card in selectedHand)
                deckManager.PlayCard(card);

            selectedHand.Clear();
            OnSelectedHandChanged?.Invoke(selectedHand);

            CheckBattleState();
        }

        /// <summary>
        /// Accumulates a single card's contribution toward the hand totals.
        /// Technical cards deal direct doubt damage.
        /// Soft cards give composure but also contribute partial doubt damage (soft skills still impress).
        /// Access cards boost both slightly.
        /// </summary>
        private void AccumulateCardEffect(SkillCardData card, ref int doubtDamage, ref int composureGain)
        {
            switch (card.cardType)
            {
                case CardType.Technical:
                    doubtDamage += card.value;
                    break;

                case CardType.Soft:
                    composureGain += card.value;
                    doubtDamage += Mathf.Max(1, card.value / 2); // Soft skills still chip away at doubt
                    break;

                case CardType.Access:
                    // Networking / access cards split the value between both stats
                    doubtDamage += Mathf.CeilToInt(card.value * 0.6f);
                    composureGain += Mathf.FloorToInt(card.value * 0.4f);
                    break;
            }
        }

        // ─────────────────────────────────────────────
        //  Turn events
        // ─────────────────────────────────────────────

        private void OnPlayerTurnStarted()
        {
            if (!isBattleActive) return;

            selectedHand.Clear();
            OnSelectedHandChanged?.Invoke(selectedHand);

            Debug.Log("Your turn — select up to 5 cards and play your hand!");
            OnBattleMessage?.Invoke("Your turn. Build your hand, then play it.");
        }

        private void OnEnemyTurnStarted()
        {
            if (!isBattleActive) return;

            Debug.Log("Interviewer's turn — challenging your answers!");
            OnBattleMessage?.Invoke("The interviewer scrutinises your responses...");

            StartCoroutine(EnemyTurnRoutine());
        }

        private IEnumerator EnemyTurnRoutine()
        {
            yield return new WaitForSeconds(0.8f);

            if (!isBattleActive) yield break;

            int damage = Random.Range(enemyMinDamage, enemyMaxDamage + 1);
            OnBattleMessage?.Invoke($"Tough question! -{damage} Confidence");
            Debug.Log($"Interviewer challenges for {damage} damage");

            playerConfidence?.TakeDamage(damage);

            yield return new WaitForSeconds(0.4f);

            if (!isBattleActive || playerConfidence.CurrentConfidence <= 0) yield break;

            // Draw back up to hand size for next turn
            while (deckManager != null && deckManager.HandCount < 5)
                deckManager.DrawCard();

            turnManager?.EndEnemyTurn();
        }

        public void EndPlayerTurn()
        {
            if (!isBattleActive) return;

            if (turnManager != null && turnManager.CurrentPhase == TurnPhase.PlayerTurn)
            {
                selectedHand.Clear();
                OnSelectedHandChanged?.Invoke(selectedHand);
                turnManager.EndPlayerTurn();
            }
        }

        // ─────────────────────────────────────────────
        //  Win / loss
        // ─────────────────────────────────────────────

        private void CheckBattleState()
        {
            if (interviewerDoubt != null && interviewerDoubt.CurrentDoubt <= 0)
                WinInterview();
            else if (playerConfidence != null && playerConfidence.CurrentConfidence <= 0)
                LoseInterview();
        }

        private void OnInterviewerDefeated() => WinInterview();
        private void OnPlayerDied() => LoseInterview();

        private void WinInterview()
        {
            if (!isBattleActive) return;
            isBattleActive = false;

            Debug.Log("=== YOU GOT THE JOB! ===");
            OnBattleMessage?.Invoke("The interviewer is impressed. You got the job!");
            OnInterviewWon?.Invoke();
        }

        private void LoseInterview()
        {
            if (!isBattleActive) return;
            isBattleActive = false;

            Debug.Log("=== YOU DIDN'T GET THE JOB ===");
            OnBattleMessage?.Invoke("You didn't get the job this time. Keep building those skills!");
            OnInterviewLost?.Invoke();
        }

        // ─────────────────────────────────────────────
        //  Helpers for UI
        // ─────────────────────────────────────────────

        public bool IsPlayerTurn() => isBattleActive && turnManager != null && turnManager.CurrentPhase == TurnPhase.PlayerTurn;
        public bool IsBattleActive() => isBattleActive;
        public List<SkillCardData> GetCurrentHand() => deckManager == null ? new List<SkillCardData>() : deckManager.Hand;

        void OnDestroy()
        {
            if (turnManager != null)
            {
                turnManager.OnPlayerTurnStart -= OnPlayerTurnStarted;
                turnManager.OnEnemyTurnStart -= OnEnemyTurnStarted;
            }

            if (playerConfidence != null)
                playerConfidence.OnPlayerDied -= OnPlayerDied;

            if (interviewerDoubt != null)
                interviewerDoubt.OnInterviewerDefeated -= OnInterviewerDefeated;
        }
    }
}
