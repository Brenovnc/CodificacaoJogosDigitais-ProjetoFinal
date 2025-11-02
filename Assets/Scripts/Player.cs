using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D _playerRb;
    private BoxCollider2D _playerCollider;
    Animator _playerAnimatorSprite;
    float xDir; // Usado para obter a direção (x, y) no vector WASD e controlar a velo do player pelo onMove

    private PhysicsMaterial2D noFriction;
    private PhysicsMaterial2D normalFriction;

    #region Variaveis - Pulo em contato com o chão e movimentação horizontal
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    #endregion

    #region Variaveis - Controlar a gravidade de pulo
    float fallGravity = 2f; // quanto maior, mais rapida a queda
    float jumpGravity = 0.3f; // quanto menor, mais lenta a subida
    float maxFallSpeed = -6f;
    #endregion

    #region Variaveis - Controlar o pulo
    bool jumpQueued; // Ao dar o pulo isso aqui fica zerado     
    bool jumpPressed; // Usado para controlar altura do pulo segurando
    [SerializeField] float jumpTimeValue = 0.2f; // tempo base para resetar o jumpTimeCurrent ao tocar o chão
    private float jumpTimeCurrent; // Tempo máximo que o player pode segurar o pulo
    private bool isJumping;

    // Double jump
    private int extraJumpsValue = 1;
    private int extraJumps;

    #endregion

    public float JumpForce
    {
        get => jumpForce;
        set => jumpForce = value;
    }

    // Referência para o script do menu de pausa
    public PauseController pauseController;

    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
        _playerAnimatorSprite = GetComponentInChildren<Animator>();

        extraJumps = extraJumpsValue;

        noFriction = new PhysicsMaterial2D("NoFriction")
        {
            friction = 0f,
            bounciness = 0f
        };

        normalFriction = new PhysicsMaterial2D("NormalFriction")
        {
            friction = 0.4f,
            bounciness = 0f
        };
    }

    void FixedUpdate()
    {
        // Checa se o player o groundCkeck está em até groundCheckRadius de distância do groundLayer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Seta valores para as variáveis VelocidadeX e VelocidadeY para controle das animações
        _playerAnimatorSprite.SetFloat("VelocidadeX", Mathf.Abs(_playerRb.linearVelocityX));
        _playerAnimatorSprite.SetFloat("VelocidadeY", _playerRb.linearVelocityY);

        // Ajusta material para o player escorregar nas paredes
        _playerCollider.sharedMaterial = noFriction;

        // Reseta a velocidade do personagem para maxFallSpeed caso ultrapasse esse valor
        if (_playerRb.linearVelocity.y < maxFallSpeed)
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, maxFallSpeed);
            
        Move(); // Aplica velocidade com base no moveSpeed, vê se o player ta andando e chama o flipSprite
        Jump(); 
    }
    void OnJump(InputValue inputValue)
    {
        // Enquanto eu estiver segurando o botão de espaço a variável fica True
        // Seto uma variável para controlar a altura do pulo
        // e outra para determinar apenas um pulo por clique
        jumpPressed = inputValue.isPressed;
        jumpQueued = inputValue.isPressed;
    }

    void OnMove(InputValue inputValue)
    {
        xDir = inputValue.Get<Vector2>().x;
    }

    void OnPause()
    {
        // A função só pode ser chamada caso o esc (vinculado ao Pause) seja apertado
        // Como o input está vinculado ao player, precisamos chamar o onPause no player
        pauseController.MenuDePausa();
    }

    void Move()
    {
        // Define a velocidade do player com base na direção do movimento (esquerda ou direita) e o moveSpeed
        _playerRb.linearVelocityX = xDir * moveSpeed;

        // Math.Epsilon é a menor representação possível de um valor que não seja zero que o float pode representar
        // Aqui eu vejo se o player está andando, independente do lado
        bool IsWalking = Mathf.Abs(_playerRb.linearVelocity.x) > Mathf.Epsilon;

        if (IsWalking)
            FlipSprite();
    }

    void Jump()
    {
        // Transiciona entre as blend tree Movimento e Pulando
        _playerAnimatorSprite.SetBool("IsJumping", isJumping);

        if (isGrounded)
            extraJumps = extraJumpsValue;

        if (jumpQueued)
        {
            bool canJump = isGrounded || extraJumps > 0;

            if (canJump)
            {
                // Se não estiver no chão, consome um pulo extra
                if (!isGrounded)
                    extraJumps--;

                isJumping = true;
                jumpTimeCurrent = jumpTimeValue;
                _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, jumpForce);
            }

            jumpQueued = false; // consome o clique
        }

        if (isJumping)
        {
            if (jumpPressed && jumpTimeCurrent > 0)
            {
                // mantém impulso enquanto o botão é segurado
                _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, jumpForce);
                jumpTimeCurrent -= Time.fixedDeltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        # region Gravidade - velocidade de subida e descida diferente
        if (_playerRb.linearVelocity.y > 0 && jumpPressed)
            _playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpGravity - 1) * Time.fixedDeltaTime;
        else if (_playerRb.linearVelocity.y < 0)
            _playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpGravity - 1) * Time.fixedDeltaTime;
        #endregion
    }

    void FlipSprite()
    {
        transform.localScale = new Vector3(x: Mathf.Sign(_playerRb.linearVelocityX) * Mathf.Abs(_playerRb.transform.localScale.x), y: _playerRb.transform.localScale.y, z: _playerRb.transform.localScale.z);
    }
}
