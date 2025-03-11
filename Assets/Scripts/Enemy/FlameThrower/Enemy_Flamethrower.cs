using UnityEngine;

public class Enemy_Flamethrower : Enemy
{
    [Header("Attack Parameters")]
    [SerializeField] private GameObject fireObject; // Assign in Inspector

    private Animator flameAnimator;
    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();
        flameAnimator = fireObject.GetComponent<Animator>(); // Get the animator from FlameAttack
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (isPlayerDetected && !isAttacking)
        {
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
        anim.SetTrigger("Attack"); // Trigger enemy attack animation
        rb.linearVelocity = Vector2.zero; // Stop movement

    }

    public void TriggerFlame()
    {
        fireObject.SetActive(true);
        flameAnimator.SetTrigger("FlameOn"); // Play flame animation
    }

    public void StopFlame()
    {
        flameAnimator.SetTrigger("FlameOff"); // Stop flame animation
        Invoke(nameof(DeactivateFlame), 0.3f); // Small delay to smoothly fade out
    }

    private void DeactivateFlame()
    {
        fireObject.SetActive(false);
        isAttacking = false; // Ready for next attack
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
}
