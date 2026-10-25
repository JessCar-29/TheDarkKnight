using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] bool destroyOnDeath = true;
    [SerializeField] float destroyDelay = 1f;
    [SerializeField] float invulnerableTime = 0.5f;
    [SerializeField] bool isPlayer = false;

    int current;
    float invulnerableUntil;
    Animator anim;

    public bool IsDead { get; private set; }
    public int Current => current;
    public int Max => maxHealth;
    public float Percent => (float)current / maxHealth;   // 0 a 1

    void Awake()
    {
        current = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || Time.time < invulnerableUntil) return;

        invulnerableUntil = Time.time + invulnerableTime;
        current -= amount;

        if (current <= 0) Die();
        else if (anim) anim.SetTrigger("Hurt");
    }

    // Cura vida sin pasar del máximo
    public void Heal(int amount)
    {
        if (IsDead) return;
        current = Mathf.Min(current + amount, maxHealth);
    }

    void Die()
    {
        current = 0;
        IsDead = true;
        if (anim) anim.SetTrigger("Die");

        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        if (destroyOnDeath) Destroy(gameObject, destroyDelay);
    }
}