using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] Transform checkpoint;
    [SerializeField] Transform player;

    void OnTriggerEnter2D(Collider2D other)
    {
        player.position = checkpoint.position;
    }
}
