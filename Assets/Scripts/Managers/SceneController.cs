using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }


        public void Menu()
        {
            SceneManager.LoadScene(0);
        }

        public void LoadScene(int scene)
        {
            SceneManager.LoadScene(scene);
        }
        

        public void Quit()
        {
            Application.Quit();
        }
        
    }
}
