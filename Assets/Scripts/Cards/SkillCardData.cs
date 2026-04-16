using UnityEngine;

namespace Cards
{
    public enum CardType
    {
        Technical,   // Deals damage to interviewer's doubt
        Soft,        // Adds composure/shield for player
        Access       // Boosts other cards or provides unique effects
    }
    

    [CreateAssetMenu(fileName = "SkillCardData", menuName = "Scriptable Objects/SkillCardData")]
    public class SkillCardData : ScriptableObject
    {
    

        [Header("Basic Info")]
        public string cardName;
        public string description;
    
        [Header("Card Type")]
        public CardType cardType;
    
        [Header("Values")]
        public int value;           // Damage for Technical, Shield for Soft, Boost for Access
    
        [Header("Visuals")]
        public Sprite cardIcon;
        public Color cardColor = Color.white;
    
        [Header("Flavor")]
        [TextArea(2, 3)]
        public string flavorText;
    }
}