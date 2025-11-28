using UnityEngine;

// Certifique-se de que este script esteja no mesmo GameObject que o Player e o Rigidbody2D.
public class WindController : MonoBehaviour
{
    private Rigidbody2D _playerRb;
    private Player _player; // Referência ao Player para acessar jumpPressed e isGliding

    #region Variaveis - Vento
    [Header("Wind Settings")]
    public bool inWind = false; // TORNADO PUBLIC para o script Player acessar o estado

    public float normalGravity = 3f; // TORNADO PUBLIC para o Player.cs poder resetar a gravidade
    public float windGravity = 0.4f;        // ainda mais leve
    public float windLiftForce = 120f;      // subida bem mais rápida
    public float windMaxUpVelocity = 8f;    // limite de velocidade maior

    #endregion

    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
        _playerRb.gravityScale = normalGravity; // Garante o valor inicial
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
            // A gravidade é reduzida ao entrar no vento
            _playerRb.gravityScale = windGravity;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wind"))
        {
            inWind = false;

            // Se o player ESTIVER planando ao sair do vento,
            // MANTEMOS a gravidade em windGravity (queda lenta).
            // Se ele não estiver planando, a gravidade volta ao normal.
            if (!_player.isGliding)
            {
                _playerRb.gravityScale = normalGravity;
            }
        }
    }


    void ApplyWindControl()
    {
        if (!inWind)
        {
            return;
        }

        // Se estamos no vento E o botão de pulo está pressionado:
        if (_player.jumpPressed)
        {
            // Força a gravidade correta para o planar/vento
            _playerRb.gravityScale = windGravity;

            // 1. Ativa o estado de planar
            _player.isGliding = true;

            // 2. Aplica a força de subida no vento
            // Usamos MoveTowards para limitar a velocidade máxima (windMaxUpVelocity)
            float newYVelocity = Mathf.MoveTowards(_playerRb.linearVelocity.y, windMaxUpVelocity, windLiftForce * Time.fixedDeltaTime);
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, newYVelocity);
        }
        else if (_player.isGliding)
        {
            // 3. Se soltou o botão DENTRO do vento, desativa o planar
            _player.isGliding = false;

            // NOVO: Se o player não está mais segurando o botão DENTRO do vento, 
            // a gravidade deve voltar ao normal para que ele caia como se estivesse fora do vento.
            _playerRb.gravityScale = normalGravity;
        }
    }
}