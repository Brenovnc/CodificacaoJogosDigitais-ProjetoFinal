using UnityEngine;
using System.Collections;

public class Dicas : MonoBehaviour
{
    [SerializeField] private GameObject elementoCanvas;
    [SerializeField] private float duracaoAtivacao = 3f;
    private bool estaMostrando = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !estaMostrando)
        {
            StartCoroutine(AtivarElementoPorTempo());
        }
    }

    private IEnumerator AtivarElementoPorTempo()
    {
        estaMostrando = true;

        if (elementoCanvas != null)
        {
            elementoCanvas.SetActive(true);
        }

        yield return new WaitForSeconds(duracaoAtivacao);

        if (elementoCanvas != null)
        {
            elementoCanvas.SetActive(false);
        }
        //estaMostrando = false;
    }
}
