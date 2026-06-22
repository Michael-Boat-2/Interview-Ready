using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

    
        public void PlaySFX(AudioClip clip)
        {
            if (clip)
                sfxSource.PlayOneShot(clip);
        }

      
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!clip || (musicSource.clip == clip && musicSource.isPlaying))
                return;
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

    
        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}