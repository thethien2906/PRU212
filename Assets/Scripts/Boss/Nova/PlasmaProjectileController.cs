using System.Collections;
using UnityEngine;

public class PlasmaProjectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Split Settings")]
    [SerializeField] private GameObject splitProjectilePrefab;
    [SerializeField] private Color[] projectileColors;

    [Header("Projectile Settings")]
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private float defaultSplitDelay = 1f;
    [SerializeField] private int defaultDamage = 15;

    // Properties set during initialization
    private float speed;
    private float splitTime;
    private float spawnTime;
    private int damage;
    private bool hasSplit = false;
    private string playerTag = "Player";

    private void Awake()
    {
        // Get components if not assigned
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Record spawn time
        spawnTime = Time.time;

        // Destroy after maximum lifetime to prevent memory issues
        Destroy(gameObject, 8f);
    }

    public void Initialize(Vector2 direction, float moveSpeed = -1f, float timeToSplit = -1f, int damageAmount = -1)
    {
        // Use provided values or defaults if negative values are passed
        speed = moveSpeed > 0 ? moveSpeed : defaultSpeed;
        splitTime = timeToSplit > 0 ? timeToSplit : defaultSplitDelay;
        damage = damageAmount > 0 ? damageAmount : defaultDamage;

        // Apply initial velocity
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;

            // Set rotation to match direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Assign random color if available
        if (spriteRenderer != null && projectileColors != null && projectileColors.Length > 0)
        {
            spriteRenderer.color = projectileColors[Random.Range(0, projectileColors.Length)];
        }
    }

    private void Update()
    {
        // Check if it's time to split
        if (!hasSplit && Time.time >= spawnTime + splitTime)
        {
            SplitProjectile();
        }
    }

    private void SplitProjectile()
    {
        // Only split once
        if (hasSplit) return;
        hasSplit = true;

        // Create 4 new projectiles in different directions
        if (splitProjectilePrefab != null)
        {
            // Calculate 4 directions (up, down, left, right relative to current direction)
            Vector2 currentDirection = rb.linearVelocity.normalized;
            Vector2 perpendicular = new Vector2(-currentDirection.y, currentDirection.x);

            Vector2[] splitDirections = new Vector2[]
            {
                currentDirection,                // Forward
                -currentDirection,               // Backward
                perpendicular,                   // Left
                -perpendicular                   // Right
            };

            // Create split projectiles
            foreach (Vector2 direction in splitDirections)
            {
                GameObject splitProjectile = Instantiate(
                    splitProjectilePrefab,
                    transform.position,
                    Quaternion.identity
                );

                PlasmaProjectile plasmaScript = splitProjectile.GetComponent<PlasmaProjectile>();
                if (plasmaScript != null)
                {
                    // Initialize with slightly reduced damage and no further splitting
                    plasmaScript.Initialize(direction, speed * 0.8f, float.MaxValue, damage);
                    plasmaScript.SetSplit(); // Mark as already split
                }
                else
                {
                    // Fallback if script not found
                    Rigidbody2D splitRb = splitProjectile.GetComponent<Rigidbody2D>();
                    if (splitRb != null)
                    {
                        splitRb.linearVelocity = direction.normalized * speed * 0.8f;
                    }

                    // Destroy after delay
                    Destroy(splitProjectile, 3f);
                }
            }

            // Destroy original projectile
            Destroy(gameObject);
        }
    }

    // Used to mark this projectile as having already split
    public void SetSplit()
    {
        hasSplit = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit player
        if (other.CompareTag(playerTag))
        {
            // Get player health component and apply damage
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Destroy this projectile
            Destroy(gameObject);
        }
    }
}