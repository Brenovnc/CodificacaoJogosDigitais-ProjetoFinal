using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;
    
    float maxFallSpeed = -4f;

    #region Variaveis - Controlar a gravidade de pulo
    float fallGravity = 2f; // quanto maior, mais rapida a queda
    float jumpGravity = 0.3f; // quanto menor, mais lenta a subida
    #endregion

    #region Variaveis - Controlar o pulo
    bool jumpPressed;
    [SerializeField] float jumpStartTime = 0.2f;
    private float jumpTime;
    private bool isJumping;
    bool jumpUsed;
    #endregion

    #region Variaveis - Controlar a fricção do player
    PhysicsMaterial2D noFriction;
    PhysicsMaterial2D normalFriction;
    BoxCollider2D _playerCollider;
    #endregion

    Rigidbody2D _playerRb;
    bool isGrounded;
    float xDir;
    float yDir;

    public float JumpForce
    {
        get => jumpForce;
        set => jumpForce = value;
    }

    Animator _playerAnimatorSprite;
    public PauseController pauseController;

    private void Awake()
    {
        _playerAnimatorSprite = GetComponentInChildren<Animator>();

        #region Variaveis para controlar a fricção do player 
        _playerCollider = GetComponent<BoxCollider2D>();

        noFriction = new PhysicsMaterial2D("NoFriction")
        {
            friction = 0.1f,
            bounciness = 0f
        };

        normalFriction = new PhysicsMaterial2D("NormalFriction")
        {
            friction = 0.4f,
            bounciness = 0f
        };
        #endregion
    }

    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerRb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _playerAnimatorSprite.SetFloat("VelocidadeX", Mathf.Abs(_playerRb.linearVelocityX));
        _playerAnimatorSprite.SetFloat("VelocidadeY", _playerRb.linearVelocityY);

        // Ajusta material de física (escorregar nas paredes)
        _playerCollider.sharedMaterial = isGrounded ? normalFriction : noFriction;

        if (_playerRb.linearVelocity.y < maxFallSpeed)
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, maxFallSpeed);
            
        Move();
        Jump();
    }
    void OnJump(InputValue inputValue)
    {
        jumpPressed = inputValue.isPressed;
    }

    void OnMove(InputValue inputValue)
    {
        xDir = inputValue.Get<Vector2>().x;
    }

    void OnPause()
    {
        pauseController.MenuDePausa();
    }


    void Move()
    {
        // A velocidade tem que ser alterada aqui para que possamos clicar uma unica vez manter a movimentacao
        // ja que o Move() e chamado no fixed update
        _playerRb.linearVelocityX = xDir * moveSpeed;

        bool IsWalking = Mathf.Abs(_playerRb.linearVelocity.x) > Mathf.Epsilon;

        if (IsWalking)
            FlipSprite();
    }

    void Jump()
    {
        _playerAnimatorSprite.SetBool("IsJumping", isJumping);

        #region Controle da altura do pulo com base no quanto tempo o botao e pressionado
        if (isGrounded && jumpPressed)
        {
            isJumping = true;
            jumpTime = jumpStartTime;
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, jumpForce);
        }

        if (isJumping && jumpPressed)
        {

            if (jumpTime > 0)
            {
                _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, jumpForce);
                jumpTime -= Time.fixedDeltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (!jumpPressed)
        {
            isJumping = false;
        }
        #endregion

        # region Gravidade - velocidade de subida e descida diferente
        if (_playerRb.linearVelocity.y > 0 && jumpPressed)
        {
            float slowerUp = jumpForce * jumpGravity * Time.fixedDeltaTime;
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, _playerRb.linearVelocity.y - slowerUp);
        }
        else if (_playerRb.linearVelocity.y < 0)
        {
            float fasterDown = jumpForce * fallGravity * Time.fixedDeltaTime;
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, _playerRb.linearVelocity.y - fasterDown);
        }
        #endregion
    }

    void FlipSprite()
    {
        transform.localScale = new Vector3(x: Mathf.Sign(_playerRb.linearVelocityX) * Mathf.Abs(_playerRb.transform.localScale.x), y: _playerRb.transform.localScale.y, z: _playerRb.transform.localScale.z);
    }
}
