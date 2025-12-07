using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections; // Necess�rio para a coroutine, se voc� us�-la.

public class MenuUIController : MonoBehaviour
{
    [Header("Pain�is de Navega��o")]
    public GameObject MenuPrincipalPanel;
    public GameObject SelecaoFasePanel;

    [Header("Visuais de Sele��o (Ativa��o de GameObject)")]
    public GameObject FundoSelecaoFloresta;
    public GameObject FundoSelecaoPantano;
    public GameObject FundoSelecaoVoltar;

    [Header("Bot�es e Foco")]
    public Button BotaoVoltar;
    public GameObject FlorestaSelect;
    public GameObject PantanoSelect;
    public Button BotaoJogar;

    [Header("Textos de Bot�es (TMP)")]
    // Apenas textos dos bot�es que precisam de estiliza��o
    public TextMeshProUGUI JogarText;
    public TextMeshProUGUI EscolherFaseText;
    public TextMeshProUGUI SairText;

    private int faseSelecionada = 1;
    private bool estaEmVoltar = false;

    public Color HighlightColor = new Color(74f / 255f, 255f / 255f, 74f / 255f, 1f);
    public Color DefaultColor = Color.white;


    void Start()
    {
        SelecaoFasePanel.SetActive(false);
        faseSelecionada = 1;
        estaEmVoltar = false;
        AtualizarVisuais();

        // Garante o foco inicial ap�s a inicializa��o
        if (BotaoJogar != null)
        {
            // Boa pr�tica: limpa o foco e define
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(BotaoJogar.gameObject);
        }
        ApplyHighlightStyle();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Quarto");
    }

    public void AbrirSelecaoFase()
    {
        MenuPrincipalPanel.SetActive(false);
        SelecaoFasePanel.SetActive(true);

        faseSelecionada = 1;
        estaEmVoltar = false;
        AtualizarVisuais();

        // Define o foco inicial na fase Floresta
        EventSystem.current.SetSelectedGameObject(FlorestaSelect.gameObject);

        // Aplica o estilo do Voltar, caso tenha sido selecionado
        ApplyHighlightStyle();
    }

    public void VoltarParaMenu()
    {
        SelecaoFasePanel.SetActive(false);
        MenuPrincipalPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);

        // Usamos uma coroutine ou Invoke para garantir que a sele��o ocorra no pr�ximo frame
        if (BotaoJogar != null)
        {
            StartCoroutine(SetInitialFocusAndStyleCoroutine());
        }
    }

    // Mantenho esta coroutine para consist�ncia e para resolver problemas de foco
    private IEnumerator SetInitialFocusAndStyleCoroutine()
    {
        yield return null; // Espera 1 frame

        if (BotaoJogar != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(BotaoJogar.gameObject);
        }

        ApplyHighlightStyle();
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (SelecaoFasePanel.activeSelf)
            {
                JogarOuVoltar();
            }
        }
    }

    public void OnNavigateHorizontal(InputAction.CallbackContext context)
    {
        if (!SelecaoFasePanel.activeSelf || !context.performed) return;

        float value = context.ReadValue<Vector2>().x;

        if (value != 0 && !estaEmVoltar)
        {
            if (value > 0) // D (Direita)
            {
                if (faseSelecionada == 1)
                {
                    SelecionarFase(2);
                    EventSystem.current.SetSelectedGameObject(PantanoSelect.gameObject);
                }
            }
            else if (value < 0) // A (Esquerda)
            {
                if (faseSelecionada == 2)
                {
                    SelecionarFase(1);
                    EventSystem.current.SetSelectedGameObject(FlorestaSelect.gameObject);
                }
            }
            // N�o precisa de ApplyHighlightStyle aqui, pois n�o h� textos para estilizar na sele��o de fase.
        }
    }

    public void OnNavigateVertical(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // Se estiver no Menu Principal, apenas atualiza o estilo com base no foco
        if (MenuPrincipalPanel.activeSelf)
        {
            ApplyHighlightStyle();
            return;
        }

        if (SelecaoFasePanel.activeSelf)
        {
            float value = context.ReadValue<Vector2>().y;

            if (value > 0) // W (Para cima) -> VAI PARA VOLTAR
            {
                if (!estaEmVoltar)
                {
                    estaEmVoltar = true;
                    AtualizarVisuais();
                    EventSystem.current.SetSelectedGameObject(BotaoVoltar.gameObject);
                    ApplyHighlightStyle(); // Aplica estilo ao bot�o Voltar
                }
            }
            else if (value < 0) // S (Para baixo) -> VAI PARA FASES
            {
                if (estaEmVoltar)
                {
                    estaEmVoltar = false;
                    AtualizarVisuais();
                    GameObject focusTarget = (faseSelecionada == 1) ? FlorestaSelect.gameObject : PantanoSelect.gameObject;
                    EventSystem.current.SetSelectedGameObject(focusTarget);
                    ApplyHighlightStyle(); // Desfaz o estilo do bot�o Voltar
                }
            }
        }
    }

    public void JogarOuVoltar()
    {
        if (estaEmVoltar)
        {
            VoltarParaMenu();
        }
        else
        {
            CarregarFaseSelecionada();
        }
    }

    public void SelecionarFase(int fase)
    {
        if (fase == 3)
        {
            Debug.Log("Fase bloqueada!");
            return;
        }

        if (fase != faseSelecionada || estaEmVoltar)
        {
            faseSelecionada = fase;
            estaEmVoltar = false;
            AtualizarVisuais();
        }
    }

    private void CarregarFaseSelecionada()
    {
        string sceneName = (faseSelecionada == 1) ? "Floresta" : "P�ntano";
        SceneManager.LoadScene(sceneName);
    }

    // M�todo que cuida dos fundos visuais da sele��o de fase
    private void AtualizarVisuais()
    {
        FundoSelecaoFloresta.SetActive(false);
        FundoSelecaoPantano.SetActive(false);
        FundoSelecaoVoltar.SetActive(false);

        if (SelecaoFasePanel.activeSelf)
        {
            if (estaEmVoltar)
            {
                FundoSelecaoVoltar.SetActive(true);
            }
            else if (faseSelecionada == 1)
            {
                FundoSelecaoFloresta.SetActive(true);
            }
            else if (faseSelecionada == 2)
            {
                FundoSelecaoPantano.SetActive(true);
            }
        }
    }

    // L�GICA DE ESTILIZA��O: Apenas cor e negrito, preservando a fonte.
    private void ApplyHighlightStyle()
    {
        // Define todos os textos de bot�es que precisam de estiliza��o
        var menuTexts = new (TextMeshProUGUI text, GameObject gameObject)[]
        {
            (JogarText, BotaoJogar?.gameObject),
            (EscolherFaseText, EscolherFaseText?.GetComponentInParent<Button>()?.gameObject),
            (SairText, SairText?.GetComponentInParent<Button>()?.gameObject),
            // O texto do bot�o Voltar (extra�do do BotaoVoltar)
            (BotaoVoltar?.GetComponentInChildren<TextMeshProUGUI>(), BotaoVoltar?.gameObject)
        };

        GameObject currentSelection = EventSystem.current.currentSelectedGameObject;

        foreach (var item in menuTexts)
        {
            // Ignora se o TextComponent ou o GameObject n�o foi encontrado
            if (item.text == null || item.gameObject == null) continue;

            // Verifica se o GameObject correspondente � o objeto atualmente selecionado
            if (item.gameObject == currentSelection)
            {
                // Aplica o destaque: Cor + Negrito (usando OR bit a bit para adicionar o estilo)
                item.text.color = HighlightColor;
                item.text.fontStyle |= FontStyles.Bold;
            }
            else
            {
                // Reseta: Cor Padr�o + Remove Negrito (usando AND com a NEGA��O bit a bit)
                item.text.color = DefaultColor;
                item.text.fontStyle &= ~FontStyles.Bold;
            }
        }

        // NOTA: A estiliza��o dos textos Floresta e P�ntano foi removida
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}