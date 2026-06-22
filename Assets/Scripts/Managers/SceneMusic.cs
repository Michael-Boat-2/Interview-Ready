using UnityEngine;

namespace Managers
{
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip musicClip;

        private void Start()
        {
            if (SoundManager.Instance && musicClip)
                SoundManager.Instance.PlayMusic(musicClip);
        }
    }
}