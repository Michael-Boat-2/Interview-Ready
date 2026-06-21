using System.Collections.Generic;
using Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using UnityEngine.Serialization;

namespace Interview
{
    public class CvDisplay : MonoBehaviour
    {
        
        [Header("Deck Data")]
        [SerializeField] private PlayerDeckData playerDeckData;
        
        [Header("UI")]
       
        //Panel game object
        [SerializeField] private GameObject cvPanel; 
        
        // Content object of ScrollView
        [SerializeField] private Transform contentParent;
        
        [SerializeField] private GameObject sectionPrefab;  
        // Prefab with a TextMeshProUGUI
        [SerializeField] private GameObject cardTextPrefab;
        
        [SerializeField] private Button toggleCvButton;
        [SerializeField]private bool isOpen = false;
        
        
        [Header("Type Colors")]
        [SerializeField] private Color technicalColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color softColor = new Color(0.2f, 0.8f, 0.4f);
        [SerializeField] private Color accessColor = new Color(0.9f, 0.6f, 0.2f);
     
        
        
        private void Start()
        {
            if (toggleCvButton)
                toggleCvButton.onClick.AddListener(ToggleCv);
            
            cvPanel.SetActive(false);
        }

        private void ToggleCv()
        {
            isOpen = !isOpen;
            cvPanel.SetActive(isOpen);
            if (isOpen)
                RefreshCv();
        }

        private void RefreshCv()
        {
            // Clear old entries
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            if (!playerDeckData) return;
            
            
            // Create sections
            CreateSection("Hard Skills", CardType.Technical, technicalColor);
            CreateSection("Soft Skills", CardType.Soft, softColor);
            CreateSection("Experience", CardType.Access, accessColor);
            
            /*foreach (var card in playerDeckData.ownedCards)
            {
                var go = Instantiate(cardTextPrefab, contentParent);
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp)
                    tmp.text = $"{card.cardName} ({card.cardType})";
            }*/
        }
        
        private void CreateSection(string title, CardType type, Color color)
        {
            // Instantiate the section objects
            var sectionObj = Instantiate(sectionPrefab, contentParent);
            
            var header = sectionObj.GetComponentInChildren<TextMeshProUGUI>();
            if (header)
            {
                header.text = title;
                header.color = color;
            }
            
            var cardContainer = sectionObj.transform.childCount > 1 ? sectionObj.transform.GetChild(1) : sectionObj.transform;

            // Add cards of this type
            foreach (var card in playerDeckData.ownedCards)
            {
                if (card.cardType != type) continue;

                var cardObj = Instantiate(cardTextPrefab, cardContainer);
                var cardTMP = cardObj.GetComponent<TextMeshProUGUI>();
                if (!cardTMP) continue;
                
                cardTMP.text = $"{card.cardName} (Value: {card.value})";
                cardTMP.color = color;
            }
        }
        
        
        
        
    }
}