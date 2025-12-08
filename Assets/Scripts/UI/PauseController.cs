using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

namespace UI
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] GameObject container;
        [SerializeField] Transform checkpoint;
        [SerializeField] Transform player;

        [SerializeField] GameObject firstSelectedButton;

        [SerializeField] private GameObject uiInputModuleObject;


        public void MenuDePausa()
        {
            // 1. Ativa o container
            container.SetActive(true);
            Time.timeScale = 0;

            StartCoroutine(SelectFirstButtonOnNextFrame());

            if (uiInputModuleObject != null)
            {
                uiInputModuleObject.SetActive(true);
            }
        }

        private IEnumerator SelectFirstButtonOnNextFrame()
        {
            yield return null;

            if (EventSystem.current != null && firstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);

                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }
        }

        public void VoltarAoJogo()
        {
            container.SetActive(false);
            Time.timeScale = 1;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            if (uiInputModuleObject != null)
            {
                uiInputModuleObject.SetActive(false);
            }
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