using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] bool spriteFacesRight = true;
    [SerializeField] Transform edgeCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float wallDistance = 0.4f;
    [SerializeField] float edgeDistance = 1f;
    [SerializeField] int contactDamage = 1;
    [SerializeField] bool backAttackOnly = true;
    [SerializeField] bool ignoreEdges = false; // marcar en enemigos voladores

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;
    Health health;
    int facing = 1; // 1 = derecha, -1 = izquierda
    float nextFlip;
    float lastX;
    float stuckTime;

    public bool BackAttackOnly => backAttackOnly;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        ApplyVisual();
        lastX = rb.position.x;
    }

    void FixedUpdate()
    {
        if (health.IsDead)
        {
            rb.linearVelocity = new Vector2(0, ignoreEdges ? 0f : rb.linearVelocity.y);
            return;
        }

        bool noGround = !ignoreEdges && !Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeDistance, groundLayer);
        bool wall = Physics2D.Raycast(col.bounds.center, Vector2.right * facing, col.bounds.extents.x + wallDistance, groundLayer);

        if ((noGround || wall) && Time.time >= nextFlip)
            TurnAround();

        float vy = ignoreEdges ? 0f : rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(facing * speed, vy);

        // anti-atasco: si no avanza, da la vuelta
        if (Mathf.Abs(rb.position.x - lastX) < 0.005f) stuckTime += Time.fixedDeltaTime;
        else stuckTime = 0f;
        lastX = rb.position.x;

        if (stuckTime > 0.4f && Time.time >= nextFlip)
        {
            stuckTime = 0f;
            TurnAround();
        }
    }

    void TurnAround()
    {
        nextFlip = Time.time + 0.5f;
        facing *= -1;
        ApplyVisual();
    }

    void ApplyVisual()
    {
        sr.flipX = spriteFacesRight ? facing < 0 : facing > 0;

        if (edgeCheck != null)
        {
            Vector3 p = edgeCheck.localPosition;
            edgeCheck.localPosition = new Vector3(Mathf.Abs(p.x) * facing, p.y, p.z);
        }
    }

    // true si el atacante está detrás del enemigo
    public bool IsBehind(Vector2 attackerPos)
    {
        return (attackerPos.x - transform.position.x) * facing < 0;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (health.IsDead) return;
        if (collision.gameObject.GetComponent<PlayerController>() == null) return;

        bool playerBehind = IsBehind(collision.transform.position);

        // daño por contacto (no si lo tocas por la espalda)
        if (!(backAttackOnly && playerBehind))
        {
            Health ph = collision.gameObject.GetComponent<Health>();
            if (ph != null) ph.TakeDamage(contactDamage);
        }

        // si el jugador le bloquea el camino, da la vuelta
        if (!playerBehind && Time.time >= nextFlip)
            TurnAround();
    }
}