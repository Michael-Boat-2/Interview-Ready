using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneController : MonoBehaviour
    {
        

        public void Menu()
        {
            SceneManager.LoadScene(0);
        }

        public void LoadScene(int scene)
        {
            SceneManager.LoadScene(scene);
        }

        public void Events()
        {
            SceneManager.LoadScene("Events");
        }
        

        public void Quit()
        {
            Application.Quit();
        }
        
    }
}
