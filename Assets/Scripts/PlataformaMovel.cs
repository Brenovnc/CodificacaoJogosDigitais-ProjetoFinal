using UnityEngine;

public class PlataformaMovel : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] pontos;

    private int i;

    void Start()
    {
        transform.position = pontos[0].position;
    }
    void Update()
    {
        if (Vector2.Distance(transform.position, pontos[i].position) < 0.01f)
        {
            i++;
            if (i == pontos.Length)
            {
                i = 0;
            }
        }
            
        transform.position = Vector2.MoveTowards(transform.position, pontos[i].position, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(null);
        }
    }
    public void ResetPlataforma()
    {
        i = 0;
        transform.position = pontos[0].position;
    }
}

