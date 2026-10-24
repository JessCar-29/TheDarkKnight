using UnityEngine;
using TMPro;
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
    [SerializeField] int attackDamage = 1;
    [SerializeField] float attackCooldown = 0.4f;
    [SerializeField] float attackAnimTime = 0.4f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Health health;
    float moveX;
    bool grounded;
    float nextAttack;
    float attackingUntil;
    private int coins;
    public  TMP_Text textcoins;
    private int estrella;
    public TMP_Text textestrella;
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
            nextAttack = Time.time + attackCooldown;
            attackingUntil = Time.time + attackAnimTime;
            anim.SetTrigger("Attack");
            Attack();
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
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }

    void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyPatrol patrol = hit.GetComponent<EnemyPatrol>();
            if (patrol != null && patrol.BackAttackOnly && !patrol.IsBehind(transform.position))
                continue; // de frente no le hace da�o

            Health h = hit.GetComponent<Health>();
            if (h != null) h.TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

  private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.transform.CompareTag("Coin"))
    {
        Destroy(collision.gameObject);
        coins++;
        textcoins.text = coins.ToString();

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin();
    }
    else if (collision.CompareTag("Estrella"))
    {
        Destroy(collision.gameObject);
        estrella++;
        textestrella.text = estrella.ToString();

        if (GameManager.Instance != null)
            GameManager.Instance.AddStar();
    }
}

}