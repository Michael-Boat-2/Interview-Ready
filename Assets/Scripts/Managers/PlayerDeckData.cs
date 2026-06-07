using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "PlayerDeckData", menuName = "Game/Player Deck Data")]
    public class PlayerDeckData : ScriptableObject
    {
        public List<SkillCardData> ownedCards = new List<SkillCardData>();

        public void AddCard(SkillCardData card)
        {
            if (card != null && !ownedCards.Contains(card))
                ownedCards.Add(card);
        }

        public void RemoveCard(SkillCardData card)
        {
            if (ownedCards.Contains(card))
            {
                ownedCards.Remove(card);
            }
        }

        public void Clear()
        {
            ownedCards.Clear();
        }
    }
}