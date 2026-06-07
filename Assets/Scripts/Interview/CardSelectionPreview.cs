using System.Collections.Generic;
using Cards;
using Managers;
using TMPro;
using UnityEngine;

namespace Interview
{
    public class CardSelectionPreview : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI composureText;

        private void Start()
        {
            if (!gameManager) 
                gameManager = FindFirstObjectByType<GameManager>();
            
            if(gameManager)
                gameManager.OnSelectedHandChanged += UpdatePreview;
            
        }

        private void UpdatePreview(List<SkillCardData> selectedCards)
        {
            if (selectedCards.Count == 0)
            {
                if (damageText) damageText.text = "Damage: 0";
                if (composureText) composureText.text = "Composure: 0";
                return;
            }

            gameManager.CalculatePlayPreview(selectedCards, out var dmg, out var comp);
            if (damageText) damageText.text = $"Damage: {dmg}";
            if (composureText) composureText.text = $"Composure: +{comp}";
        }

        private void OnDestroy()
        {
            if (gameManager)
                gameManager.OnSelectedHandChanged -= UpdatePreview;
        }
    }
}