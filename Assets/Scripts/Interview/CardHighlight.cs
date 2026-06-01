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

        private void Start()
        {
            baseColor = buttonHighlight.color;
        }


        public void Highlight(bool isHighlighted)
        {
            buttonHighlight.color = isHighlighted ? highlightColor : baseColor;
        }
        
        

        private void Update()
        {
            
        }
        
        
    }
    
}