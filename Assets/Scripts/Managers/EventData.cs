using Cards;
using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "EventData", menuName = "Game/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("Display")]
        public string eventName;
        [TextArea(2, 4)]
        public string description;      

        [Header("Reward")]
        public SkillCardData cardReward;
    }
}