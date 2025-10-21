using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;

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
    }

    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerRb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        Move();
        Jump();
    }

    void OnMove(InputValue inputValue)
    {
        xDir = inputValue.Get<Vector2>().x;
    }

    void OnJump(InputValue inputValue)
    {
        if (isGrounded)
        {
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, jumpForce);
        }
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
        bool IsJumping = !_playerRb.IsTouchingLayers(groundLayer);
        _playerAnimatorSprite.SetBool("IsJumping", IsJumping);

    }

    void FlipSprite()
    {
        transform.localScale = new Vector3(x: Mathf.Sign(_playerRb.linearVelocityX) * Mathf.Abs(_playerRb.transform.localScale.x), y: _playerRb.transform.localScale.y, z: _playerRb.transform.localScale.z);
    }
}
