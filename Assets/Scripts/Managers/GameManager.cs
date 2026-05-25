using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Interview;
using Cards;
using Managers;



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
    
      // Events for UI
    public System.Action OnInterviewStarted;
    public System.Action OnInterviewWon;
    public System.Action OnInterviewLost;
    public System.Action<string> OnBattleMessage;
    
    
    // Fired whenever the selected hand changes — UI listens to this to highlight cards
    public System.Action<List<SkillCardData>> OnSelectedHandChanged;

    private List<SkillCardData> selectedHand = new List<SkillCardData>();
    
    
    private bool isBattleActive = false;
    
    //autostart
    [SerializeField]private bool autoStart = true;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find references if not assigned
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
    void Update()
    {
        
    }
    
    
    public void StartInterview()
    {
        Debug.Log("Interview BEGINS");
        OnBattleMessage?.Invoke("Interview starts!");
        
        // Reset all systems
        playerConfidence?.ResetForNewInterview();
        interviewerDoubt?.ResetForNewInterview();
        
        // Setup deck with test cards
        if (deckManager != null && testStartingDeck.Count > 0)
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
        
        // Draw new cards for next turn
        yield return new WaitForSeconds(0.3f);
        
        if (isBattleActive && playerConfidence.CurrentConfidence > 0)
        {
            // Draw back to hand size (5 cards)
            while (deckManager != null && deckManager.HandCount < 5)
            {
                deckManager.DrawCard();
            }
            
            // End enemy turn (goes back to player)
            turnManager?.EndEnemyTurn();
        }
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
        if (interviewerDoubt != null && interviewerDoubt.CurrentDoubt <= 0)
        {
            WinInterview();
        }
        else if (playerConfidence != null && playerConfidence.CurrentConfidence <= 0)
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
        Debug.Log("YOU GOT THE JIB");
        OnBattleMessage?.Invoke("Congratulations! The interviewer is impressed. You got the job!");
        OnInterviewWon?.Invoke();
        
        // Disable further turn actions
        if (turnManager != null)
        {
            // You might want to disable turn switching here
        }
    }
    
    private void LoseInterview()
    {
        if (!isBattleActive) return;
        
        isBattleActive = false;
        Debug.Log("INTERVIEWER PASSES");
        OnBattleMessage?.Invoke("You didn't get the job. Keep building your skills and try again!");
        OnInterviewLost?.Invoke();
        
        // Disable further turn actions
        if (turnManager != null)
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



    
 
    

   
    
 
