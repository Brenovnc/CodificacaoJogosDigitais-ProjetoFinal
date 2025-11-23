using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleVine : MonoBehaviour
{
    // Adiciona Rigidbody2D e Player.cs para controle
    private Rigidbody2D rb;
    private Player playerScript;

    [Header("Grapple Settings")]
    [SerializeField] private float grappleLength = 10f;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer vine;

    [Header("Swing Settings")]
    [SerializeField] private float swingForce = 20f; // Força para influenciar o balanço (Ajuste no Inspector!)
    [SerializeField] private float playerDrag = 0.5f; // Valor de arrasto do Rigidbody2D durante o agarre

    private DistanceJoint2D joint;
    private Vector2 aimDir;
    private Vector2 lastAimDir;
    private Vector3 grapplePoint;

    private bool isGrappling = false;
    private string sceneName;

    void Awake()
    {
        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // --- Inicializa Componentes ---
        joint = GetComponent<DistanceJoint2D>();
        rb = GetComponent<Rigidbody2D>(); // Pega o Rigidbody2D do Player
        playerScript = GetComponent<Player>(); // Pega o script Player.cs

        joint.enabled = false;
        vine.enabled = false;

        // Para segurança, garante que haja uma direção inicial.
        lastAimDir = Vector2.right;
    }

    void Update()
    {
        if (sceneName != "Floresta")
            return;

        var kb = Keyboard.current;

        // --- DIREÇÃO via WASD ---
        // 'x' e 'y' são usados tanto para mira (lançamento) quanto para controle (balanço)
        float x = 0;
        float y = 0;

        if (kb.aKey.isPressed) x = -1;
        if (kb.dKey.isPressed) x = 1;
        if (kb.wKey.isPressed) y = 1;
        if (kb.sKey.isPressed) y = -1;

        aimDir = new Vector2(x, y).normalized;

        if (aimDir != Vector2.zero)
            lastAimDir = aimDir;

        // --- LANÇAR (ENTER - uma vez por clique) ---
        if (kb.enterKey.wasPressedThisFrame)
        {
            if (!isGrappling)
                TryGrapple();
        }

        // --- SOLTAR (SPACE - uma vez por clique) ---
        if (kb.spaceKey.wasPressedThisFrame)
        {
            if (isGrappling)
                ReleaseGrapple();
        }

        // Atualiza o final da vinha E lida com o controle de balanço
        if (isGrappling)
        {
            vine.SetPosition(1, transform.position);
            HandleSwingControl(x); // Passa o input horizontal 'x'
        }
    }

    // --- LÓGICA DE CONTROLE DE BALANÇO ---
    void HandleSwingControl(float horizontalInput)
    {
        // Se não houver Rigidbody ou entrada, não aplica força
        if (rb == null || Mathf.Abs(horizontalInput) < 0.01f)
            return;

        // 1. Vetor do ponto de agarre para o jogador (a "corda")
        Vector2 playerToGrapple = grapplePoint - transform.position;

        // 2. Calcule a direção perpendicular (tangencial) à corda
        // Isso é a direção do balanço ao longo do arco do pêndulo.
        Vector2 swingDirection = new Vector2(playerToGrapple.y, -playerToGrapple.x).normalized;

        // 3. Aplique a força
        // Multiplicamos pela entrada horizontal para direcionar a força (esquerda ou direita)
        Vector2 force = swingDirection * horizontalInput * swingForce;

        // Aplica a força no Rigidbody para influenciar o pêndulo
        rb.AddForce(force, ForceMode2D.Force);
    }

    // ------------------------------------

    void TryGrapple()
    {
        // Segurança para evitar raycast nulo
        if (lastAimDir == Vector2.zero || rb == null || playerScript == null)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            lastAimDir,
            grappleLength,
            grappleLayer
        );

        Debug.DrawRay(transform.position, lastAimDir * grappleLength, Color.red, 0.2f);

        if (hit.collider == null)
            return;

        // salva ponto fixo
        grapplePoint = hit.point;

        // calcula distância real até o ponto acertado
        float dist = Vector2.Distance(transform.position, grapplePoint);

        // limita pela distância máxima
        dist = Mathf.Min(dist, grappleLength);

        // aplica no Joint
        joint.connectedAnchor = grapplePoint;
        joint.distance = dist;
        joint.enabled = true;

        // --- Ajustes do Player (Integração) ---
        // Desabilita o controle de movimento horizontal do script Player.cs
        playerScript.CanMoveHorizontally = false;
        // Zera a velocidade horizontal para iniciar o balanço mais limpo
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        // Define o arrasto (drag) para um balanço mais limpo (0 é o ideal para pêndulo livre, mas pode ajustar)
        rb.linearDamping = playerDrag;
        // -------------------------------------

        // ativa a vinha
        vine.SetPosition(0, grapplePoint);
        vine.SetPosition(1, transform.position);
        vine.enabled = true;

        isGrappling = true;
    }

    public void ReleaseGrapple()
    {
        // --- Ajustes do Player (Integração) ---
        if (rb != null && playerScript != null)
        {
            // Habilita o controle de movimento horizontal do script Player.cs
            playerScript.CanMoveHorizontally = true;
            // Restaura o arrasto (deixe em 0, pois seu script Player.cs não lida com drag)
            rb.linearDamping = 0f;
        }
        // -------------------------------------

        isGrappling = false;
        joint.enabled = false;
        vine.enabled = false;
    }

}