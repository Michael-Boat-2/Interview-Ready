using System.Collections;
using UnityEngine;

namespace Managers
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }
        
        //shake for set duration and intensity
        public void Shake(float intensity = 0.15f, float duration = 0.2f)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                transform.localPosition = originalPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = originalPos;
        }
    }
}