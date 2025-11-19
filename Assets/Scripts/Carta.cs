using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Carta : MonoBehaviour
{
    public GameObject cartaObject;

    private bool playerIsClose;

    void Update()
    {
        if (!playerIsClose) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleCard();
        }
    }

    void ToggleCard()
    {
        bool currentStatus = cartaObject.activeInHierarchy;

        // Alterna o estado:
        if (currentStatus)
        {
            cartaObject.SetActive(false);
        }
        else
        {
            cartaObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            if (cartaObject.activeInHierarchy)
            {
                cartaObject.SetActive(false);
            }
        }
    }
}