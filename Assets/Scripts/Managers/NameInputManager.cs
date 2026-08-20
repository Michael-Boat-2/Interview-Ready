using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Managers
{
    public class NameInputManager : MonoBehaviour
    {
        
        [SerializeField] private PlayerProfileData profileData;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Text errorText;
        
        
        private void Start()
        {
            if (!profileData)
                Debug.LogError("No profile data found");
        }
        
        public void OnConfirm()
        {
            if (profileData && !string.IsNullOrWhiteSpace(nameInput.text))
            {
                profileData.playerName = nameInput.text.Trim();
                PlayerPrefs.SetString("PlayerName", profileData.playerName);
                PlayerPrefs.Save();
                SceneManager.LoadScene("Onboarding");
            }
            else
            {
                if (errorText)
                {
                    StopAllCoroutines();
                    StartCoroutine(ShowError());
                }
               
            }
        }

        private IEnumerator ShowError()
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "You entered invalid name";

            yield return new WaitForSeconds(2f);
            
            errorText.gameObject.SetActive(false);
        }
        
       
    }
}