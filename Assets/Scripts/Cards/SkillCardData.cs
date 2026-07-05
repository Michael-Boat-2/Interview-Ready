using UnityEngine;

namespace Cards
{
    public enum CardType
    {
        Technical,   // Deals damage to interviewer's doubt
        Soft,        // Adds composure/shield for player
        Access       // Boosts other cards or provides unique effects
    }
    

    public enum SkillTag {Web, Data, Team, Project, Network, Communication, ProblemSolving, Leadership}

    
    [CreateAssetMenu(fileName = "SkillCardData", menuName = "Scriptable Objects/SkillCardData")]
    public class SkillCardData : ScriptableObject
    {
    
        //public enum SkillTag {Web, Data, Team, Project, Network, Communication, ProblemSolving, Leadership}
        

        [Header("Basic Info")]
        public string cardName;
        
        [Header("Description Text")]
        [TextArea(3, 5)]
        public string description;
    
        [Header("Card Type")]
        public CardType cardType;
    
        [Header("Values")]
        public int value;           // Damage for Technical, Shield for Soft, Boost for Access

        [Header("Synergy Tag")] 
        public SkillTag[] tags;
        
        [Header("Visuals")]
        public Sprite cardIcon;
        public Color cardColor = Color.white;
    
       
    }
}