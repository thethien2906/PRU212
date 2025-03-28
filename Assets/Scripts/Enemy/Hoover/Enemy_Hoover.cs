using UnityEngine;

public class Enemy_Hoover : Enemy
{
    [Header("Hoover Enemy Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float agrroRadius = 7f;
    [SerializeField] private float chaseDuration = 1f;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 10f;
    [SerializeField] private float chargeSpeed = 5f; // Speed when charging at player
    [SerializeField] private float groundHeight = 1f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 2f;

    private Animator animator;
    private bool isAttacking = false;
    private float attackTimer;
    private float defaultSpeed;
    private float chaseTimer;
    private float originalHeight;

    private Vector3 originalPosition;
    private Vector3 destination;

    private bool canDetectPlayer;
    private Collider2D target;

    private Vector3 hoverPointA;
    private Vector3 hoverPointB;
    private bool movingToB = true;
    private bool isCharging = false; // Changed from isDescending to isCharging

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();

        defaultSpeed = moveSpeed;
        originalPosition = transform.position;
        originalHeight = originalPosition.y; // Store original height

        Debug.Log("Original height: " + originalHeight);

        hoverPointA = originalPosition + new Vector3(-patrolDistance, 0, 0);
        hoverPointB = originalPosition + new Vector3(patrolDistance, 0, 0);

        destination = hoverPointB;
        canMove = true;
        attackTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();

        chaseTimer -= Time.deltaTime;

        if (idleTimer < 0)
            canDetectPlayer = true;

        HandlePlayerDetection();
        HandleMovement();

        // Decrease attack timer
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    private void HandlePlayerDetection()
    {
        if (canDetectPlayer)
        {
            Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, agrroRadius, whatIsPlayer);

            if (target == null && detectedPlayer != null)
            {
                AudioManager.instance.PlaySFX(45);
                Debug.Log("Player detected! Starting charge attack.");
                target = detectedPlayer;
                chaseTimer = chaseDuration;
                destination = target.transform.position; // Set destination directly to player position
                canDetectPlayer = false;
                canMove = true;
                isCharging = true; // Set charging flag
                animator.SetTrigger("Ready");
            }
            else if (target != null && detectedPlayer == null)
            {
                Debug.Log("Player lost! Returning to patrol.");
                target = null;
                isCharging = false; // Clear charging flag
                StopAttacking();
            }
        }
    }

    public void FireProjectile()
    {
        // Only fire if cooldown has elapsed
        if (attackTimer > 0)
        {
            Debug.Log("Attack on cooldown: " + attackTimer);
            return;
        }

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile Prefab or Fire Point not assigned!");
            return;
        }

        Debug.Log("Firing projectile!");
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        AudioManager.instance.PlaySFX(46);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = (target != null)
                ? (target.transform.position - firePoint.position).normalized
                : new Vector2(transform.localScale.x, 0); // Fire forward if no target

            rb.linearVelocity = direction * projectileSpeed;
        }

        // Set attack timer to cooldown
        attackTimer = attackCooldown;
    }

    private void HandleMovement()
    {
        if (isDead) return;

        if (!canMove)
            return;


        Vector3 currentPos = transform.position;

        if (target == null)
        {
            // Patrol mode
            moveSpeed = patrolSpeed;
            HandleFlip(destination.x);

            Vector3 newPosition = new Vector3(
                Vector2.MoveTowards(new Vector2(currentPos.x, 0), new Vector2(destination.x, 0), moveSpeed * Time.deltaTime).x,
                originalHeight,
                currentPos.z
            );
            transform.position = newPosition;

            if (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(destination.x, 0)) < 0.1f)
            {
                movingToB = !movingToB;
                destination = movingToB ? hoverPointB : hoverPointA;
                Debug.Log("Switching patrol direction to: " + destination);
            }
        }
        else
        {
            moveSpeed = isCharging ? chargeSpeed : attackSpeed;
            HandleFlip(destination.x);

            // Calculate dynamic stop distance based on colliders
            float buffer = 0.5f; // extra safety margin (e.g., 20cm world space)
            float stopDistance = 0.5f;

            if (target != null)
            {
                Collider2D playerCollider = target.GetComponent<Collider2D>();
                Collider2D enemyCollider = GetComponent<Collider2D>();

                if (playerCollider != null && enemyCollider != null)
                {
                    // Distance where colliders would start to overlap
                    stopDistance = (playerCollider.bounds.extents.x + enemyCollider.bounds.extents.x) + buffer;
                }
                else
                {
                    stopDistance = 1f; // fallback
                }
            }

            float distToTarget = Vector2.Distance(transform.position, destination);

            if (distToTarget > stopDistance)
            {
                Vector3 targetPos = Vector2.MoveTowards(
                    currentPos,
                    destination,
                    moveSpeed * Time.deltaTime
                );
                transform.position = targetPos;
            }
            else
            {
                Debug.Log("Enemy stopped outside player's collider range");
            }

            if (chaseTimer > 0 && target != null)
                destination = target.transform.position;

            Debug.Log($"Charging: pos={transform.position}, dest={destination}, stopDistance={stopDistance}");
        }


    }

    public void OnReadyAnimationComplete()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);
        Debug.Log("Ready animation complete, starting attack");
        // Reset attack timer when starting to attack
        attackTimer = 0f;
    }

    public void OnStopAttackAnimationComplete()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("Normal");
        Debug.Log("Stop attack animation complete");
    }

    private void StopAttacking()
    {
        canMove = true; // Allow movement again
        isAttacking = false;
        animator.SetTrigger("StopAttack");
        Debug.Log("Stopping attack");

        // Resume patrol by moving to the nearest patrol point
        float distToA = Vector2.Distance(transform.position, hoverPointA);
        float distToB = Vector2.Distance(transform.position, hoverPointB);
        destination = (distToA < distToB) ? hoverPointA : hoverPointB;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, agrroRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originalPosition + new Vector3(-patrolDistance, 0, 0), 0.2f);
        Gizmos.DrawWireSphere(originalPosition + new Vector3(patrolDistance, 0, 0), 0.2f);

        // Draw a line at ground height for debugging
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(transform.position.x - 5, groundHeight, 0),
                new Vector3(transform.position.x + 5, groundHeight, 0)
            );
        }
    }

    private void hideSprite()
    {
        sr.enabled = false;
    }
    public void CallDeactivateGameObject()
    {
        DeactivateGameObject();
    }

}