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
        if (isDead)
        {
            Debug.Log("Gun is dead, dont do anything");
        }


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
        AudioManager.instance.PlaySFX(16);
        isAttacking = true;
        anim.SetTrigger("Attack"); // Trigger enemy attack animation
        rb.linearVelocity = Vector2.zero; // Stop movement

    }
    public void TriggerProjectile()
    {
        if (isDead)
        {
            Debug.Log("Gun is dead, dont do fire");
            return;
        }
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
        if (isDead)
        {
            Debug.Log("Gun is dead, dont do stop projectile");
            return;
        }
        Invoke(nameof(StopAttack), 0.3f);
    }

    private void ShootProjectile()
    {
        if (isDead) return;
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
