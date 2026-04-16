using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "PlayerConfidence", menuName = "Game/Player Confidence")]
    public class PlayerConfidence : ScriptableObject
    {
        [Header("Current Values")]
        [SerializeField] private int currentConfidence;
    
        [Header("Starting Values")]
        [SerializeField] private int maxConfidence = 30;
        [SerializeField] private int startingConfidence = 20;
    
        [Header("Composure (Shield)")]
        [SerializeField] private int currentComposure;
    
        // Events for UI and other systems
        public System.Action OnConfidenceChanged;
        public System.Action OnComposureChanged;
        public System.Action OnPlayerDied;
    
        public int CurrentConfidence => currentConfidence;
        public int MaxConfidence => maxConfidence;
        public int CurrentComposure => currentComposure;
        public float ConfidencePercentage => (float)currentConfidence / maxConfidence;
    
        // Initialize or reset the player
        public void Initialize()
        {
            currentConfidence = startingConfidence;
            currentComposure = 0;
            OnConfidenceChanged?.Invoke();
            OnComposureChanged?.Invoke();
            Debug.Log($"Confidence initialized: {currentConfidence}/{maxConfidence}");
        }
    
        // Add composure (shield)
        public void AddComposure(int amount)
        {
            if (amount <= 0) return;
        
            currentComposure += amount;
            OnComposureChanged?.Invoke();
            Debug.Log($"Composure +{amount}. Total: {currentComposure}");
        }
    
        // Take damage (hits composure first, then confidence)
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;
        
            int remainingDamage = damage;
        
            // Damage composure first
            if (currentComposure > 0)
            {
                int composureAbsorb = Mathf.Min(currentComposure, remainingDamage);
                currentComposure -= composureAbsorb;
                remainingDamage -= composureAbsorb;
                OnComposureChanged?.Invoke();
                Debug.Log($"Composure absorbed {composureAbsorb} damage. Remaining: {currentComposure}");
            }
        
            // Remaining damage goes to confidence
            if (remainingDamage > 0)
            {
                currentConfidence -= remainingDamage;
                OnConfidenceChanged?.Invoke();
                Debug.Log($"Confidence -{remainingDamage}. Current: {currentConfidence}/{maxConfidence}");
            }
        
            // Check for death
            if (currentConfidence <= 0)
            {
                currentConfidence = 0;
                OnPlayerDied?.Invoke();
                Debug.Log("PLAYER HAS LOST ALL CONFIDENCE!");
            }
        }
    
        // Heal confidence (rare, but possible)
        public void HealConfidence(int amount)
        {
            if (amount <= 0) return;
        
            currentConfidence = Mathf.Min(maxConfidence, currentConfidence + amount);
            OnConfidenceChanged?.Invoke();
            Debug.Log($"Confidence +{amount}. Current: {currentConfidence}/{maxConfidence}");
        }
    
        // Reset for a new interview
        public void ResetForNewInterview()
        {
            currentConfidence = startingConfidence;
            currentComposure = 0;
            OnConfidenceChanged?.Invoke();
            OnComposureChanged?.Invoke();
            Debug.Log($"Reset for interview. Confidence: {currentConfidence}/{maxConfidence}");
        }
    
        // Manual override (for debugging)
        public void SetConfidence(int value)
        {
            currentConfidence = Mathf.Clamp(value, 0, maxConfidence);
            OnConfidenceChanged?.Invoke();
        }
    }
}