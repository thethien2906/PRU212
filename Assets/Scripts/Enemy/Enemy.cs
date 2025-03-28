using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr => GetComponent<SpriteRenderer>();
    protected Transform player;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D[] colliders;
    private Flash flashEffect;

    [Header("General info")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float idleDuration = 1.5f;
    protected float idleTimer;
    protected bool canMove = true;

    [Header("Health System")]
    [SerializeField] protected int health = 3;

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
        rb.bodyType = RigidbodyType2D.Kinematic;
        colliders = GetComponentsInChildren<Collider2D>();
        flashEffect = GetComponent<Flash>(); // Add this line to get Flash component
    }

    protected virtual void Start()
    {
        //wait 2 seconds before update player refs
        Invoke("UpdatePlayersRef", 2f);
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

    public void TakeDamage()
    {
        if (isDead) return;

        health--;

        if (flashEffect != null)
        {
            flashEffect.FlashSprite(); // Trigger flash effect when taking damage
        }

        if (health <= 0)
        {
            AudioManager.instance.PlaySFXwithRandomPitch(UnityEngine.Random.Range(43, 45));
            Die();
        }
    }

    public virtual void Die()
    {
        anim.SetTrigger("isDead");
        Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.isKinematic = true;
        }
        Destroy(gameObject, 1f);
        isDead = true;
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

        if (gameObject.name == "Enemy_Missile")
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * facingDir * playerDetectionDistance);
        }
    }
    public void DeactivateGameObject()
    {
        gameObject.SetActive(false);
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
            AudioManager.instance.PlaySFX(42);
            Mana playerMana = other.transform.root.GetComponent<Mana>();
            if (playerMana != null)
            {
                playerMana.GainManaOnHit(10);
            }
            TakeDamage();
            Collider2D col = other.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
        else if (other.CompareTag("SpecialAttack"))
        {
            // Disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Freeze Rigidbody2D position
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            Die();
        }
    }
}
