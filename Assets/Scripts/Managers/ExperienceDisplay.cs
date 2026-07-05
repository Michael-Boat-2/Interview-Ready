using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Managers
{
    public class ExperienceDisplay : MonoBehaviour
    {
        [SerializeField] private PlayerExperienceData experienceData;
        [SerializeField] private TMP_Text expLevel;
        [SerializeField] private Image expFill;

        
        public void OnEnable()
        {
            PlayerExperienceData.ExperienceGained += ExperienceFill;
        }

        public void OnDisable()
        {
            PlayerExperienceData.ExperienceGained -= ExperienceFill;
        }

        public void Start()
        {
            expFill.fillAmount = (float)experienceData.currentExp / experienceData.maxExpForLevelUp;
            expLevel.text = $"{experienceData.currentLevel}";
        }
        
        

        private void ExperienceFill(float ratioGain)
        {
            
            var counter = ratioGain;
            
            if (ratioGain <= experienceData.maxExpForLevelUp)
            {
                expFill.fillAmount = (float)experienceData.currentExp / experienceData.maxExpForLevelUp;
                expLevel.text = $"{experienceData.currentLevel}";
                
            }
            else
            {  
                
                while (counter > 1)
                {
                    //we'll level up
                    //lerp to full level and repeat
                
                
                    counter -= 1;
                
                }
                
                expFill.fillAmount = (float)experienceData.currentExp / experienceData.maxExpForLevelUp;
                expLevel.text = $"{experienceData.currentLevel}";
                
                
            }
            
        }

        

        private IEnumerator FillOverTime(float amount)
        {
            yield return new WaitForSeconds(1f);
        }
        
        
        
        
        
    }
}