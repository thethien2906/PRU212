using UnityEngine;

public class Enemy_Gun : Enemy
{
    [Header("Attack Parameters")]
    [SerializeField] private GameObject gunOpenFireAnimation; 
    [SerializeField] private GameObject projectilePrefab; 
    [SerializeField] private Transform firePoint; 

    private Animator projectileAnimator;
    private bool isAttacking;


    protected override void Awake()
    {
        base.Awake();
        projectileAnimator = gunOpenFireAnimation.GetComponent<Animator>(); 
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (isPlayerDetected && !isAttacking)
        {
            // attack every 1 second
           StartAttack();
        }
        if (!isAttacking)
        {
            HandleMovement();
            if (isGrounded) HandleTurnAround();
        }
    }
    public override void Die()
    {
        if (isDead) return; // Prevent multiple calls
        isDead = true;

        // Stop any ongoing attack animations
        isAttacking = false;
        anim.ResetTrigger("Attack"); // Ensure attack animation stops
        anim.SetTrigger("isDead");

        // Disable movement
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        // Hide projectile effects if active
        if (gunOpenFireAnimation != null)
        {
            gunOpenFireAnimation.SetActive(false);
        }

        Destroy(gameObject, 1f);
    }

    private void StartAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack"); // Trigger enemy attack animation
        rb.linearVelocity = Vector2.zero; // Stop movement

    }
    public void TriggerProjectile()
    {
        if (isDead) return;
        if (projectileAnimator != null) {
            gunOpenFireAnimation.SetActive(true);
            ShootProjectile();
        }
        else
        {
            return;
        }
        
    }

    public void StopProjectile()
    {
        Invoke(nameof(StopAttack), 0.3f);
    }

    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        projScript.SetDirection(facingDir); // Ensure projectile moves in the right direction
    }

    private void StopAttack()
    {   if (gunOpenFireAnimation==null)
        {
            return;
        }
        else
        {
            
            gunOpenFireAnimation.SetActive(false);
            isAttacking = false;
            anim.SetTrigger("StopAttack");
        }
    }



    private void HandleTurnAround()
    {
        if (isDead) return;
        if (!isGroundInfrontDetected || isWallDetected || !isGrounded)
        {
            Flip();
            idleTimer = idleDuration;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        if (isDead) return;
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
