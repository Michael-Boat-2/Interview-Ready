using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cards;
using Interview;
using TMPro;
using System.Linq;

namespace Managers
{
    public class EventSelectionManager : MonoBehaviour
    {
        
        [Header("UI References")]
        [SerializeField] private Button[] eventButtons; 
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button interviewButton;
        
        [Header("Data")]
        [SerializeField] private PlayerDeckData playerDeckData;
        [SerializeField] private EventData[] allEvents;
        private EventData[] currentEvents;
        
        [SerializeField] private CvDisplay cvDisplay;
        
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            
            if (refreshButton)
                refreshButton.onClick.AddListener(RefreshEvents);

            if (interviewButton)
                interviewButton.onClick.AddListener(GoToInterview);
            
            RefreshEvents();
        
        }
        
        
        private void RefreshEvents()
        {
            //select events randomly
            currentEvents = allEvents.OrderBy(_ => Random.value)
                .Take(eventButtons.Length)
                .ToArray();

            //Update button options
            for (var i = 0; i < eventButtons.Length; i++)
            {
                if (i < currentEvents.Length)
                {
                    var ev = currentEvents[i];
                    eventButtons[i].gameObject.SetActive(true);
                    eventButtons[i].interactable = true;

                    var btnText = eventButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    
                    if (btnText)
                        btnText.text = $"{ev.eventName}";

                    //Remove old listeners and add new one
                    eventButtons[i].onClick.RemoveAllListeners();
                    
                    var index = i;  
                    eventButtons[i].onClick.AddListener(() => OnEventClicked(index));
                }
                
                else
                {
                    eventButtons[i].gameObject.SetActive(false);
                }
            }

            if (feedbackText)
                feedbackText.text = "Choose an event to gain a skill.";
        }



        private void OnEventClicked(int buttonIndex)
        {
            if (currentEvents == null || buttonIndex >= currentEvents.Length)
                return;

            var selectedEvent = currentEvents[buttonIndex];
            if (!selectedEvent) return;

            // Add the card reward
            if (playerDeckData && selectedEvent.cardReward)
            {
                playerDeckData.AddCard(selectedEvent.cardReward);
                if (feedbackText)
                    feedbackText.text = $"{selectedEvent.description}" +
                                        $" \n \n Added {selectedEvent.cardReward.cardName} to your CV!";
                
                cvDisplay?.RefreshCv();
            }

            // Disable the button after claiming
            eventButtons[buttonIndex].interactable = false;
            currentEvents[buttonIndex] = null;   // mark as taken
            
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
