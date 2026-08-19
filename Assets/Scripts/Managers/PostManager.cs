using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Managers
{
    public class PostManager : MonoBehaviour
    {

        [Header("UI References")] 
        [SerializeField] private GameObject postPanel;
        [SerializeField] private GameObject tipPopup;
        
        [SerializeField] private Button[] tipButtons;
        [SerializeField] private TextMeshProUGUI popupText;
        

        [Header("Data")] 
        [SerializeField] private PostTipData postTipData;
        [SerializeField] private PostProgressData progressData;
        [SerializeField] private PlayerExperienceData expData;
        [SerializeField] private int expReward = 20;

        private void Start()
        {
            
            if (progressData && postTipData)
                progressData.Initialise(postTipData.InterviewTips.Length);
            
            for (int i = 0; i < tipButtons.Length; i++)
            {
                // capture for closure
                var index = i; 

                tipButtons[i].gameObject.SetActive(true);
                
                // disable if already read
                tipButtons[i].interactable = !(progressData && progressData.IsTipRead(index)); 

                tipButtons[i].onClick.RemoveAllListeners();
                tipButtons[i].onClick.AddListener(() => ShowTip(index));

                TextMeshProUGUI btnText = tipButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText)
                    btnText.text = $"Tip #{index + 1}";
            }
        }
        
        

        //Toggle our posts panel on and off
        public void TogglePostPanel()
        {
            postPanel.SetActive(!postPanel.activeSelf);
        }
        
        
        private void ShowTip(int buttonIndex)
        {

            if (!postTipData || buttonIndex < 0 || buttonIndex >= postTipData.InterviewTips.Length)
                return;
            
            if (tipPopup && popupText)
            {
                popupText.text = postTipData.InterviewTips[buttonIndex] ?? "";
                tipPopup.SetActive(true);
            }
            
            // Award exp only if this tip hasn't been read yet this run
            if (progressData && !progressData.IsTipRead(buttonIndex))
            {
                expData?.AddExperience(expReward);
                progressData.MarkTipRead(buttonIndex);
                tipButtons[buttonIndex]!.interactable = false;
                Debug.Log($"Awarded {expReward} exp for Tip #{buttonIndex + 1}");
            }
            
        }
    }
        
    
}
