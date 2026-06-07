using Managers;
using TMPro;
using UnityEngine;

namespace Interview
{
    public class BattleMessageDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private GameManager gameManager;

        private void Start()
        {
            if (!gameManager)
                gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager)
                gameManager.OnBattleMessage += DisplayMessage;
        }

        private void DisplayMessage(string msg)
        {
            if (messageText)
                messageText.text = msg;
        }

        private void OnDestroy()
        {
            if (gameManager)
                gameManager.OnBattleMessage -= DisplayMessage;
        }
    }
}