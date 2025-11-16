using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseController : MonoBehaviour
{
    [SerializeField] GameObject container;
    [SerializeField] Transform checkpoint;
    [SerializeField] Transform player;

    public void MenuDePausa()
    {
        container.SetActive(true);
        Time.timeScale = 0;
    }

    public void VoltarAoJogo()
    {
        container.SetActive(false);
        Time.timeScale = 1;
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void VoltarUltimoCheckpoint()
    {
        player.position = checkpoint.position;
        VoltarAoJogo();
    }

}

}