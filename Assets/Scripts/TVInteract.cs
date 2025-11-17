using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TVInteract : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    public string[] dialogue;
    private int index;

    public float wordSpeed;
    private bool playerIsClose;
    private bool isTyping;
    private Coroutine typingCoroutine;

    public GameObject[] tvFrames;  // arraste os 4 frames aqui no inspetor

    void Update()
    {
        if (!playerIsClose) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!dialoguePanel.activeInHierarchy)
            {
                dialoguePanel.SetActive(true);
                StartTyping();
            }
            else
            {
                if (isTyping)
                {
                    SkipTyping();
                }
                else
                {
                    NextLine();
                }
            }
        }
    }

    void SetTVFrame(int frameIndex)
    {
        for (int i = 0; i < tvFrames.Length; i++)
            tvFrames[i].SetActive(i == frameIndex);
    }

    void StartTyping()
    {
        dialogueText.text = "";

        // Troca o frame da TV de acordo com o index da fala
        SetTVFrame(index);

        typingCoroutine = StartCoroutine(Typing());
    }

    IEnumerator Typing()
    {
        isTyping = true;

        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }

        isTyping = false;
    }

    void SkipTyping()
    {
        // interrompe a coroutine
        StopCoroutine(typingCoroutine);
        // exibe o texto inteiro instantaneamente
        dialogueText.text = dialogue[index];
        isTyping = false;
    }

    public void NextLine()
    {
        // Se tem próxima frase
        if (index < dialogue.Length - 1)
        {
            index++;
            StartTyping();
        }
        else
        {
            // Última frase → fecha
            zeroText();
        }
    }

    public void zeroText()
{
    dialogueText.text = "";
    index = 0;
    dialoguePanel.SetActive(false);

    // Desliga todos os frames (se quiser)
    SetTVFrame(-1);
}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerIsClose = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            zeroText();
        }
    }
}
