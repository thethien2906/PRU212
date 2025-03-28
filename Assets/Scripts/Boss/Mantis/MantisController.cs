using System.Collections;
using UnityEngine;

public class MantisBoss : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;
    private MantisHealth mantisHealth;
    private SpriteRenderer sr;

    [Header("Attack Parameters")]
    [SerializeField] public int attackType = 0; // 1 = Missile Attack, 2 = Gun Attack
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private float attack1Duration = 4f; // Duration for Attack 1 (missile)
    [SerializeField] private float attack2Duration = 2f; // Duration for Attack 2 (guns)

    [Header("Missile Settings")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float missileSpeed = 10f;
    [SerializeField] private int missileCount = 3; // Number of missiles to fire in Attack 1
    [SerializeField] private float missileDelay = 0.5f; // Time between missile firings

    [Header("Gun Settings")]
    [SerializeField] private Transform leftGunPoint;  // Left gun point
    [SerializeField] private Transform rightGunPoint; // Right gun point
    [SerializeField] private GameObject leftGunFireEffectPrefab;
    [SerializeField] private GameObject rightGunFireEffectPrefab;
    [SerializeField] private GameObject explosionPrefab; // Explosion effect prefab
    [SerializeField] private float bulletSpeed = 20f; // Bullet speed (for visual effect)
    [SerializeField] private LayerMask groundLayer; // Layer for ground detection
    [SerializeField] private float bulletAngle = 45f; // Angle of bullets in degrees
    [SerializeField] private int bulletCount = 3; // Number of bullets per gun
    [SerializeField] private float bulletDelay = 0.2f; // Delay between bullets
    private GameObject currentLeftGunEffect;
    private GameObject currentRightGunEffect;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform missilePoint; // Point where missiles are fired from
    [SerializeField] private LayerMask obstacleLayer;

    // Animation parameter names
    private readonly string WakeUpTrigger = "WakeUpTrigger";
    private readonly string AttackTrigger = "AttackTrigger";
    private readonly string AttackType = "AttackType";
    private readonly string IsAttacking = "isAttacking";
    private readonly string EndAttackTrigger = "EndAttackTrigger";

    // State tracking
    private bool isAwake = false;
    public bool isAttacking = false;
    private bool isCoolingDown = false;
    private int facingDir = -1; // 1 = right, -1 = left
    private Vector2 originalPosition; // Store original position for reset if needed
    // Player detection
    [SerializeField] private float detectionRange = 8f;
    private bool playerDetected = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mantisHealth = GetComponent<MantisHealth>();
        sr = GetComponent<SpriteRenderer>();

        // Configure rigidbody
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Store original position
        originalPosition = transform.position;

        // Find player if not set
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (mantisHealth.currentHealth <= 0) return;
        // Check if player is in detection range
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Player detection logic
            if (distanceToPlayer < detectionRange && !isAwake && !playerDetected)
            {
                AudioManager.instance.PlaySFX(47);
                playerDetected = true;
                StartCoroutine(WakeUp());
            }
        }

        // If awake and not currently in an attack or cooldown, decide next attack
        if (isAwake && !isAttacking && !isCoolingDown)
        {
            DecideNextAttack();
        }
    }

    private IEnumerator WakeUp()
    {
        // Trigger wake up animation
        animator.SetTrigger(WakeUpTrigger);

        // Wait for animation to complete (adjust time based on your animation length)
        yield return new WaitForSeconds(2f);

        isAwake = true;
    }

    private void DecideNextAttack()
    {
        // Choose attack type randomly between 1 and 2
        attackType = Random.Range(1, 3); // 1 or 2

        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Set attack parameters
        animator.SetInteger(AttackType, attackType);
        AudioManager.instance.PlaySFX(49);

        animator.SetTrigger(AttackTrigger);
        animator.SetBool(IsAttacking, true);

        // Face the player
        FacePlayer();

        // Wait for prepare animation (adjust based on your animation timing)
        yield return new WaitForSeconds(1f);

        // Get current animation state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Attack duration depends on which attack is being performed
        float attackDuration = (attackType == 1) ? attack1Duration : attack2Duration;

        Debug.Log("Performing Attack " + attackType + " for " + attackDuration + " seconds");

        // Note: FireMissile or FireBullets methods will be called by animation events

        // Wait for attack animation to play for the specified duration
        yield return new WaitForSeconds(attackDuration);

        // End attack
        animator.SetBool(IsAttacking, false);
        if (attackType == 2)
        {
            CleanupGunEffects();
            Debug.Log("Clean up guns");
        }
           
        // Wait for end animation to complete
        yield return new WaitForSeconds(1f);

        // Reset and start cooldown
        isAttacking = false;
        StartCoroutine(StartCooldown());
    }

    // Animation Event: Called from Attack 1 animation
    public void FireMissile()
    {
        if (player == null) return;
        AudioManager.instance.PlaySFX(22);
        StartCoroutine(FireMissileSequence());
    }

    private IEnumerator FireMissileSequence()
    {
        // Fire multiple missiles with delay
        for (int i = 0; i < missileCount; i++)
        {
            // Instantiate the missile at missilePoint position
            GameObject missile = Instantiate(missilePrefab, missilePoint.position, Quaternion.identity);

            // Get the missile rigidbody
            Rigidbody2D missileRb = missile.GetComponent<Rigidbody2D>();

            // Fix the missile initial rotation (assuming up is the correct facing direction)
            missile.transform.rotation = Quaternion.Euler(0, 0, 90);

            // First make it fly straight up
            Vector2 initialDirection = Vector2.up; // Flies upward initially
            missileRb.linearVelocity = initialDirection * missileSpeed;

            // After delay, change direction to target the player
            StartCoroutine(RedirectMissile(missile, missileRb));

            // Wait before firing next missile
            yield return new WaitForSeconds(missileDelay);
        }
    }

    private IEnumerator RedirectMissile(GameObject missile, Rigidbody2D missileRb)
    {
        // Wait for delay before redirecting
        yield return new WaitForSeconds(0.25f);

        // Make sure missile and player still exist
        if (missile != null && player != null)
        {
            // Calculate direction towards the player
            Vector2 targetDirection = (player.position - missile.transform.position).normalized;

            // Set new velocity towards player
            missileRb.linearVelocity = targetDirection * missileSpeed;

            // Rotate missile to face the direction it's moving
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            missile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Animation Event: Called from Attack 2 animation
    public void FireBullets()
    {
        StartCoroutine(FireBulletsSequence());
    }

    private IEnumerator FireBulletsSequence()
    {
        // Fire multiple bullets from each gun with delay
        for (int i = 0; i < bulletCount; i++)
        {
            // Fire from both guns
            FireBulletFromGun(leftGunPoint);
            FireBulletFromGun(rightGunPoint);

            // Wait before firing next bullet
            yield return new WaitForSeconds(bulletDelay);
        }
    }
    // Animation Event: Called at the very first frame of Attack 2 animation
    public void ActivateGunEffects()
    {
        if (leftGunPoint != null && rightGunPoint != null)
        {
            if (currentLeftGunEffect == null)
            {
                currentLeftGunEffect = Instantiate(leftGunFireEffectPrefab, leftGunPoint.position, Quaternion.identity, leftGunPoint);
            }
            else
            {
                AudioManager.instance.PlaySFXLoopedByDuration(48, 2);
                currentLeftGunEffect.SetActive(true); // Reactivate
            }

            if (currentRightGunEffect == null)
            {
                currentRightGunEffect = Instantiate(rightGunFireEffectPrefab, rightGunPoint.position, Quaternion.identity, rightGunPoint);
            }
            else
            {
                AudioManager.instance.PlaySFXLoopedByDuration(48, 2);
                currentRightGunEffect.SetActive(true); // Reactivate
            }
        }
    }


    private void CleanupGunEffects()
    {
        if (currentLeftGunEffect != null)
        {
            currentLeftGunEffect.SetActive(false);
        }

        if (currentRightGunEffect != null)
        {
            currentRightGunEffect.SetActive(false);
        }
    }



    private void FireBulletFromGun(Transform gunPoint)
    {
        Debug.Log("Firing form" + gunPoint);
        // Calculate the direction based on facing direction and angle
        float angleInRadians = (Random.Range(bulletAngle-10f, bulletAngle + 10f)) * Mathf.Deg2Rad;
        Vector2 direction = (facingDir > 0)
            ? new Vector2(Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians))
            : new Vector2(-Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians));

        // Perform raycast
        Vector2 rayStart = new Vector2(gunPoint.position.x, gunPoint.position.y + 1.5f); // Add an offset (e.g., 0.5f)

        // Perform raycast with an offset to make it hit slightly above the ground
        RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, 30f, groundLayer);

        if (hit.collider != null)
        {
            Debug.DrawRay(gunPoint.position, direction * hit.distance, Color.red, 1f);

            // Spawn explosion prefab at the hit point
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, hit.point, Quaternion.identity);
            }
        }
        else
        {
            Debug.DrawRay(gunPoint.position, direction * 30f, Color.blue, 1f);
        }
    }


    private void FacePlayer()
    {
        // Determine which direction to face based on player position
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Face right (adjust if your sprite faces the opposite direction)
            facingDir = 1;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); // Face left
            facingDir = -1;
        }
    }

    private IEnumerator StartCooldown()
    {
        isCoolingDown = true;

        // Wait for cooldown duration
        yield return new WaitForSeconds(cooldownTime);

        isCoolingDown = false;
    }

    // Optional: Method to reset boss position
    public void ResetPosition()
    {
        transform.position = originalPosition;
        rb.linearVelocity = Vector2.zero;
    }

    // Optional: Method to hide sprite (can be called from animation event)
    private void hideSprite()
    {
        sr.enabled = false;
    }

    // Optional: Method to show sprite (can be called from animation event)
    private void showSprite()
    {
        sr.enabled = true;
    }

    // Optional: Player collision damage
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
            Mana playerMana = other.transform.root.GetComponent<Mana>();
            if (playerMana != null)
            {
                playerMana.GainManaOnHit(10);
            }
            mantisHealth.TakeDamage(10);
            Collider2D col = other.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
        else if (other.CompareTag("SpecialAttack"))
        {
            mantisHealth.TakeDamage(30);
        }
    }
    public void TriggerBullet(string gun)
    {
        if (gun == "Left")
            FireBulletFromGun(leftGunPoint);
        else if (gun == "Right")
            FireBulletFromGun(rightGunPoint);
    }

    // For debugging: Draw detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw gun fire directions
        if (leftGunPoint != null && rightGunPoint != null)
        {
            float angleInRadians = bulletAngle * Mathf.Deg2Rad;

            // Right-facing directions
            Vector2 rightDirection = new Vector2(Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians));
            // Left-facing directions
            Vector2 leftDirection = new Vector2(-Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians));

            Gizmos.color = Color.cyan;
            // Draw using current facing direction
            if (facingDir > 0)
            {
                Gizmos.DrawRay(leftGunPoint.position, rightDirection * 5f);
                Gizmos.DrawRay(rightGunPoint.position, rightDirection * 5f);
            }
            else
            {
                Gizmos.DrawRay(leftGunPoint.position, leftDirection * 5f);
                Gizmos.DrawRay(rightGunPoint.position, leftDirection * 5f);
            }
        }
    }
}