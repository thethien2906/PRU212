using UnityEngine;

public class Enemy_Hoover : Enemy
{
    [Header("Hoover Enemy Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float agrroRadius = 7f;
    [SerializeField] private float chaseDuration = 1f;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; 
    [SerializeField] private Transform firePoint; 
    [SerializeField] private float projectileSpeed = 2f; 

    private Animator animator;
    private bool isAttacking = false;
    private float attackTimer;
    private float defaultSpeed;
    private float chaseTimer;

    private Vector3 originalPosition;
    private Vector3 destination;

    private bool canDetectPlayer;
    private Collider2D target;

    private Vector3 hoverPointA;
    private Vector3 hoverPointB;
    private bool movingToB = true;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();

        defaultSpeed = moveSpeed;
        originalPosition = transform.position;

        hoverPointA = originalPosition + new Vector3(-patrolDistance, 0, 0);
        hoverPointB = originalPosition + new Vector3(patrolDistance, 0, 0);

        destination = hoverPointB;
        canMove = true;
    }

    protected override void Update()
    {
        base.Update();

        chaseTimer -= Time.deltaTime;

        if (idleTimer < 0)
            canDetectPlayer = true;

        HandleMovement();
        HandlePlayerDetection();

        if (isAttacking && attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    private void HandlePlayerDetection()
    {
        if (canDetectPlayer)
        {
            Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, agrroRadius, whatIsPlayer);

            if (target == null && detectedPlayer != null)
            {
                target = detectedPlayer;
                chaseTimer = chaseDuration;
                destination = target.transform.position;
                canDetectPlayer = false;
                canMove = true;
                animator.SetTrigger("Ready");
            }
            else if (target != null && detectedPlayer == null)
            {
                target = null;
                StopAttacking();
            }
        }
    }
    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile Prefab or Fire Point not assigned!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = (target != null)
                ? (target.transform.position - firePoint.position).normalized
                : new Vector2(transform.localScale.x, 0); // Fire forward if no target

            rb.linearVelocity = direction * projectileSpeed;
        }
    }
    private void HandleMovement()
    {
        if (!canMove)
            return;

        if (target == null)
        {
            // **Hovering Mode**
            moveSpeed = patrolSpeed;
            HandleFlip(destination.x);
            transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, destination) < 0.1f)
            {
                movingToB = !movingToB;
                destination = movingToB ? hoverPointB : hoverPointA;
            }
        }
        else
        {
            // **Chase Mode**
            moveSpeed = attackSpeed;
            HandleFlip(destination.x);
            transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

            if (chaseTimer > 0 && target != null)
                destination = target.transform.position;
        }
    }

    public void OnReadyAnimationComplete()
    {
        animator.SetBool("isAttacking", true);
    }

    public void OnStopAttackAnimationComplete()
    {
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("Normal");
    }

    private void StopAttacking()
    {
        canMove = true; // Allow movement again
        isAttacking = false;
        animator.SetTrigger("StopAttack");

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
    }
    private void hideSprite()
    {
        sr.enabled = false;
    }
}
