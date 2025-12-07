using UnityEngine;
using System.Collections;

public class Death : MonoBehaviour
{
    [SerializeField] Transform checkpoint;
    [SerializeField] Player player;
    float dissolveTime = 0.5f;

    Material playerMat;
    int dissolveID;

    PlataformaMovel[] plataformas;

    bool isRespawning = false;

    void Start()
    {
        plataformas = FindObjectsByType<PlataformaMovel>(FindObjectsSortMode.None);
        
        playerMat = player.GetComponent<SpriteRenderer>().material;
        dissolveID = Shader.PropertyToID("_DissolveAmount");
        playerMat.SetFloat(dissolveID, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isRespawning) return;
        if (!other.CompareTag("Player")) return;
        player.CanMoveHorizontally = false;
        player.extraJumps = 0;

        StartCoroutine(RespawnWithDissolve());
    }

    IEnumerator RespawnWithDissolve()
    {
        isRespawning = true;

        player.CanMoveHorizontally = false;
        player.extraJumps = 0;

        var rb = player.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        // 🔥 dissolve 0 → 1
        float t = 0;
        while (t < dissolveTime)
        {
            t += Time.deltaTime;
            playerMat.SetFloat(dissolveID, t / dissolveTime);
            yield return null;
        }

        // ⚠ desabilita o COLLIDER enquanto teleporta
        // evita o player colidir com o chão/espinho na hora
        Collider2D playerCol = player.GetComponent<Collider2D>();
        playerCol.enabled = false;

        // reseta dissolve
        playerMat.SetFloat(dissolveID, 0f);

        // teleporta
        player.transform.position = checkpoint.position;

        // pequena espera para física estabilizar
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // agora ativa o collider
        playerCol.enabled = true;

        // reseta plataformas
        foreach (var plat in plataformas)
            plat.ResetPlataforma();

        player.CanMoveHorizontally = true;
        isRespawning = false;
    }
}
