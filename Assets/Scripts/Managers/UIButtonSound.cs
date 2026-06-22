using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private AudioClip clickSound;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (SoundManager.Instance)
                    SoundManager.Instance.PlaySFX(clickSound);
            });
        }
    }
}