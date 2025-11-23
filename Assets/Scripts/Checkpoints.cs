using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    [SerializeField] Transform checkpoint;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            checkpoint.position = transform.position;

            GrappleVine playerGrapple = other.GetComponent<GrappleVine>();

            if (playerGrapple != null)
            {
                playerGrapple.ReleaseGrapple();
            }
        }
    }
}
