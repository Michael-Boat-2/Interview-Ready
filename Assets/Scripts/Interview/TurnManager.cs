using UnityEngine;

namespace Interview
{
    
    
    public enum TurnPhase
    {
        PlayerTurn,
        EnemyTurn
    }
    
    
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private TurnPhase currentPhase = TurnPhase.PlayerTurn;
    
        // Events for other scripts to listen to
        public System.Action OnPlayerTurnStart;
        public System.Action OnEnemyTurnStart;
    
        public TurnPhase CurrentPhase => currentPhase;
    
        void Start()
        {
            // Start the game with player turn
            StartPlayerTurn();
        }
    
        // Call this when player ends their turn
        public void EndPlayerTurn()
        {
            if (currentPhase != TurnPhase.PlayerTurn) return;
        
            Debug.Log("Player ended turn. Starting enemy turn...");
            StartEnemyTurn();
        }
    
        // Call this when enemy ends their turn
        public void EndEnemyTurn()
        {
            if (currentPhase != TurnPhase.EnemyTurn) return;
        
            Debug.Log("Enemy ended turn. Starting player turn...");
            StartPlayerTurn();
        }
    
        private void StartPlayerTurn()
        {
            currentPhase = TurnPhase.PlayerTurn;
            OnPlayerTurnStart?.Invoke();
            Debug.Log("=== PLAYER TURN ===");
        }
    
        private void StartEnemyTurn()
        {
            currentPhase = TurnPhase.EnemyTurn;
            OnEnemyTurnStart?.Invoke();
            Debug.Log("=== ENEMY TURN ===");
        }
    }
}
