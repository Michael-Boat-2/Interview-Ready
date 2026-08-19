using System.Collections.Generic;
using UnityEngine;
using Cards;
using System;
using Random = UnityEngine.Random;
using Managers;

namespace Interview
{
    public class DeckManager : MonoBehaviour
    {
        
        //public static DeckManager Instance{get; private set;}
        
        [Header("Player Deck Data")]
        [SerializeField] private PlayerDeckData playerDeckData;
        
        [Header("Deck Configuration")]
        //[SerializeField] private List<SkillCardData> allOwnedCards = new List<SkillCardData>();
        [SerializeField] private int startingHandSize = 5;
    
        [Header("Runtime")]
        [SerializeField] private List<SkillCardData> drawPile = new List<SkillCardData>();
        [SerializeField] private List<SkillCardData> hand = new List<SkillCardData>();
        [SerializeField] private List<SkillCardData> discardPile = new List<SkillCardData>();
    
        // Events
        public Action<List<SkillCardData>> OnHandChanged;
        public Action<int> OnDrawPileCountChanged;
        public Action<int> OnDiscardPileCountChanged;
        public Action OnDeckReshuffled;
    
        public List<SkillCardData> Hand => hand;
        public int HandCount => hand.Count;
        public int DrawPileCount => drawPile.Count;
        public int DiscardPileCount => discardPile.Count;




        // Set up the deck with player's collected cards
        public void SetupDeck(List<SkillCardData> ownedCards)
        {
            //allOwnedCards = new List<SkillCardData>(ownedCards);
            
            drawPile = new List<SkillCardData>(ownedCards);
            hand.Clear();
            discardPile.Clear();
        
            ShuffleDrawPile();
        
            Debug.Log($"Deck setup complete. Draw pile: {drawPile.Count} cards");
        }
    
        // Shuffle the draw pile
        public void ShuffleDrawPile()
        {
            for (int i = 0; i < drawPile.Count; i++)
            {
                SkillCardData temp = drawPile[i];
                int randomIndex = Random.Range(i, drawPile.Count);
                drawPile[i] = drawPile[randomIndex];
                drawPile[randomIndex] = temp;
            }
        
            OnDrawPileCountChanged?.Invoke(drawPile.Count);
            Debug.Log("Draw pile shuffled");
        }
    
        // Draw a single card
        public bool DrawCard()
        {
            // If no cards in draw pile, reshuffle discard pile
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    Debug.Log("No cards to draw!");
                    return false;
                }
            
                ReshuffleFromDiscard();
            }
        
            var drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(drawnCard);
        
            OnHandChanged?.Invoke(hand);
            OnDrawPileCountChanged?.Invoke(drawPile.Count);
        
            Debug.Log($"Drew card: {drawnCard.cardName}. Hand size: {hand.Count}");
            return true;
        }
    
        // Draw multiple cards
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!DrawCard())
                    break;
            }
        }
    
        // Draw starting hand
        public void DrawStartingHand()
        {
            for (int i = 0; i < startingHandSize; i++)
            {
                DrawCard();
            }
            Debug.Log($"Starting hand drawn: {hand.Count} cards");
        }
    
        // Play a card (move from hand to discard)
        public bool PlayCard(SkillCardData card)
        {
            if (!hand.Contains(card))
            {
                Debug.LogWarning("Card not in hand!");
                return false;
            }
        
            hand.Remove(card);
            discardPile.Add(card);
        
            OnHandChanged?.Invoke(hand);
            OnDiscardPileCountChanged?.Invoke(discardPile.Count);
        
            Debug.Log($"Played card: {card.cardName}. Moved to discard pile.");
            return true;
        }
    

        // Play a card by index
        // safe for duplicate cards since it uses RemoveAt
        public bool PlayCardAt(int index)
        {
            if (index < 0 || index >= hand.Count)
            {
                Debug.LogWarning("Invalid hand index!");
                return false;
            }

            var card = hand[index];
            hand.RemoveAt(index);
            discardPile.Add(card);

            OnHandChanged?.Invoke(hand);
            OnDiscardPileCountChanged?.Invoke(discardPile.Count);

            Debug.Log($"Played card at [{index}]: {card.cardName}. Moved to discard.");
            return true;
        }


        public bool DiscardCardAt(int index)
        {
            
            if (index < 0 || index >= hand.Count)
            {
                Debug.LogWarning("Invalid hand index!");
                return false;
            }

            var card = hand[index];
            hand.RemoveAt(index);
            discardPile.Add(card);

            OnHandChanged?.Invoke(hand);
            OnDiscardPileCountChanged?.Invoke(discardPile.Count);

            Debug.Log($"Discarded card at [{index}]: {card.cardName}. Now in Discard Pile.");
            return true;
            
        }
        
        
    
        // Reshuffle discard pile into draw pile
        private void ReshuffleFromDiscard()
        {
            drawPile = new List<SkillCardData>(discardPile);
            discardPile.Clear();
            ShuffleDrawPile();
        
            OnDiscardPileCountChanged?.Invoke(0);
            OnDeckReshuffled?.Invoke();
        
            Debug.Log("Reshuffled discard pile into draw pile");
        }


        //Add cards to owned cards
        public void AddCardToOwned(SkillCardData card)
        {
            if (playerDeckData)
                playerDeckData.AddCard(card);
            
            Debug.Log($"Adding card {card.cardName} to owned cards");
        }
        
    
        // Add a new card to the deck (when acquiring skills)
        /*public void AddCardToDeck(SkillCardData newCard)
        {
            allOwnedCards.Add(newCard);
            discardPile.Add(newCard);
        
            Debug.Log($"Added new card to deck: {newCard.cardName}");
        }*/
    
        
        // Remove a card from the deck
        public void RemoveCardFromDeck(SkillCardData card)
        {
            /*if (allOwnedCards.Contains(card))
                allOwnedCards.Remove(card);
                */
            
            playerDeckData.RemoveCard(card);
        
            if (drawPile.Contains(card))
                drawPile.Remove(card);
        
            if (hand.Contains(card))
                hand.Remove(card);
        
            if (discardPile.Contains(card))
                discardPile.Remove(card);
        
            OnHandChanged?.Invoke(hand);
            Debug.Log($"Removed card from deck: {card.cardName}");
        }
    
        // Reset for new interview (keeps cards but reshuffles)
        public void ResetForNewInterview()
        {
            //drawPile = new List<SkillCardData>(allOwnedCards);
            hand.Clear();
            discardPile.Clear();
            ShuffleDrawPile();
        
            OnHandChanged?.Invoke(hand);
            OnDrawPileCountChanged?.Invoke(drawPile.Count);
            OnDiscardPileCountChanged?.Invoke(0);
        
            Debug.Log("Deck reset for new interview");
        }
    
        // Get all owned cards (for save/display)
        public List<SkillCardData> GetAllOwnedCards()
        {
            return playerDeckData ? playerDeckData.ownedCards : new List<SkillCardData>();
        }
    }
}