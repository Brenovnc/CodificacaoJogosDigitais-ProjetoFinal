using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MenuUIController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void StartGame()
        {
            SceneManager.LoadScene("Quarto");
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
