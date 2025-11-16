using UnityEngine;
using UnityEngine.SceneManagement;

public class FaseManager : MonoBehaviour
{
    [SerializeField] string NomeFase;

    void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(NomeFase);
    }
}
