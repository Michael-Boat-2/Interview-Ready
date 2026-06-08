using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cards;
using TMPro;

namespace Managers
{
    public class EventSelectionManager : MonoBehaviour
    {
        [Header("Player Deck Data")]
        [SerializeField] private PlayerDeckData playerDeckData;
        
        [Header("UI Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button interviewButton;
        
        
        //Event buttons linked to cards you'll earn as a reward
        [System.Serializable]
        public struct EventOption
        {
            public Button button;
            public SkillCardData cardReward;
        }
        
        [SerializeField] private EventOption[] events;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            
            foreach (var ev in events)
            {
                if (ev.button && ev.cardReward)
                {
                    SkillCardData card = ev.cardReward; // capture for closure
                    ev.button.onClick.AddListener(() => OnEventClicked(card));
                }
            }

            if (interviewButton)
                interviewButton.onClick.AddListener(GoToInterview);
        
        }


        private void OnEventClicked(SkillCardData card)
        {
            if (!playerDeckData)
            {
                Debug.LogError("DeckManager not found! Make sure it's a persistent singleton.");
                return;
            }

            playerDeckData.AddCard(card);
            
            if (feedbackText)
                feedbackText.text = $"Added {card.cardName} to your CV!";
            
            Debug.Log($"Acquired card: {card.cardName}");
            
        }


        private void GoToInterview()
        {
            // Ensure we have at least 5 cards, or warn
            if (playerDeckData  && playerDeckData.ownedCards.Count < 5)
            {
                if (feedbackText)
                    feedbackText.text = "Collect at least 5 skills before applying!";
                return;
            }
            
            
            //Load interview 
            SceneManager.LoadScene("Interview"); 
        }

        
  
    }
}
