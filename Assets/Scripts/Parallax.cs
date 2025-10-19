using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;

    public Transform player;

    Vector2 startPosition;
    float startZ;

    // Distância entre o sprite e a câmera
    Vector2 travel => (Vector2)cam.transform.position - startPosition;

    float distanceFromPlayer => transform.position.z - player.position.z;

    // Se o plano tiver mais perto do farClipPlane, usamos o far como referência, caso contrário usamos o near como referência
    float clippingPlane => (cam.transform.position.z + (distanceFromPlayer > 0 ? cam.farClipPlane : cam.nearClipPlane));

    float parallaxFactor => Mathf.Abs(distanceFromPlayer) / clippingPlane;

    private void Start()
    {
        // O transform.position é um vector3, porém estamos armazenando apenas o x e y
        // e deixando o z numa variável à parte
        // Isso é importante pois evita conversão entre vectors nos cálculos que serão feitos no update que serão 2d
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    private void FixedUpdate()
    {
        Vector2 newPos = transform.position = startPosition + travel * parallaxFactor;

        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }



}
