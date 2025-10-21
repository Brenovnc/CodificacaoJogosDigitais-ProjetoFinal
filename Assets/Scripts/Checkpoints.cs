using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    [SerializeField] Transform checkpoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        checkpoint.position = transform.position;
    }
}
