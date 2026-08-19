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

        [Header("Animation")]
        [SerializeField] private float fillAnimationDuration = 0.5f;
        
        private Coroutine fillRoutine;
        
        
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
            
            // Set initial values without animation
            UpdateDisplayInstant();
            
        }
        
        private void UpdateDisplayInstant()
        {
            if (!experienceData || !expFill || !expLevel)
                return;

            expFill.fillAmount = (float)experienceData.currentExp / experienceData.maxExpForLevelUp;
            expLevel.text = $"{experienceData.currentLevel}";
        }

        private void ExperienceFill(float ratioGain)
        {
            
            if (!experienceData || !expFill || !expLevel)
                return;
            
            //level update
            expLevel.text = $"{experienceData.currentLevel}";
            
            
            // Target fill based on final experience values
            float targetFill = (float)experienceData.currentExp / experienceData.maxExpForLevelUp;
            
            
            // Stop any running animation
            if (fillRoutine != null)
                StopCoroutine(fillRoutine);

            // Start smooth fill animation
            fillRoutine = StartCoroutine(AnimateFill(targetFill));
            
        }
        
        

        private IEnumerator AnimateFill(float targetFill)
        {
            float startFill = expFill.fillAmount;
            float elapsed = 0f;

            while (elapsed < fillAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fillAnimationDuration);
                expFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                yield return null;
            }

            expFill.fillAmount = targetFill;
            fillRoutine = null;
        }
        
        
        
        
        
    }
}