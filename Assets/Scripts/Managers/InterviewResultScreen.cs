using System;
using System.Linq;
using Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class InterviewResultScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PlayerDeckData playerDeckData;

        [Header("UI")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI outcomeText;
        [SerializeField] private TextMeshProUGUI analysisText;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button menuButton;

        private void Start()
        {
            if (resultPanel)
                resultPanel.SetActive(false);

            if (!gameManager)
                gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager)
                gameManager.OnInterviewEnded += ShowResult;

            if (returnButton)
                returnButton.onClick.AddListener(ReturnToEvents);
        }

        private void ShowResult(InterviewEndReason reason)
        {
            if (resultPanel)
                resultPanel.SetActive(true);
            
            switch (reason)
            {
                case InterviewEndReason.Won:
                    if (outcomeText) outcomeText.text = "Congratulations! You got the job!";
                    if(returnButton) returnButton.interactable = false;
                    if (menuButton) menuButton.interactable = true;
                    break;
                case InterviewEndReason.LostConfidence:
                    if (outcomeText) outcomeText.text = "You lost confidence during the interview.";
                    if(returnButton) returnButton.interactable = true;
                    if (menuButton) menuButton.interactable = true;
                    break;
                case InterviewEndReason.TimedOut:
                    if (outcomeText) outcomeText.text = "The interview ended, and the interviewer still had some doubts.";
                    if(returnButton) returnButton.interactable = true;
                    if (menuButton) menuButton.interactable = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }

            // Generate educational analysis based on deck composition
            if (analysisText && playerDeckData)
                analysisText.text = GenerateAnalysis(reason);
        }

        private string GenerateAnalysis(InterviewEndReason reason)
        {
            if (!playerDeckData) return "";

            // Count card types in the player's collection
            int tech = 0, soft = 0, access = 0;
            foreach (var card in playerDeckData.ownedCards.Where(card => card))
            {
                switch (card.cardType)
                {
                    case CardType.Technical: tech++; break;
                    case CardType.Soft: soft++; break;
                    case CardType.Access: access++; break;
                }
            }

            var analysis = $"Your CV Skills Tally: \n {tech} Hard Skills , {soft} Soft Skills , {access} Experience.\n\n";

            // General advice based on outcome
            if (reason == InterviewEndReason.Won)
            {
                analysis += "Well done! Your skill set impressed the interviewer.\n";
                if (soft < 2)
                    analysis += "Soft skills like Communication are valuable even after you land the job. Keep building them.";
            }
            else
            {
                switch (reason)
                {
                    // Loss feedback
                    case InterviewEndReason.LostConfidence:
                    {
                        analysis += "The interviewer's tough questions wore you down.\n";
                        if (soft < 2)
                            analysis += "Tip: Soft skills build Composure, which acts as a shield. Consider adding more Soft skills to handle pressure.";
                        break;
                    }
                    case InterviewEndReason.TimedOut:
                    {
                        analysis += "You didn't reduce the interviewer's doubt quickly enough.\n";
                        if (tech < 3)
                            analysis += "Tip: Hard skills deal direct damage to the interviewer's doubt. Build a strong set of Hard skill cards to convince them";
                        break;
                    }
                    
                    
                }

                analysis += "\nRemember: failure is part of the process. Keep learning and trying!";
            }

            return analysis;
        }

        private static void ReturnToEvents()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Events");
        }

        private void OnDestroy()
        {
            if (gameManager)
                gameManager.OnInterviewEnded -= ShowResult;
        }
    }
}