using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;

    public Transform player;

    Vector2 startPosition;
    float startZ;

    // Dist�ncia entre o sprite e a c�mera
    Vector2 travel => (Vector2)cam.transform.position - startPosition;

    float distanceFromPlayer => transform.position.z - player.position.z;

    // Se o plano tiver mais perto do farClipPlane, usamos o far como refer�ncia, caso contr�rio usamos o near como refer�ncia
    float clippingPlane => (cam.transform.position.z + (distanceFromPlayer > 0 ? cam.farClipPlane : cam.nearClipPlane));

    float parallaxFactor => Mathf.Abs(distanceFromPlayer) / clippingPlane;

    private void Start()
    {
        // O transform.position � um vector3, por�m estamos armazenando apenas o x e y
        // e deixando o z numa vari�vel � parte
        // Isso � importante pois evita convers�o entre vectors nos c�lculos que ser�o feitos no update que ser�o 2d
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    private void FixedUpdate()
    {
        Vector2 newPos = transform.position = startPosition + travel * parallaxFactor;

        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }



}
