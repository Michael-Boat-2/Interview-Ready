using System;
using UnityEngine;
using UnityEngine.UI;

namespace Interview
{
    public class CardHighlight : MonoBehaviour
    {
        
        //target to change
        [SerializeField]private Image buttonHighlight;
    
        [SerializeField] private Color baseColor;
        [SerializeField] private Color highlightColor;
        
        private CardAnimator _cardAnimator;

        private void Start()
        {
            baseColor = buttonHighlight.color;
            _cardAnimator = GetComponent<CardAnimator>();
        }


        public void Highlight(bool isHighlighted)
        {
            buttonHighlight.color = isHighlighted ? highlightColor : baseColor;
            
            if(isHighlighted)
                _cardAnimator?.PlaySelectionBounce();
            
        }
        
        

        private void Update()
        {
            
        }
        
        
    }
    
}