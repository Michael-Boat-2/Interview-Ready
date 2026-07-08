using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class InterviewTipButton : MonoBehaviour
    {
        [SerializeField][TextArea] private string tipText;
        [SerializeField] private PlayerExperienceData experienceData;
        [SerializeField] private int expReward = 10;

        [SerializeField] private GameObject tipPopup;          // panel with TextMeshProUGUI
        [SerializeField] private TextMeshProUGUI popupText;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(ShowTip);
        }

        private void ShowTip()
        {
            if (tipPopup && popupText)
            {
                popupText.text = tipText;
                tipPopup.SetActive(true);
            }
            experienceData?.AddExperience(expReward);
        }
    }
}