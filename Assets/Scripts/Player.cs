using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;

    #region Variaveis - Controlar o pulo
    bool jumpPressed;
    [SerializeField] float jumpStartTime = 0.25f;
    private float jumpTime;
    private bool isJumping;
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

        // Ajusta material de física (escorregar nas paredes)
        _playerCollider.sharedMaterial = isGrounded ? normalFriction : noFriction;

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


    void Move()
    {
        // A velocidade tem que ser alterada aqui para que possamos clicar uma �nica vez manter a movimenta��o
        // j� que o Move() � chamado no fixed update
        _playerRb.linearVelocityX = xDir * moveSpeed;

        bool IsWalking = Mathf.Abs(_playerRb.linearVelocity.x) > Mathf.Epsilon;
        _playerAnimatorSprite.SetBool("IsWalking", IsWalking);

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
    }

    void FlipSprite()
    {
        transform.localScale = new Vector3(x: Mathf.Sign(_playerRb.linearVelocityX) * Mathf.Abs(_playerRb.transform.localScale.x), y: _playerRb.transform.localScale.y, z: _playerRb.transform.localScale.z);
    }
}
