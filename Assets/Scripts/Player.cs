using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    #region Variaveis - Vento
    [Header("Wind Settings")]
    bool inWind = false;

    public float normalGravity = 3f;
    public float windGravity = 0.4f;        // ainda mais leve
    public float windLiftForce = 120f;      // subida bem mais rápida
    public float windMaxUpVelocity = 8f;    // limite de velocidade maior

    #endregion

    #region Variaveis - Cipó
    [Header("Cipó")]
    [SerializeField] private float grappleLength = 5f;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer vine;

    private DistanceJoint2D joint;
    private Vector2 moveDir;
    //private Vector3 grapplePoint;
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
        #region Cipó
        joint = GetComponent<DistanceJoint2D>();
        joint.enabled = false;
        vine.enabled = false;
        #endregion

        _playerRb = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
        _playerAnimatorSprite = GetComponent<Animator>();


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
        #region Cipó - lança e solta

        var kb = Keyboard.current;

        // ----- DIREÇÃO (WASD) -----
        float x = 0f;
        float y = 0f;

        if (kb.aKey.isPressed) x = -1;
        if (kb.dKey.isPressed) x = 1;
        if (kb.wKey.isPressed) y = 1;
        if (kb.sKey.isPressed) y = -1;

        moveDir = new Vector2(x, y).normalized;

        // ----- LANÇAR (ENTER) -----
        if (kb.enterKey.isPressed)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                moveDir,
                grappleLength,
                grappleLayer
            );

            Debug.DrawRay(transform.position, moveDir * 3f, Color.red);

            if (hit.collider != null)
            {
                //grapplePoint = hit.point;
                joint.connectedAnchor = hit.point;
                joint.distance = Vector2.Distance(transform.position, hit.point); // Distancia variavel
                //joint.distance = grappleLength; // Distancia fixa
                joint.enabled = true;
                vine.SetPosition(0, hit.point);
                vine.SetPosition(1, transform.position);
                vine.enabled = true;
            }
            //else
            //{
            //    Debug.DrawRay(transform.position, moveDir * 20f, Color.blue);
            //}
        }

        // ----- SOLTAR (ESPAÇO) -----
        if (kb.spaceKey.isPressed)
        {
            joint.enabled = false;
            vine.enabled = false;
        }

        if (vine.enabled)
        {
            //vine.enabled = true;
            vine.SetPosition(1, transform.position);
        }
        #endregion

        // Checa se o player o groundCkeck está em até groundCheckRadius de distância do groundLayer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isOnWall = Physics2D.Raycast(wallCheck.position, Vector2.right * Mathf.Sign(transform.localScale.x), wallCheckDistance, wallLayer);


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
        HandleWallSlide();
        ApplyWindControl();
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
        if (!wallJumping)
            _playerRb.linearVelocityX = xDir * moveSpeed;

        // Math.Epsilon é a menor representação possível de um valor que não seja zero que o float pode representar
        // Aqui eu vejo se o player está andando, independente do lado
        bool IsWalking = Mathf.Abs(_playerRb.linearVelocity.x) > Mathf.Epsilon;

        if (IsWalking)
            FlipSprite();
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


    void Jump()
    {
        // Transiciona entre as blend tree Movimento e Pulando
        _playerAnimatorSprite.SetBool("IsJumping", isJumping);

        if (isGrounded)
            extraJumps = 0;

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

                jumpQueued = false;

                // Bloqueia o movimento horizontal por um curto tempo para evitar "voltar para a parede"
                Invoke(nameof(StopWallJump), wallJumpTime);

                return;
            }

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
        // SÓ APLICA gravidade customizada se NÃO estiver no vento
        if (!inWind)
        {
            if (_playerRb.linearVelocity.y > 0 && jumpPressed)
                _playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpGravity - 1) * Time.fixedDeltaTime;
            else if (_playerRb.linearVelocity.y < 0)
                _playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpGravity - 1) * Time.fixedDeltaTime;
        }
        #endregion

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
        if (wallJumping)
        {
            isWallSliding = false;
            return;
        }

        if (isOnWall && !isGrounded && _playerRb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, Mathf.Clamp(_playerRb.linearVelocity.y, -2f, float.MaxValue));
        }

        //_playerAnimatorSprite.SetBool("IsWallSliding", isWallSliding);
    }

    void FlipSpriteInstant(float direction)
    {
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void StopWallJump()
    {
        wallJumping = false;
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
        if (jumpPressed) // O 'jumpPressed' vem do OnJump
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

