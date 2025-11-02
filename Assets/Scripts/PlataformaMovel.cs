using UnityEngine;

public class PlataformaMovel : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] pontos;

    private int i;

    void Start()
    {
        transform.position = pontos[1].position;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(null);
        }
    }

}

