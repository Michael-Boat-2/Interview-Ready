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
    
      
    [Header("Test Cards")]
    [SerializeField] private List<SkillCardData> testStartingDeck = new List<SkillCardData>();
    
      // Events for UI
    public System.Action OnInterviewStarted;
    public System.Action OnInterviewWon;
    public System.Action OnInterviewLost;
    public System.Action<string> OnBattleMessage;
    
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
        Debug.Log("=== INTERVIEW BEGINS ===");
        OnBattleMessage?.Invoke("Interview starts! Prove your skills!");
        
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
    
    
    
    private void OnPlayerTurnStarted()
    {
        if (!isBattleActive) return;
        
        Debug.Log("Your turn - Play skill cards!");
        OnBattleMessage?.Invoke("Your turn. Choose a skill to demonstrate.");
        
        // UI should enable card buttons here
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
    
    public void PlayCard(SkillCardData card)
    {
        if (!isBattleActive)
        {
            Debug.Log("No active interview!");
            return;
        }
        
        if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
        {
            Debug.Log("Not your turn!");
            OnBattleMessage?.Invoke("Wait for your turn!");
            return;
        }
        
        if (deckManager == null)
        {
            Debug.LogError("DeckManager missing!");
            return;
        }
        
        // Check if card is in hand
        if (!deckManager.Hand.Contains(card))
        {
            Debug.Log("Card not in hand!");
            return;
        }
        
        // Apply card effect based on type
        string effectMessage = ApplyCardEffect(card);
        OnBattleMessage?.Invoke(effectMessage);
        
        // Move card to discard
        deckManager.PlayCard(card);
        
        // Check win/loss conditions after card play
        CheckBattleState();
        
        // If battle still active and it's still player turn, continue
        // Player can play multiple cards per turn
    }
    
    private string ApplyCardEffect(SkillCardData card)
    {
        switch (card.cardType)
        {
            case CardType.Technical:
                int damage = card.value;
                interviewerDoubt?.ReduceDoubt(damage);
                return $"Used {card.cardName}! Dealt {damage} damage to interviewer's doubt!";
                
            case CardType.Soft:
                int shield = card.value;
                playerConfidence?.AddComposure(shield);
                return $"Used {card.cardName}! Gained {shield} composure!";
                
            case CardType.Access:
                int boost = card.value;
                playerConfidence?.AddComposure(boost / 2);
                return $"Used {card.cardName}! Networking pays off! Gained {boost / 2} composure.";
                
            default:
                return $"Used {card.cardName}! No effect?";
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
        Debug.Log("=== YOU GOT THE JOB! ===");
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
        Debug.Log("=== YOU DIDN'T GET THE JOB ===");
        OnBattleMessage?.Invoke("You didn't get the job. Keep building your skills and try again!");
        OnInterviewLost?.Invoke();
        
        // Disable further turn actions
        if (turnManager != null)
        {
            // You might want to disable turn switching here
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



    
 
    

   
    
 
