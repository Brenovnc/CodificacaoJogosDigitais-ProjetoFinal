using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Player : MonoBehaviour
{
    private Rigidbody2D _playerRb;
    private BoxCollider2D _playerCollider;
    Animator _playerAnimatorSprite;
    float xDir; // Usado para obter a direção (x, y) no vector WASD e controlar a velo do player pelo onMove

    private PhysicsMaterial2D noFriction;
    private PhysicsMaterial2D normalFriction;

    // Referência para o novo script de controle do vento
    private WindController _windController;

    #region Variaveis - Pulo em contato com o chão e movimentação horizontal
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.18f;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    #endregion

    #region Variaveis - Controlar a gravidade de pulo
    float jumpGravity = 0.3f; // quanto menor, mais lenta a subida
    float maxFallSpeed = -6f;

    [Header("Gliding Settings")]
    public bool isGliding = false; // Estado de planar (voo lento com guarda-chuva)
    [SerializeField] float maxGlideFallSpeed = -2f; // Velocidade máxima de queda durante o planamento
    #endregion

    #region Variaveis - Controlar o pulo
    public bool jumpQueued; // Ao dar o pulo isso aqui fica zerado      
    public bool jumpPressed; // Usado para controlar altura do pulo segurando. TORNADO PUBLIC para WindController acessar
    [SerializeField] float jumpTimeValue = 0.2f; // tempo base para resetar o jumpTimeCurrent ao tocar o chão
    private float jumpTimeCurrent; // Tempo máximo que o player pode segurar o pulo
    private bool isJumping;
    private bool isFalling;

    // Double jump
    private int extraJumpsValue = 1;
    public int extraJumps;

    #endregion

    #region Variaveis - Controlar o WallJump
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallCheckDistance = 0.2f;
    [SerializeField] LayerMask wallLayer;

    bool isOnWall;
    bool isWallSliding;
    bool wallJumping;
    [SerializeField] float wallJumpTime = 0.15f; // tempo de "travamento"


    [SerializeField] float wallJumpForce = 6f;
    [SerializeField] float wallJumpHorizontal = 4f;

    #endregion

    public float JumpForce
    {
        get => jumpForce;
        set => jumpForce = value;
    }

    // Referência para o script do menu de pausa
    public PauseController pauseController;
    private bool isPaused = false;
    public bool IsGamePaused => isPaused;

    public bool CanMoveHorizontally { get; set; } = true;

    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
        _playerAnimatorSprite = GetComponent<Animator>();
        _windController = GetComponent<WindController>(); // Obter referência ao script WindController

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
        isOnWall = Physics2D.Raycast(wallCheck.position, Vector2.right * Mathf.Sign(transform.localScale.x), wallCheckDistance, wallLayer);


        // Seta valores para as variáveis VelocidadeX e VelocidadeY para controle das animações
        _playerAnimatorSprite.SetFloat("VelocidadeX", Mathf.Abs(_playerRb.linearVelocityX));
        _playerAnimatorSprite.SetFloat("VelocidadeY", _playerRb.linearVelocityY);
        _playerAnimatorSprite.SetBool("IsSliding", isWallSliding);
        _playerAnimatorSprite.SetBool("IsJumping", isJumping);
        _playerAnimatorSprite.SetBool("IsFalling", isFalling);
        _playerAnimatorSprite.SetBool("IsGliding", isGliding); // NOVO: Animação de Planar


        // Ajusta material para o player escorregar nas paredes
        _playerCollider.sharedMaterial = noFriction;

        // Lógica de Planar (sem alteração, mantém a queda lenta)
        if (isGrounded)
        {
            // Reseta o estado de planar ao tocar o chão
            isGliding = false;
            _playerRb.gravityScale = _windController.normalGravity; // Garante que a gravidade volte ao normal
        }
        else if (isGliding && !jumpPressed && !_windController.inWind)
        {
            // Se está planando fora do vento e soltou o botão, para de planar.
            isGliding = false;
            _playerRb.gravityScale = _windController.normalGravity;
        }

        // Reseta a velocidade do personagem para maxFallSpeed caso ultrapasse esse valor
        // Usa maxGlideFallSpeed se estiver planando
        float currentMaxFall = isGliding ? maxGlideFallSpeed : maxFallSpeed;
        if (_playerRb.linearVelocity.y < currentMaxFall)
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, currentMaxFall);

        Move(); // Aplica velocidade com base no moveSpeed, vê se o player ta andando e chama o flipSprite
        Jump();
        HandleWallSlide();
        ApplyCustomGravity(); // NOVO: Chama a lógica de gravidade customizada
        // ApplyWindControl() está no FixedUpdate do WindController
    }

    // NOVO: Método para aplicar a gravidade customizada quando não está planando ou no vento
    void ApplyCustomGravity()
    {
        // Se estiver no vento ou planando (fora do vento), a gravidade é controlada externamente ou pelo clamping de velocidade
        if (_windController.inWind || isGliding)
        {
            return;
        }

        # region Gravidade - velocidade de subida e descida diferente (Original)
        // SÓ APLICA gravidade customizada se NÃO estiver no vento ou planando
        if (_playerRb.linearVelocity.y > 0 && jumpPressed)
            _playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpGravity - 1) * Time.fixedDeltaTime;
        else if (_playerRb.linearVelocity.y < 0)
            _playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpGravity - 1) * Time.fixedDeltaTime;
        #endregion
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
        if (!isPaused)
        {
            pauseController.MenuDePausa();
            isPaused = true;
        }
        else
        {
            pauseController.VoltarAoJogo();
            isPaused = false;
        }
    }

    void Move()
    {
        // Define a velocidade do player com base na direção do movimento (esquerda ou direita) e o moveSpeed
        if (!CanMoveHorizontally || wallJumping)
            return; // SE NÃO PODE MOVER, SAI DA FUNÇÃO.

        // Antigo código de Move()
        _playerRb.linearVelocityX = xDir * moveSpeed;

        // Math.Epsilon é a menor representação possível de um valor que não seja zero que o float pode representar
        // Aqui eu vejo se o player está andando, independente do lado
        bool IsWalking = Mathf.Abs(_playerRb.linearVelocity.x) > Mathf.Epsilon;

        if (IsWalking)
            FlipSprite();
    }

    // OnTriggerEnter2D e OnTriggerExit2D foram movidos para WindController.cs

    void Jump()
    {
        if (_playerRb.linearVelocity.y < 0)
            isJumping = false;

        // Transiciona entre as blend tree Movimento e Pulando
        _playerAnimatorSprite.SetBool("IsJumping", isJumping);

        // if(isGrounded)
        //       extraJumps = 0;

        if (jumpQueued)
        {

            if (isWallSliding)
            {
                wallJumping = true;
                isJumping = true;
                jumpTimeCurrent = jumpTimeValue;

                // Impulso para longe da parede
                float direction = -Mathf.Sign(transform.localScale.x);
                _playerRb.linearVelocity = new Vector2(direction * wallJumpHorizontal, wallJumpForce);

                FlipSpriteInstant(direction);

                // Cancela a detecção e o estado de slide imediatamente
                isWallSliding = false;
                _playerAnimatorSprite.SetBool("IsSliding", false);

                jumpQueued = false;

                // Bloqueia o movimento horizontal por um curto tempo para evitar "voltar para a parede"
                Invoke(nameof(StopWallJump), wallJumpTime);

                return;
            }

            // Removido o bloqueio do pulo no chão dentro do vento.
            // O pulo normal agora ocorre, e o WindController assume imediatamente no FixedUpdate.

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

        // A lógica de gravidade customizada foi movida para ApplyCustomGravity()
    }

    private void FlipSprite()
    {
        transform.localScale = new Vector3(x: Mathf.Sign(_playerRb.linearVelocityX) * Mathf.Abs(_playerRb.transform.localScale.x), y: _playerRb.transform.localScale.y, z: _playerRb.transform.localScale.z);
    }

    public void ResetExtraJump()
    {
        extraJumps = extraJumpsValue;
    }

    void HandleWallSlide()
    {
        bool pushingTowardsWall =
        (xDir > 0 && Mathf.Sign(transform.localScale.x) > 0 && isOnWall) ||
        (xDir < 0 && Mathf.Sign(transform.localScale.x) < 0 && isOnWall);



        if (wallJumping)
        {
            isWallSliding = false;
            return;
        }

        if (isOnWall && !isGrounded && _playerRb.linearVelocity.y < 0 && pushingTowardsWall)
        {
            isWallSliding = true;
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, Mathf.Clamp(_playerRb.linearVelocity.y, -2f, float.MaxValue));
        }

        else
        {
            isWallSliding = false;
        }

        _playerAnimatorSprite.SetBool("IsSliding", isWallSliding);
    }

    void FlipSpriteInstant(float direction)
    {
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void StopWallJump()
    {
        wallJumping = false;
    }
}