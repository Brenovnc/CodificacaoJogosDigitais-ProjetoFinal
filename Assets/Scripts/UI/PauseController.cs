using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections; // << Adicionado para usar Coroutines

namespace UI
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] GameObject container;
        [SerializeField] Transform checkpoint;
        [SerializeField] Transform player;

        // NOVO: Referência ao primeiro botão que queremos focar
        [SerializeField] GameObject firstSelectedButton;

        // NOVO: Referência ao InputSystemUIInputModule
        // (Mantenho a referência, mas veja a nota sobre remoção)
        [SerializeField] private GameObject uiInputModuleObject;


        public void MenuDePausa()
        {
            // 1. Ativa o container
            container.SetActive(true);
            Time.timeScale = 0;

            // 2. Inicia a coroutine para selecionar o botão no próximo frame
            StartCoroutine(SelectFirstButtonOnNextFrame());

            // 3. Opcional, mas recomendado: Ativar o módulo de UI Input
            if (uiInputModuleObject != null)
            {
                uiInputModuleObject.SetActive(true);
            }
        }

        // Coroutine para definir o foco após 1 frame
        private IEnumerator SelectFirstButtonOnNextFrame()
        {
            // Espera 1 frame para garantir que o EventSystem processou a ativação do Container
            yield return null;

            if (EventSystem.current != null && firstSelectedButton != null)
            {
                // Limpa o foco primeiro (boa prática para prevenir submits acidentais)
                EventSystem.current.SetSelectedGameObject(null);

                // Redefine o foco para o primeiro botão (ativa o estado 'Selected')
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }
        }

        public void VoltarAoJogo()
        {
            container.SetActive(false);
            Time.timeScale = 1;

            // 1. Limpa o foco imediatamente
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            // 2. Opcional, mas recomendado: Desativar o módulo de UI Input
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