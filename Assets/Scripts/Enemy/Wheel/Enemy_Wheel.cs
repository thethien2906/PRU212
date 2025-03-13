using UnityEngine;

public class Enemy_Wheel : Enemy
{
    private bool isAttacking;
    private float attackCooldownTimer;
    private float attackDuration = 2f; // Time spent rushing
    private float attackCooldown = 2f; // Cooldown before attacking again
    private float attackSpeedMultiplier = 2.5f; // Speed increase during attack

    [SerializeField] private GameObject glowingObject;
    private Animator glowingAnimator;

    protected override void Awake()
    {
        base.Awake();
        glowingAnimator = glowingObject.GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (isAttacking) return;

        if (isPlayerDetected && attackCooldownTimer <= 0)
        {
            TriggerGlowing();  // Activate glow before attacking
            Invoke(nameof(StartAttack), 0.5f); // Delay attack slightly for animation effect
        }
        else
        {
            HandleMovement();
            if (isGrounded) HandleTurnAround();
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        anim.SetBool("isAttacking", true);
        glowingObject.SetActive(true); // Ensure glow is active before rushing

        rb.linearVelocity = new Vector2(moveSpeed * attackSpeedMultiplier * facingDir, rb.linearVelocity.y);
        Invoke(nameof(EndAttack), attackDuration);
    }

    private void EndAttack()
    {
        isAttacking = false;
        attackCooldownTimer = attackCooldown;

        anim.SetBool("isAttacking", false);
        glowingObject.SetActive(false);

        rb.linearVelocity = Vector2.zero; // Stop movement after attack
    }

    private void TriggerGlowing()
    {
        glowingObject.SetActive(true);
    }

    private void HandleTurnAround()
    {
        if (!isGroundInfrontDetected || isWallDetected || !isGrounded)
        {
            Flip();
            idleTimer = idleDuration;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        if (idleTimer > 0 || isAttacking) return;
        rb.linearVelocity = new Vector2(moveSpeed * facingDir, rb.linearVelocity.y);
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(30);
            }
        }
    }
}
