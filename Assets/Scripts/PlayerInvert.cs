using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInvert : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;              // Rigidbody do player
    [SerializeField] Player playerScript;         // Referência ao script Player
    [SerializeField] float rotationDuration = 0.2f; // Velocidade da rotação

    private bool isInverted = false;  // Controla o estado atual

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = GetComponent<Player>();
    }

    void OnHabilidade01(InputValue inputValue)
    {
        print("TEste");
        InvertGravity();
    }

    void InvertGravity()
    {
        isInverted = !isInverted;

        // inverte a gravidade ou volta ao normal
        rb.gravityScale = Mathf.Abs(rb.gravityScale) * (isInverted ? -1 : 1);

        // inverte a direção da força ou volta ao normal
        playerScript.JumpForce = Mathf.Abs(playerScript.JumpForce) * (isInverted ? -1f : 1f);

        // Rotação: gira 180 ou volta pra 0
        float newRotation = isInverted ? 180f : 0f;
        StopAllCoroutines();
        StartCoroutine(RotateSmoothly(newRotation));
    }

    System.Collections.IEnumerator RotateSmoothly(float targetZ)
    {
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetZ);

        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
    }


}
