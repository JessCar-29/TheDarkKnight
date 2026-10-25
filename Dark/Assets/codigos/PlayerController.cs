using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CoinFlyAnimation coinFlyAnimation;
    [SerializeField] float speed = 6f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float checkRadius = 0.15f;
    [SerializeField] LayerMask groundLayer;

    [Header("Ataque")]
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRadius = 0.5f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackCooldown = 0.4f;     // mínimo entre golpes del combo
    [SerializeField] float attackAnimTime = 0.4f;

    [Header("Combo")]
    [SerializeField] float comboWindow = 0.8f;         // tiempo máx. desde el golpe anterior para encadenar
    [SerializeField] float finisherCooldown = 0.7f;    // pausa tras el 3er golpe
    [SerializeField] int[] comboDamage = { 1, 1, 2 };  // daño de cada golpe

    [Header("Vida")]
    [SerializeField] float lowHealthThreshold = 0.6f;  // por debajo (o igual) usa el idle herido
    [SerializeField] int healPerPotion = 1;            // vida que cura cada poción

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Health health;
    float moveX;
    bool grounded;
    float nextAttack;
    float attackingUntil;
    int comboStep;          // 0 = ninguno todavía
    float lastAttackTime;
    public int vida = 3;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (health.IsDead)
        {
            moveX = 0;
            return;
        }

        moveX = Input.GetAxisRaw("Horizontal");

        grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && grounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttack)
        {
            // si pasó demasiado tiempo, el combo vuelve al primer golpe
            if (Time.time - lastAttackTime > comboWindow) comboStep = 0;

            comboStep++;
            lastAttackTime = Time.time;
            nextAttack = Time.time + (comboStep >= 3 ? finisherCooldown : attackCooldown);
            attackingUntil = Time.time + attackAnimTime;

            anim.SetTrigger("Attack" + comboStep);   // Attack1, Attack2 o Attack3
            Attack(comboDamage[comboStep - 1]);

            if (comboStep >= 3) comboStep = 0;       // el combo termina y se reinicia
        }

        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;

        // el punto de ataque siempre queda delante del personaje
        float dir = sr.flipX ? -1f : 1f;
        Vector3 p = attackPoint.localPosition;
        attackPoint.localPosition = new Vector3(Mathf.Abs(p.x) * dir, p.y, p.z);

        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetFloat("VelocityY", rb.linearVelocity.y);
        anim.SetBool("Grounded", grounded);
        anim.SetBool("Attacking", Time.time < attackingUntil);
        anim.SetBool("LowHealth", health.Percent <= lowHealthThreshold);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }

    void Attack(int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyPatrol patrol = hit.GetComponent<EnemyPatrol>();
            if (patrol != null && patrol.BackAttackOnly && !patrol.IsBehind(transform.position))
                continue; // de frente no le hace daño

            Health h = hit.GetComponent<Health>();
            if (h != null) h.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pocion"))
        {
            Destroy(collision.gameObject);
            health.Heal(healPerPotion);
        }
    }
}