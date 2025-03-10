using UnityEngine;

public class FlameAttack : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 1;
    [SerializeField] private float damageInterval = 0.5f; // Time between damage ticks
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        gameObject.SetActive(false); // Make sure flame is off at start
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //if (other.CompareTag("Player")) // Ensure the player has the correct tag
        //{
        //    PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        //    if (playerHealth != null)
        //    {
        //        playerHealth.TakeDamage(damagePerSecond);
        //    }
        //}
    }
}
