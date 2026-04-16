using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "InterviewerDoubt", menuName = "Game/Interviewer Doubt")]
    public class InterviewerDoubt : ScriptableObject
    {
        [Header("Current Values")]
        [SerializeField] private int currentDoubt;
    
        [Header("Starting Values")]
        [SerializeField] private int maxDoubt = 30;
        [SerializeField] private int startingDoubt = 30;
    
        // Events for UI and other systems
        public System.Action OnDoubtChanged;
        public System.Action OnInterviewerDefeated;
    
        public int CurrentDoubt => currentDoubt;
        public int MaxDoubt => maxDoubt;
        public float DoubtPercentage => (float)currentDoubt / maxDoubt;
    
        // Initialize or reset the interviewer
        public void Initialize()
        {
            currentDoubt = startingDoubt;
            OnDoubtChanged?.Invoke();
            Debug.Log($"Interviewer doubt initialized: {currentDoubt}/{maxDoubt}");
        }
    
        // Reduce doubt (take damage from player skills)
        public void ReduceDoubt(int amount)
        {
            if (amount <= 0) return;
        
            currentDoubt = Mathf.Max(0, currentDoubt - amount);
            OnDoubtChanged?.Invoke();
            Debug.Log($"Doubt -{amount}. Current: {currentDoubt}/{maxDoubt}");
        
            // Check if defeated
            if (currentDoubt <= 0)
            {
                OnInterviewerDefeated?.Invoke();
                Debug.Log("INTERVIEWER DOUBT ELIMINATED - JOB OFFERED!");
            }
        }
    
        // Increase doubt (if player performs poorly, though rare)
        public void IncreaseDoubt(int amount)
        {
            if (amount <= 0) return;
        
            currentDoubt = Mathf.Min(maxDoubt, currentDoubt + amount);
            OnDoubtChanged?.Invoke();
            Debug.Log($"Doubt +{amount}. Current: {currentDoubt}/{maxDoubt}");
        }
    
        // Reset for a new interview
        public void ResetForNewInterview()
        {
            currentDoubt = startingDoubt;
            OnDoubtChanged?.Invoke();
            Debug.Log($"Reset for new interview. Doubt: {currentDoubt}/{maxDoubt}");
        }
    
        // Manual override (for debugging)
        public void SetDoubt(int value)
        {
            currentDoubt = Mathf.Clamp(value, 0, maxDoubt);
            OnDoubtChanged?.Invoke();
        }
    
        // Check if defeated without triggering event
        public bool IsDefeated()
        {
            return currentDoubt <= 0;
        }
    }
}