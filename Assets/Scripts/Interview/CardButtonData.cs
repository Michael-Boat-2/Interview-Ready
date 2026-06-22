using UnityEngine;
using Cards;
using UnityEngine.UI;
using TMPro;

namespace Interview
{
    
    
    // Stores the card reference on each button GameObject
    public class CardButtonData : MonoBehaviour
    {
        public Image cardBackground;   // assign in prefab
        public TextMeshProUGUI cardNameText; 
        
        public Image cardImage;
        
        public SkillCardData card;
        public void SetCard(SkillCardData newCard) => card = newCard;
    }
}