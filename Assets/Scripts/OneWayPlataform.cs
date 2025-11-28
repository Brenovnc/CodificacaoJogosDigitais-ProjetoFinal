using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class OneWayPlataform : MonoBehaviour
{
    private GameObject currentOneWayPlataform;

    [SerializeField] private BoxCollider2D playerCollider;

    void Update()
    {
        var kb = Keyboard.current;
        if ((kb.sKey.isPressed)) {
            if (currentOneWayPlataform != null)
                StartCoroutine(DisableCollision());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("OneWayPlataform"))
        {
            currentOneWayPlataform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlataform"))
        {
            currentOneWayPlataform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D plataformCollider = currentOneWayPlataform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, plataformCollider);
        yield return new WaitForSeconds(0.25f);
        Physics2D.IgnoreCollision(playerCollider, plataformCollider, false);

    }
}
