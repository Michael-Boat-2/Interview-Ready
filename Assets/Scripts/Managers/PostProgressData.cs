using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "PostProgressData", menuName = "Game/Post Progress Data")]
    public class PostProgressData : ScriptableObject
    {
        [SerializeField] private bool[] tipsRead;

        public void Initialise(int tipCount)
        {
            if (tipsRead == null || tipsRead.Length != tipCount)
                tipsRead = new bool[tipCount];
        }

        public bool IsTipRead(int index)
        {
            return tipsRead != null && index >= 0 && index < tipsRead.Length && tipsRead[index];
        }

        public void MarkTipRead(int index)
        {
            if (tipsRead != null && index >= 0 && index < tipsRead.Length)
                tipsRead[index] = true;
        }

        public void ResetProgress()
        {
            if (tipsRead != null)
                for (int i = 0; i < tipsRead.Length; i++)
                    tipsRead[i] = false;
        }
    }
}