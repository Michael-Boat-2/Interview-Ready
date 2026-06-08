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
        //Panel game object
        [SerializeField] private GameObject cvPanel;
        // Content object of ScrollView
        [SerializeField] private Transform contentParent;
        // Prefab with a TextMeshProUGUI
        [SerializeField] private GameObject cardTextPrefab;
        [SerializeField] private Button toggleCvButton;
        
        [SerializeField] private PlayerDeckData playerDeckData;
        [SerializeField]private bool isOpen = false;

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
            
            foreach (var card in playerDeckData.ownedCards)
            {
                var go = Instantiate(cardTextPrefab, contentParent);
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp)
                    tmp.text = $"{card.cardName} ({card.cardType})";
            }
        }
    }
}