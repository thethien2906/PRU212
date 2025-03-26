using UnityEngine;

public class FlameAttack : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 1;
    [SerializeField] private float damageInterval = 0.5f; 
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
                playerHealth.TakeDamage(30); 
            }
        }
    }
}
