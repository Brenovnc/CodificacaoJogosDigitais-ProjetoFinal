using UnityEngine;
using System.Collections;

public class Dicas : MonoBehaviour
{
    [SerializeField] private GameObject elementoCanvas;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            elementoCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            elementoCanvas.SetActive(false);
        }
    }
}
