using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health health =
            collision.GetComponent<Health>();
        if (health != null )
        {
            health.TakeDamage(health.Current);
        }
    }
}