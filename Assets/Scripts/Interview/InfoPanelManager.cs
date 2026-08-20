using Cards;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interview
{
    public class InfoPanelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private PlayerDeckData playerDeckData;

        [Header("UI")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI drawCountText;
        [SerializeField] private TextMeshProUGUI discardCountText;

        [Header("CV Section")]
        [SerializeField] private Transform contentParent;
        
        [SerializeField] private GameObject sectionPrefab;  
        // Prefab with a TextMeshProUGUI
        [SerializeField] private GameObject cardTextPrefab;
        
        [Header("Player Profile")]
        [SerializeField] private PlayerProfileData profileData;
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        
        
        [Header("Type Colors")]
        [SerializeField] private Color technicalColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color softColor = new Color(0.2f, 0.8f, 0.4f);
        [SerializeField] private Color accessColor = new Color(0.9f, 0.6f, 0.2f);

        private bool isOpen = false;

        private void Start()
        {
            if (infoPanel)
                infoPanel.SetActive(false);

            if (!deckManager)
                deckManager = FindFirstObjectByType<DeckManager>();

            if (!deckManager) return;
            deckManager.OnDrawPileCountChanged += UpdateDrawCount;
            deckManager.OnDiscardPileCountChanged += UpdateDiscardCount;
            UpdateDrawCount(deckManager.DrawPileCount);
            UpdateDiscardCount(deckManager.DiscardPileCount);
            
            
            if (playerNameLabel && profileData)
                playerNameLabel.text = profileData.playerName + "'s CV";
            
            
        }

        public void TogglePanel()
        {
            isOpen = !isOpen;
            infoPanel.SetActive(isOpen);

            if (!isOpen) return;
            RefreshInfo();
            // Ensure latest counts
            if (!deckManager) return;
            UpdateDrawCount(deckManager.DrawPileCount);
            UpdateDiscardCount(deckManager.DiscardPileCount);
        }

        private void UpdateDrawCount(int count)
        {
            if (drawCountText)
                drawCountText.text = $"Draw pile: {count}";
        }

        private void UpdateDiscardCount(int count)
        {
            if (discardCountText)
                discardCountText.text = $"Discard pile: {count}";
        }

     

        private void RefreshInfo()
        {
            // Clear old entries
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            if (!playerDeckData) return;

            CreateSection("Hard Skills", CardType.Technical, technicalColor);
            CreateSection("Soft Skills", CardType.Soft, softColor);
            CreateSection("Experience", CardType.Access, accessColor);

            // Force layout rebuild to prevent overlapping
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(infoPanel.GetComponent<RectTransform>());
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
                
                cardTMP.text = $"{card.cardName} ({card.value})";
                cardTMP.color = color;
            }
        }
   
      

        private void OnDestroy()
        {
            if (!deckManager) return;
            deckManager.OnDrawPileCountChanged -= UpdateDrawCount;
            deckManager.OnDiscardPileCountChanged -= UpdateDiscardCount;
        }
    }
}