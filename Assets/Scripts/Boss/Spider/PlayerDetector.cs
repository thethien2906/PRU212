using UnityEngine;
using UnityEngine.Events;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask playerLayer;

    public UnityEvent onPlayerDetected;
    public UnityEvent onPlayerLost;

    private Transform player;
    private bool playerDetected = false;

    void Update()
    {
        // Find player if not set
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRadius)
            {
                if (!playerDetected)
                {
                    playerDetected = true;
                    onPlayerDetected.Invoke();
                }
            }
            else
            {
                if (playerDetected)
                {
                    playerDetected = false;
                    onPlayerLost.Invoke();
                }
            }
        }
    }

    // Optional: Visualize detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}