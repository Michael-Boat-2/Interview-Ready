using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DialogueData dialogueData;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image characterImage;
       
        //press to continue indicator
        [SerializeField] private GameObject continuePrompt;  

        [Header("Background Panels")]
        [SerializeField] private GameObject cvPanel;
        [SerializeField] private GameObject eventsPanel;
        [SerializeField] private GameObject defaultBackground; 

        private int currentIndex = -1;

        private void Start()
        {
            if (!dialogueData)
            {
                Debug.LogError("DialogueData null");
                return;
            }

            //set static info
            if (speakerNameText) speakerNameText.text = dialogueData.speakerName;
            if (characterImage && dialogueData.characterPortrait)
                characterImage.sprite = dialogueData.characterPortrait;

            //hide panels at start
            SetPanelVisibility(null);

            NextDialogue();
            
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                NextDialogue();
            }
        }

        public void NextDialogue()
        {
            currentIndex++;

            // load next scene if at end
            if (currentIndex >= dialogueData.entries.Length)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Events");
                return;
            }

            // get current entry
            var entry = dialogueData.entries[currentIndex];
            
            if (dialogueText) dialogueText.text = entry.text;
            
            // activate necessary panel 
            SetPanelVisibility(entry.panelToActivate);

            
            if (continuePrompt)
                continuePrompt.SetActive(currentIndex < dialogueData.entries.Length);
        }

        private void SetPanelVisibility(DialogueData.Entry.PanelType? type)
        {
            // deactivate all panels
            if (cvPanel) cvPanel.SetActive(false);
            if (eventsPanel) eventsPanel.SetActive(false);
            if (defaultBackground) defaultBackground.SetActive(false);

            switch (type)
            {
                case DialogueData.Entry.PanelType.CV:
                    if (cvPanel) cvPanel.SetActive(true);
                    break;
                case DialogueData.Entry.PanelType.Events:
                    if (eventsPanel) eventsPanel.SetActive(true);
                    break;
                default:
                    if (defaultBackground) defaultBackground.SetActive(true);
                    break;
            }
        }

        // skip intro button
        public void SkipToEnd()
        {
            currentIndex = dialogueData.entries.Length - 1;
            NextDialogue();
        }
    }
}