using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        gameObject.SetActive(false); // Make sure flame is off at start
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20);
                Debug.Log("Projectile hit player! Dealing 20 damage.");
            }

            Destroy(gameObject);
        }
    }
}
