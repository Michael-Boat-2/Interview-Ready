using UnityEngine;
using Cards;
using UnityEngine.UI;

namespace Interview
{
    
    
    // Stores the card reference on each button GameObject
    public class CardButtonData : MonoBehaviour
    {
        
        public Image cardImage;
        
        public SkillCardData card;
        public void SetCard(SkillCardData newCard) => card = newCard;
    }
}