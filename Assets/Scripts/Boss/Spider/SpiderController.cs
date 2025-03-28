using System.Collections;
using UnityEngine;

public class SpiderBoss : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;
    private SpiderHealth spiderHealth;
    [Header("Attack Parameters")]
    [SerializeField] private int attackType = 0; // 0 = Rush, 1 = Shoot (Grenade)
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private float rushSpeed = 15f;
    [SerializeField] private float rushDuration = 1.5f;

    [Header("Grenade Parameters")]
    [SerializeField] private int grenadeCount = 3;
    [SerializeField] private float grenadeDelay = 0.5f;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwAngle = 45f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private Transform rushingPoint;
    [SerializeField] private LayerMask obstacleLayer; // Add this to detect walls

    // Animation parameter names
    private readonly string IntroTrigger = "IntroTrigger";
    private readonly string AttackTrigger = "AttackTrigger";
    private readonly string AttackType = "AttackType";
    private readonly string IsCoolingDown = "isCoolingDown";

    // State tracking
    private bool isIntroComplete = false;
    private bool isAttacking = false;
    private bool isCoolingDown = false;
    private int facingDir = 1; // 1 = right, -1 = left
    private Vector2 originalPosition; // Store original position for reset if needed

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spiderHealth = GetComponent<SpiderHealth>();
        // Configure rigidbody to be immovable by external forces
        rb.gravityScale = 0;
        rb.mass = 1000; // Very high mass
        rb.linearDamping = 1000; // Very high drag
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation

        // Store original position
        originalPosition = transform.position;

        // Find player if not set
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        AudioManager.instance.PlaySFX(53);
        // Start with intro animation
        StartCoroutine(PlayIntro());
    }

    void Update()
    {
        if (!isIntroComplete || isAttacking || isCoolingDown)
            return;

        // Choose attack based on distance to player or other logic
        DecideNextAttack();
    }

    private IEnumerator PlayIntro()
    {
        animator.SetTrigger(IntroTrigger);

        // Wait for animation to complete
        yield return new WaitForSeconds(2f); // Adjust time based on animation length

        isIntroComplete = true;
    }

    private void DecideNextAttack()
    {
        // Choose attack type based on distance or random
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        //if (distanceToPlayer < 5f)
        //{
        //    attackType = 0; // Rush when close
        //}
        //else
        //{
        //    attackType = 1; // Shoot grenades when far
        //}

        //// Or use random choice
        attackType = Random.Range(0, 2);

        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Set attack type and trigger
        animator.SetInteger(AttackType, attackType);
        animator.SetTrigger(AttackTrigger);

        // Only wait a very short time to ensure animation has started
        yield return new WaitForSeconds(1f);

        // Get animation state info to sync with animation
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (attackType == 0) // Rush attack
        {
            AudioManager.instance.PlaySFX(54);
            // Get animation length
            float rushAnimLength = stateInfo.length * 0.8f; // Use 80% of animation for actual rush

            // Do rush attack synchronized with animation
            yield return StartCoroutine(PerformRushAttack(rushAnimLength));
        }
        else // Grenade attack
        {

            yield return StartCoroutine(PerformGrenadeAttack());
        }

        // Only a minimal wait before cooldown
        yield return new WaitForSeconds(0.1f);

        // Start cooldown
        StartCoroutine(StartCooldown());
    }

    private IEnumerator PerformRushAttack(float duration)
    {
        // Calculate direction to player but only use horizontal component
        Vector2 playerDirection = (player.position - transform.position).normalized;
        Vector2 direction = new Vector2(playerDirection.x, 0).normalized; // Only move horizontally

        // Face the player
        FacePlayer();

        // Store original position before rushing
        Vector2 startPosition = transform.position;
        float rushTimer = 0;

        // Reference to the boss collider
        Collider2D bossCollider = GetComponent<Collider2D>();
        Vector2 colliderSize = bossCollider.bounds.size;

        // Make the rush happen during the animation, not after
        while (rushTimer < duration)
        {
            rushTimer += Time.deltaTime;
            float normalizedTime = rushTimer / duration; // 0 to 1 progress

            // Use a curve to make start and end smoother
            float speedMultiplier = Mathf.Sin(normalizedTime * Mathf.PI);

            // Calculate movement delta
            float moveDelta = direction.x * rushSpeed * speedMultiplier * Time.deltaTime;

            // Calculate next position
            Vector3 nextPosition = transform.position;
            nextPosition.x += moveDelta;

            // Check for obstacles using a boxcast (more reliable than raycast for tilemaps)
            RaycastHit2D hit = Physics2D.BoxCast(
                transform.position,       // Origin
                colliderSize * 0.9f,      // Size (slightly smaller than collider)
                0f,                       // Angle
                direction,                // Direction
                Mathf.Abs(moveDelta) + 0.1f, // Distance (movement + small buffer)
                obstacleLayer             // Layer mask
            );

            if (hit.collider != null)
            {
                AudioManager.instance.PlaySFX(33);
                // Hit a wall or obstacle, stop rushing
                Debug.Log("Hit obstacle: " + hit.collider.name);
                break;
            }

            // Move the boss (only horizontally)
            transform.position = nextPosition;

            // Check if we hit the player
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, colliderSize.x * 0.5f);
            foreach (Collider2D collider in hits)
            {
                if (collider.CompareTag("Player"))
                {
                    AudioManager.instance.PlaySFX(33);
                    // Deal damage to player
                    Health playerHealth = collider.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(1);
                    }

                    // Push player a bit (horizontally)
                    Rigidbody2D playerRb = collider.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 pushDirection = new Vector2(direction.x, 0.2f); // Slight upward push
                        playerRb.AddForce(pushDirection.normalized * 10f, ForceMode2D.Impulse);
                    }

                    break;
                }
            }

            yield return null;
        }

        // Make sure velocity is zero
        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator PerformGrenadeAttack()
    {
        // Face the player
        FacePlayer();

        // Ensure the boss doesn't move during grenade throws
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        // Throw multiple grenades
        for (int i = 0; i < grenadeCount; i++)
        {
            AudioManager.instance.PlaySFX(52);
            ThrowGrenade();
            yield return new WaitForSeconds(grenadeDelay);
        }

        // Return to normal physics state
        rb.isKinematic = false;
    }

    private void FacePlayer()
    {
        // Determine which direction to face based on player position
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Face right
            facingDir = 1;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); // Face left
            facingDir = -1;
        }
    }

    private void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, shootingPoint.position, Quaternion.identity);
        EnemyGrenade grenadeScript = grenade.GetComponent<EnemyGrenade>();

        // Calculate the angle based on distance to player for better targeting
        Vector2 toPlayer = player.position - shootingPoint.position;
        float distance = toPlayer.magnitude;

        // Optional: Adjust angle based on distance
        float adjustedAngle = throwAngle;
        if (distance > 8f)
        {
            adjustedAngle = Mathf.Lerp(throwAngle, 60f, (distance - 8f) / 4f);
        }

        // Make sure throwing the grenade doesn't affect the boss's position
        grenadeScript.Throw(facingDir, throwForce, adjustedAngle);
    }

    private IEnumerator StartCooldown()
    {
        isAttacking = false;
        isCoolingDown = true;

        animator.SetBool(IsCoolingDown, true);

        // Reset velocity just to be safe
        rb.linearVelocity = Vector2.zero;

        // Wait for cooldown duration
        yield return new WaitForSeconds(cooldownTime);

        isCoolingDown = false;
        animator.SetBool(IsCoolingDown, false);
    }

    // Optional: Method to reset boss position if it somehow gets stuck
    public void ResetPosition()
    {
        transform.position = originalPosition;
        rb.linearVelocity = Vector2.zero;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
        }
        else if (other.CompareTag("PlayerAttack"))
        {
            AudioManager.instance.PlaySFX(50);
            spiderHealth.TakeDamage(10);
            
        }
        else if (other.CompareTag("SpecialAttack"))
        {
            spiderHealth.TakeDamage(30);
        }
    }
}