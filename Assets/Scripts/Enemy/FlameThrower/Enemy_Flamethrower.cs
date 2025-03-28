using UnityEngine;

public class Enemy_Flamethrower : Enemy
{
    [Header("Attack Parameters")]
    [SerializeField] private GameObject fireObject; // Assign in Inspector
    [SerializeField] private float attackCooldown = 3f; // Cooldown time in seconds

    private Animator flameAnimator;
    private bool isAttacking;
    private float cooldownTimer;

    protected override void Awake()
    {
        base.Awake();
        flameAnimator = fireObject.GetComponent<Animator>(); // Get the animator from FlameAttack
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        cooldownTimer -= Time.deltaTime;

        if (isPlayerDetected && !isAttacking && cooldownTimer <= 0)
        {
            AudioManager.instance.PlaySFX(15);
            StartAttack();
        }

        if (!isAttacking)
        {
            HandleMovement();
            if (isGrounded) HandleTurnAround();
        }
    }

    private void StartAttack()
    {


        isAttacking = true;
        cooldownTimer = attackCooldown; // Reset cooldown timer
        anim.SetTrigger("Attack"); // Trigger enemy attack animation
        rb.linearVelocity = Vector2.zero; // Stop movement
    }

    public void TriggerFlame()
    {
        if (isDead) return;
        fireObject.SetActive(true);
        flameAnimator.SetTrigger("FlameOn"); // Play flame animation
    }

    public void StopFlame()
    {
        if (isDead) return;
        flameAnimator.SetTrigger("FlameOff"); // Stop flame animation
        Invoke(nameof(DeactivateFlame), 0.1f); // Small delay to smoothly fade out
    }

    private void DeactivateFlame()
    {
        fireObject.SetActive(false);
        isAttacking = false; // Ready for next attack after cooldown
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

    private void hideSprite()
    {
        sr.enabled = false;
    }
    public void CallDeactivateGameObject()
    {
        DeactivateGameObject();
    }
}
