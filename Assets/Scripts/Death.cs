using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] Transform checkpoint;
    [SerializeField] Player player;

    void OnTriggerEnter2D(Collider2D other)
    {
        player.transform.position = checkpoint.position;
        player.extraJumps = 0;
    }
}
