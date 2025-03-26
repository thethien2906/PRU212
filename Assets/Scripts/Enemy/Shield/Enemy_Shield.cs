using UnityEngine;

public class Enemy_Shield : Enemy
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float throwAngle = 45f;

    private bool isAttacking;
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        lastAttackTime = -attackCooldown; // Allow attacking immediately on first detection
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (isPlayerDetected && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            StartParry();
            lastAttackTime = Time.time;
        }
        else if (!isAttacking)
        {
            HandleMovement();
            if (isGrounded) HandleTurnAround();
        }
    }

    private void StartParry()
    {
        isAttacking = true;
        anim.SetTrigger("Parry");
        rb.linearVelocity = Vector2.zero;
        Invoke(nameof(StartAttack), 2f);
    }

    private void StartAttack()
    {
        anim.SetTrigger("Attack");
    }

    private void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);
        EnemyGrenade grenadeScript = grenade.GetComponent<EnemyGrenade>();
        grenadeScript.Throw(facingDir, throwForce, throwAngle);
    }

    private void StopThrowGrenade()
    {
        isAttacking = false;
        anim.SetTrigger("StopAttack");
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