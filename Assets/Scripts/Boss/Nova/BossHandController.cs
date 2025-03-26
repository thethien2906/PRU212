using System.Collections;
using UnityEngine;

public class BossHandController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float hoverDuration = 3f;
    [SerializeField] private float appearHeight = 5f;

    [Header("Damageable Settings")]
    [SerializeField] private bool isVulnerable = false;

    private Transform player;
    private Vector3 targetPosition;
    private bool isFalling = false;
    private BossController bossController; // Reference to the main boss controller

    private void Awake()
    {
        // Get components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        if (damageCollider == null)
            damageCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Initially disable collider
        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    // New method to set the player reference from BossController
    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        if (player == null)
        {
            Debug.LogError("BossHand: Player reference is null!");
        }
    }

    // Called by BossController to set up and start the hand attack
    public void PositionAndActivate()
    {
        // Ensure this game object is active before starting any coroutines
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        if (player != null)
        {
            // Position above player
            targetPosition = player.position;
            targetPosition.y += appearHeight;
            transform.position = targetPosition;

            // Start attack sequence
            StartCoroutine(HandAttackSequence());
        }
        else
        {
            // Fallback if player not found
            Debug.LogWarning("BossHand: Player not found! Trying to find player in scene.");

            // Last attempt to find player
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player != null)
            {
                // Position above player
                targetPosition = player.position;
                targetPosition.y += appearHeight;
                transform.position = targetPosition;

                // Start attack sequence
                StartCoroutine(HandAttackSequence());
            }
            else
            {
                Debug.LogError("BossHand: Player still not found! Cannot position hand.");
                Destroy(gameObject);
            }
        }
    }

    // Optional: Allow the BossController to set a reference to itself when spawning the hand
    public void SetBossController(BossController controller)
    {
        bossController = controller;
    }

    private IEnumerator HandAttackSequence()
    {
        // Play appear animation
        if (animator != null)
            animator.SetTrigger("Appear");

        // Wait for appear animation
        yield return new WaitForSeconds(1f);

        // Update target position (player might have moved)
        if (player != null)
        {
            targetPosition = player.position;
            targetPosition.y += 0.5f; // Slight offset so hand doesn't appear inside ground
        }

        // Start falling
        isFalling = true;
        if (animator != null)
            animator.SetTrigger("Fall");

        // Wait until hand reaches destination
        while (Mathf.Abs(transform.position.y - targetPosition.y) > 0.1f)
        {
            transform.position = new Vector3(
                transform.position.x, // Keep X the same
                Mathf.MoveTowards(transform.position.y, targetPosition.y, fallSpeed * Time.deltaTime), // Move only Y
                transform.position.z  // Keep Z the same
            );
            yield return null;
        }


        // Impact effect
        isFalling = false;
        // Enable collider for collision with player attacks
        if (damageCollider != null)
            damageCollider.enabled = true;

        // Make hand vulnerable to damage
        isVulnerable = true;

        // Stay on ground for a while
        yield return new WaitForSeconds(hoverDuration);

        // Disappear
        isVulnerable = false;
        // Wait for disappear animation
        yield return new WaitForSeconds(1f);

        // Destroy hand object
        Destroy(gameObject);
    }

    // Called when player damages the hand
    public void TakeDamage(int damage)
    {
        if (!isVulnerable) return;

        // Visual feedback
        StartCoroutine(FlashRed());

        // Apply damage to the main boss
        if (bossController != null)
        {
            bossController.TakeDamagePhase2(50);
        }
        else
        {
            Debug.LogWarning("BossHand: Cannot apply damage to main boss - BossController reference is null");
        }

    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If hand hits player while falling, damage them
        if (isFalling && collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20);
            }
        }
    }

    // Handle player attacks
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with a player attack
        if (isVulnerable && collision.CompareTag("PlayerAttack"))
        {
            // Get damage amount from the attack if available
            int damageAmount = 50; // Default damage
            TakeDamage(damageAmount);
        }else if (isVulnerable && collision.CompareTag("SpecialAttack"))
        {
            int damageAmount = 100; // Default damage
            TakeDamage(damageAmount);
        }
    }
}