using System.Collections;
using UnityEngine;

public class KnightEnemy : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float speed = 1.5f;
    [SerializeField] bool spriteFacesRight = true;
    [SerializeField] Transform edgeCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float wallDistance = 0.4f;
    [SerializeField] float edgeDistance = 1f;

    [Header("Ataque")]
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRadius = 0.6f;
    [SerializeField] int attackDamage = 1;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float hitDelay = 0.3f;
    [SerializeField] float attackDuration = 0.6f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;
    Animator anim;
    Health health;
    int facing = 1; // 1 = derecha, -1 = izquierda
    float nextFlip;
    float nextAttack;
    float attackingUntil;
    float lastX;
    float stuckTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        ApplyVisual();
        lastX = rb.position.x;
    }

    void FixedUpdate()
    {
        if (health.IsDead || Time.time < attackingUntil)
        {
            Stop();
            return;
        }

        PlayerController player = FindPlayerInRange();
        if (player != null)
        {
            FaceTowards(player.transform.position.x);
            Stop();
            if (Time.time >= nextAttack) StartCoroutine(AttackRoutine());
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        bool noGround = !Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeDistance, groundLayer);
        bool wall = Physics2D.Raycast(col.bounds.center, Vector2.right * facing, col.bounds.extents.x + wallDistance, groundLayer);

        if ((noGround || wall) && Time.time >= nextFlip)
            TurnAround();

        rb.linearVelocity = new Vector2(facing * speed, rb.linearVelocity.y);
        anim.SetFloat("Speed", 1f);

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

    void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0f);
        stuckTime = 0f;
        lastX = rb.position.x;
    }

    IEnumerator AttackRoutine()
    {
        attackingUntil = Time.time + attackDuration;
        nextAttack = Time.time + attackCooldown;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(hitDelay);

        if (health.IsDead) yield break;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<PlayerController>() == null) continue;
            Health ph = hit.GetComponent<Health>();
            if (ph != null) ph.TakeDamage(attackDamage);
        }
    }

    PlayerController FindPlayerInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            PlayerController p = hit.GetComponent<PlayerController>();
            if (p == null) continue;

            Health ph = p.GetComponent<Health>();
            if (ph == null || !ph.IsDead) return p;
        }
        return null;
    }

    void FaceTowards(float targetX)
    {
        int dir = targetX > transform.position.x ? 1 : -1;
        if (dir != facing)
        {
            facing = dir;
            ApplyVisual();
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

        FlipChild(edgeCheck);
        FlipChild(attackPoint);
    }

    void FlipChild(Transform t)
    {
        if (t == null) return;
        Vector3 p = t.localPosition;
        t.localPosition = new Vector3(Mathf.Abs(p.x) * facing, p.y, p.z);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}