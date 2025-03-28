using UnityEngine;

public class EnemyGrenade : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private bool hasExploded = false;

    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private LayerMask damageableLayers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void Throw(int direction, float force, float angle)
    {
        float radianAngle = angle * Mathf.Deg2Rad;
        Vector2 throwVelocity = new Vector2(Mathf.Cos(radianAngle) * force * direction, Mathf.Sin(radianAngle) * force);
        rb.linearVelocity = throwVelocity;

        // Ignore collision between grenade and boss
        Collider2D grenadeCollider = GetComponent<Collider2D>();
        GameObject boss = GameObject.FindGameObjectWithTag("Boss"); // Make sure your boss has this tag
        if (boss != null)
        {
            Collider2D bossCollider = boss.GetComponent<Collider2D>();
            if (bossCollider != null && grenadeCollider != null)
            {
                Physics2D.IgnoreCollision(grenadeCollider, bossCollider);
            }
        }

        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        AudioManager.instance.PlaySFXwithRandomPitch(21);
        anim.SetTrigger("Explode");
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (Collider2D obj in hitObjects)
        {
            // Skip affecting the boss
            if (obj.CompareTag("Boss")) continue;

            Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                Vector2 forceDirection = obj.transform.position - transform.position;
                objRb.AddForce(forceDirection.normalized * explosionForce, ForceMode2D.Impulse);
            }

            if (obj.CompareTag("Player"))
            {
                Health playerHealth = obj.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(40);
                }
            }
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}