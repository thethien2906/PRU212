using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Missile : Enemy
{
    [Header("Missile Settings")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float missileSpeed = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float missileCooldown = 1.5f; // Adjust this value as needed
    private float missileTimer = 0f;
    private Animator animator;
    private bool isAttacking = false;
    private float attackTimer;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        // Handle animation states based on player detection
        HandleAnimationStates(isPlayerDetected);

        // Handle attack cooldown if necessary
        if (isAttacking && attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // Handle missile cooldown
        if (missileTimer > 0)
        {
            missileTimer -= Time.deltaTime;
        }
    }

    private void HandleAnimationStates(bool playerDetected)
    {
        // If player is detected and we're not already attacking
        if (playerDetected && !isAttacking)
        {
            // Trigger the "Ready" animation
            animator.SetTrigger("Ready");
            isAttacking = true;
            attackTimer = attackCooldown;
        }
        // If player is no longer detected and we are attacking
        else if (!playerDetected && isAttacking)
        {
            // Trigger the "StopAttack" animation
            animator.SetTrigger("StopAttack");
            isAttacking = false;
        }
    }

    // Called by animation event from EnemyReadyForAttack when it completes
    public void OnReadyAnimationComplete()
    {
        // Set Attacking to true to transition to EnemyLaunch
        animator.SetBool("isAttacking", true);
    }

    // Called by animation event from EnemyStopAttack when it completes
    public void OnStopAttackAnimationComplete()
    {
        // Reset the Attacking parameter
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("Normal");
    }

    // Animation Event: Call this at the START of "EnemyLaunch" 
    // Animation Event: Call this at the START of "EnemyLaunch" 
    public void FireMissile()
    {
        if (player == null || missileTimer > 0) return; // Skip if on cooldown
        missileTimer = missileCooldown; // Start cooldown

        // Instantiate the missile at firePoint position
        GameObject missile = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);

        // Get the missile rigidbody
        Rigidbody2D missileRb = missile.GetComponent<Rigidbody2D>();

        // Fix the missile initial rotation (assuming up is the correct facing direction)
        // If your sprite faces up when rotation is 0, you can remove this line
        missile.transform.rotation = Quaternion.Euler(0, 0, 90);

        // First make it fly straight up
        Vector2 initialDirection = Vector2.up; // Flies upward initially
        missileRb.linearVelocity = initialDirection * missileSpeed;

        // After 1 second, change direction to target the player
        StartCoroutine(RedirectMissile(missile, missileRb));
    }

    private IEnumerator RedirectMissile(GameObject missile, Rigidbody2D missileRb)
    {
        // Wait for 1 second
        yield return new WaitForSeconds(0.25f);

        // Make sure missile and player still exist
        if (missile != null && player != null)
        {
            // Calculate direction towards the player
            Vector2 targetDirection = (player.position - missile.transform.position).normalized;

            // Set new velocity towards player
            missileRb.linearVelocity = targetDirection * missileSpeed;

            // Rotate missile to face the direction it's moving
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            missile.transform.rotation = Quaternion.Euler(0, 0, angle);
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