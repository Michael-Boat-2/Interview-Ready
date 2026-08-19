using System.Collections.Generic;
using UnityEngine;
using Cards;

namespace Managers
{
    public class NewGameReset : MonoBehaviour
    {
        [Header("Data Assets")]
        public PlayerDeckData playerDeckData;
        public PostProgressData postProgressData;
        public PlayerExperienceData playerExperienceData;
        
        [Header("Starting Deck")]
        [SerializeField] private List<SkillCardData> startingDeck = new List<SkillCardData>();
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
            // Reset owned cards to starting deck
            if (playerDeckData)
            {
                playerDeckData.ownedCards.Clear();
                playerDeckData.ownedCards.AddRange(startingDeck);
            }
            
            
            //make deck reset for owned cards to some arbitrary starting deck
            //playerDeckData?.ownedCards.Clear();       
            postProgressData?.ResetProgress();       // reset tips
            playerExperienceData?.ResetLevel();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
