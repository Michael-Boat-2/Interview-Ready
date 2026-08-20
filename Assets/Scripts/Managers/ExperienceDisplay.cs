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
        [SerializeField] private float levelUpPause = 0.2f;
        
        [Header("Level Up Sound")]
        [SerializeField] private AudioClip levelUpSound;
        
        private Coroutine fillRoutine;
        private int previousLevel;
        
        
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
            if (!experienceData)
            {
                Debug.LogWarning("missing player experience data");
                return;
            }
            
            // Set initial values without animation
            previousLevel = experienceData.currentLevel;
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
            
      
            // Stop any running animation
            if (fillRoutine != null)
                StopCoroutine(fillRoutine);

            // Start smooth fill animation
            fillRoutine = StartCoroutine(AnimateExperience());
            
        }

        private IEnumerator AnimateExperience()
        {
            int targetLevel = experienceData.currentLevel;
            float targetFill = (float)experienceData.currentExp / experienceData.maxExpForLevelUp;
            
            // If we leveled up (possibly multiple times), animate each level-up
            while (previousLevel < targetLevel)
            {
                // Animate fill from current to full
                yield return StartCoroutine(AnimateFill(1f, fillAnimationDuration));

                // Play level up sound
                if (levelUpSound)
                    SoundManager.Instance?.PlaySFX(levelUpSound);

                // Small pause at full
                yield return new WaitForSeconds(levelUpPause);

                // Reset fill to 0 for next level
                expFill.fillAmount = 0f;
                previousLevel++;

                // Update level text
                if (expLevel)
                    expLevel.text = $"{previousLevel}";
            }
            
            // Animate to the final fill amount (current exp within the current level)
            yield return StartCoroutine(AnimateFill(targetFill, fillAnimationDuration));

            fillRoutine = null;
            
        }
        
        

        private IEnumerator AnimateFill(float targetFill, float duration)
        {
            float startFill = expFill.fillAmount;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                expFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                yield return null;
            }

            expFill.fillAmount = targetFill;
      
        }
        
        
        
        
        
    }
}