using UnityEngine;

// Certifique-se de que este script esteja no mesmo GameObject que o Player e o Rigidbody2D.
public class WindController : MonoBehaviour
{
    private Rigidbody2D _playerRb;
    private Player _player; // Referência ao Player para acessar jumpPressed

    #region Variaveis - Vento
    [Header("Wind Settings")]
    public bool inWind = false; // PUBLIC para o Player acessar

    public float normalGravity = 3f;
    public float windGravity = 0.4f;        // ainda mais leve
    public float windLiftForce = 120f;      // subida bem mais rápida
    public float windMaxUpVelocity = 8f;    // limite de velocidade maior

    #endregion

    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
    }

    void FixedUpdate()
    {
        ApplyWindControl();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wind"))
        {
            inWind = true;
            _playerRb.gravityScale = windGravity;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wind"))
        {
            inWind = false;
            _playerRb.gravityScale = normalGravity;
        }
    }

    void ApplyWindControl()
    {
        // Se não estamos no vento, não fazemos nada. 
        // A gravidade já foi cuidada pelo OnTriggerExit2D.
        if (!inWind)
        {
            return;
        }

        // Se estamos no vento (e a gravidade já está baixa):

        // se segurar pulo, aplicar força para cima de forma contínua
        if (_player.jumpPressed) // O 'jumpPressed' agora é acessado do script Player
        {
            // Usamos MoveTowards para "empurrar" suavemente a velocidade vertical
            // para o nosso máximo (windMaxUpVelocity) na velocidade do "lift" (windLiftForce).
            float newYVelocity = Mathf.MoveTowards(_playerRb.linearVelocity.y, windMaxUpVelocity, windLiftForce * Time.fixedDeltaTime);
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, newYVelocity);
        }
        // Se o 'jumpPressed' não estiver sendo segurado, não fazemos nada.
        // O player simplesmente flutuará com a gravidade reduzida (windGravity),
        // o que parece ser o comportamento que você quer.
    }
}