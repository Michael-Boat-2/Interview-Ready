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
        [SerializeField] private PlayerExperienceData expData;
        [SerializeField] private int expReward = 10;

        private void Start()
        {

            var index = 0;
            
            foreach (var tipBtn in tipButtons)
            {
                var index1 = index;

                tipBtn.gameObject.SetActive(true);
                tipBtn.interactable = true;
                
                tipBtn.onClick.RemoveAllListeners();
                
                tipBtn.onClick.AddListener(() => ShowTip(index1));
                
                var btnText = tipBtn.GetComponentInChildren<TextMeshProUGUI>();

                if (btnText)
                    btnText.text = $"Tip #{index + 1}";
                
                index++;
                
            }
        }
        
        

        //Toggle our posts panel on and off
        public void TogglePostPanel()
        {
            postPanel.SetActive(!postPanel.activeSelf);
        }
        
        
        private void ShowTip(int buttonIndex)
        {

            if (postTipData.InterviewTips.Length <= 0 )
                return;
                
            
            if (tipPopup && popupText)
            {
                popupText.text = postTipData.InterviewTips[buttonIndex] ?? "";
                tipPopup.SetActive(true);
            }
            
            expData?.AddExperience(expReward);
            
            // Disable the button after claiming
            tipButtons[buttonIndex].interactable = false;
            tipButtons[buttonIndex] = null;   // mark as taken
        }
    }
        
    
}
