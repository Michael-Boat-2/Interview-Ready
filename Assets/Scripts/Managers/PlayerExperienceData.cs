using UnityEngine;
using System;

namespace Managers
{
    [CreateAssetMenu(fileName = "PlayerExperienceData", menuName = "Game/Player Experience Data")]
    public class PlayerExperienceData : ScriptableObject
    {
        
        public int currentExp = 0;
        //experience it takes to level up
        public int maxExpForLevelUp = 100;      
        public int currentLevel = 1;
        public int maxLevel = 5;
        
        // 1.0 => 1.2 => 1.4 => 1.6 => 1.8 => 2
        public float LevelMultiplier => 
            1f + (currentLevel - 1) * 0.2f;


        public static Action<float> ExperienceGained;
        
        

        public void AddExperience(int amount)
        {
            var ratioGain = (float)amount / maxExpForLevelUp;
            
            currentExp += amount;
            
            //if we have exp go above amount to level-up
            //then level up and start counting to the next threshold
            while (currentExp >= maxExpForLevelUp && currentLevel < maxLevel)
            {
                currentExp -= maxExpForLevelUp;
                currentLevel++;
            }
            
            //no more exp gains
            if (currentLevel >= maxLevel)
                currentExp = 0;
            
          
            
            // do we need to pass this logic? maybe not
            ExperienceGained?.Invoke(ratioGain);
            
        }
        
        
    }
}