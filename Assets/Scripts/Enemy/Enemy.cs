using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer sr => GetComponent<SpriteRenderer>();
    protected Transform player;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D[] colliders;

    [Header("General info")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float idleDuration = 1.5f;
    protected float idleTimer;
    protected bool canMove = true;

    [Header("Health System")]
    [SerializeField] protected int health = 3; // Default value, can be overridden by subclasses

    [Header("Death details")]
    [SerializeField] protected float deathImpactSpeed = 5;
    [SerializeField] protected float deathRotationSpeed = 150;
    protected int deathRotationDirection = 1;
    protected bool isDead;

    [Header("Basic collision")]
    [SerializeField] protected float groundCheckDistance = 1.1f;
    [SerializeField] protected float wallCheckDistance = .7f;
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected float playerDetectionDistance = 15;
    [SerializeField] protected LayerMask whatIsPlayer;
    [SerializeField] protected Transform groundCheck;
    protected bool isPlayerDetected;
    protected bool isGrounded;
    protected bool isWallDetected;
    protected bool isGroundInfrontDetected;

    protected int facingDir = 1;
    protected bool facingRight = true;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    protected virtual void Start()
    {
        InvokeRepeating(nameof(UpdatePlayersRef), 0, 1);
    }

    private void UpdatePlayersRef()
    {
        if (player == null)
            player = GameManager.instance.player.transform;
    }

    protected virtual void Update()
    {
        HandleCollision();
        HandleAnimator();

        idleTimer -= Time.deltaTime;

    }

    // New Method: Handles damage when hit by player
    public void TakeDamage()
    {
        if (isDead) return;

        health--;

        if (health <= 0)
        {
            Die();
        }
    }

    // Modified Die() Method
    public virtual void Die()
    {
        anim.SetTrigger("isDead");
        isDead = true;
        Destroy(gameObject, 10);
    }


    protected virtual void HandleFlip(float xValue)
    {
        if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight)
            Flip();
    }

    protected virtual void Flip()
    {
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    protected virtual void HandleAnimator()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);

        Vector3 frontCheckPos = groundCheck.position + (Vector3.right * facingDir * 0.5f);
        isGroundInfrontDetected = Physics2D.Raycast(frontCheckPos, Vector2.down, groundCheckDistance, whatIsGround);

        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        
        // if gameobject name = Enemy_Missile then it will detech from the left
        if (gameObject.name == "Enemy_Missile")
        {
            isPlayerDetected = Physics2D.Raycast(transform.position, Vector2.left * facingDir, playerDetectionDistance, whatIsPlayer);
        }
        else
        {
            isPlayerDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, playerDetectionDistance, whatIsPlayer);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * facingDir * wallCheckDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * facingDir * playerDetectionDistance);

        // if gameobject name = Enemy_Missile than both will draw on the left
        if (gameObject.name == "Enemy_Missile")
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * facingDir * playerDetectionDistance);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Check if collided with Player
        {
            Health playerHealth = other.GetComponent<Health>(); // Get Health component

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10); // Reduce Player Health by 10
            }
        }
    }

}
