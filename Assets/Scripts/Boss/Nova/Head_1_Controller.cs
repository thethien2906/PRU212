using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Head1Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform laserSpawnPoint; // New dedicated spawn point for laser
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private BossController bossController;

    [Header("Attack Settings")]
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float vulnerableTime = 2f;
    [SerializeField] private float laserDuration = 1.5f;
    [SerializeField] private Vector2 laserDirection = Vector2.left; // Direction for the laser
    private Flash flashEffect;


    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider;
    public UnityEvent onDamaged;
    public UnityEvent onHealthBelowHalf;
    public UnityEvent onHeadDeath;
    [SerializeField] private float health = 100f;

    private bool isHalfHealthTriggered = false;

    // Animation parameter hashes (for better performance)
    private int isAppearingHash;
    private int isDisappearingHash;
    private int isDeadHash;
    private int isVulnerableHash;
    private int isLaseringHash;
    private int attackTypeHash;
    private int attack1LoopCounterHash;

    // State trackers
    private int currentAttack1Loops = 0;
    private bool isDead = false;
    private bool isActive = false;
    private GameObject currentLaser;

    private void Awake()
    {
        // Cache animator parameter hashes
        isAppearingHash = Animator.StringToHash("isAppearing");
        isDisappearingHash = Animator.StringToHash("isDisappearing");
        isDeadHash = Animator.StringToHash("isDead");
        isVulnerableHash = Animator.StringToHash("isVulnerable");
        isLaseringHash = Animator.StringToHash("isLasering");
        attackTypeHash = Animator.StringToHash("attackType");
        attack1LoopCounterHash = Animator.StringToHash("attack1LoopCounter");
        
        // Get references if not assigned
        if (animator == null) animator = GetComponent<Animator>();

        // If laser spawn point not set, default to projectile spawn point
        if (laserSpawnPoint == null) laserSpawnPoint = projectileSpawnPoint;

        // Initially deactivate
        gameObject.SetActive(false);
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            healthSlider.gameObject.SetActive(false); // Initially hide the health slider
        }
        flashEffect = GetComponent<Flash>();

    }

    public void Activate()
    {
        if (isDead) return;

        gameObject.SetActive(true);
        isActive = true;
        animator.SetBool(isAppearingHash, true);
        ShowHealthBar();
        // Start the attack sequence after the appear animation
        StartCoroutine(StartAttackAfterAppear());
    }

    private IEnumerator StartAttackAfterAppear()
    {
        // Wait for the appear animation to finish
        yield return new WaitForSeconds(GetAnimationLength("head_1_appear"));

        // Reset appearance flag
        animator.SetBool(isAppearingHash, false);

        // Start attack sequence
        PerformAttack1();
    }

    public void PerformAttack1()
    {
        // Initialize attack1 loop counter
        currentAttack1Loops = 0;
        animator.SetInteger(attack1LoopCounterHash, currentAttack1Loops);

        // Set attack type to 1 (projectile attack)
        animator.SetInteger(attackTypeHash, 1);

        // The animation transitions will handle the attack sequence
        StartCoroutine(LoopAttack1());
    }

    private IEnumerator LoopAttack1()
    {
        // Wait for pre-attack animation
        yield return new WaitForSeconds(GetAnimationLength("head_1_pre_attack1"));

        // Fire projectile when attack animation starts
        yield return new WaitForSeconds(0.2f); // Small delay to sync with animation
        FireProjectile();

        // Count loops
        currentAttack1Loops++;
        animator.SetInteger(attack1LoopCounterHash, currentAttack1Loops);

        // If we haven't done 2 loops yet, wait and loop again
        if (currentAttack1Loops < 2)
        {
            yield return new WaitForSeconds(GetAnimationLength("head_1_attack1") - 0.2f);
            StartCoroutine(LoopAttack1());
        }
        else
        {
            // After completing loops, enter vulnerable state
            yield return new WaitForSeconds(GetAnimationLength("head_1_end_attack1"));
            StartCoroutine(BecomeVulnerable());
        }
    }

    public void PerformAttack2()
    {
        // Set attack type to 2 (laser attack)
        animator.SetInteger(attackTypeHash, 2);

        // Start the laser attack sequence
        StartCoroutine(PerformLaserAttack());
    }

    private IEnumerator PerformLaserAttack()
    {
        // Wait for pre-attack animation
        yield return new WaitForSeconds(GetAnimationLength("head_1_pre_attack2"));

        // Activate laser
        animator.SetBool(isLaseringHash, true);
        FireLaser();

        // Keep laser active for duration
        yield return new WaitForSeconds(laserDuration);

        // Deactivate laser
        animator.SetBool(isLaseringHash, false);

        // Clean up laser if it still exists
        if (currentLaser != null)
        {
            Destroy(currentLaser);
            currentLaser = null;
        }

        // Wait for end animation
        yield return new WaitForSeconds(GetAnimationLength("head_1_end_attack2"));

        // Become vulnerable after laser attack
        StartCoroutine(BecomeVulnerable());
    }

    private IEnumerator BecomeVulnerable()
    {
        // Set vulnerable state
        animator.SetBool(isVulnerableHash, true);

        // Wait for vulnerable period
        yield return new WaitForSeconds(vulnerableTime);

        // End vulnerable state
        animator.SetBool(isVulnerableHash, false);

        // Disappear
        Disappear();
    }

    public void Disappear()
    {
        if (!isActive) return;

        animator.SetBool(isDisappearingHash, true);

        StartCoroutine(CompleteDisappear());
    }

    private IEnumerator CompleteDisappear()
    {
        // Wait for disappear animation
        yield return new WaitForSeconds(GetAnimationLength("head_1_disappear"));

        // Reset animation parameters
        animator.SetBool(isDisappearingHash, false);
        animator.SetBool(isVulnerableHash, false);
        animator.SetBool(isLaseringHash, false);
        animator.SetInteger(attackTypeHash, 0);

        // Deactivate game object
        isActive = false;
        gameObject.SetActive(false);
        HideHealthBar();
        // Notify boss controller that this head completed its sequence
        bossController.OnHeadCompleteSequence(1);
    }

    public void TakeDamage(float damage)
    {
        // Only take damage when vulnerable
        if (!animator.GetBool(isVulnerableHash)) return;

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        // Update health slider
        UpdateHealthUI();

        // Trigger flash effect
        if (flashEffect != null)
        {
            flashEffect.FlashSprite();
        }

        // Trigger damage event
        onDamaged?.Invoke();

        // Check for half health trigger
        if (!isHalfHealthTriggered && health <= maxHealth / 2)
        {
            isHalfHealthTriggered = true;
            onHealthBelowHalf?.Invoke();
        }

        Debug.Log($"Head took damage. Health: {health}/{maxHealth}");

        // Check if dead
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        // Set dead state
        isDead = true;
        animator.SetBool(isDeadHash, true);
        onHeadDeath?.Invoke(); // Trigger death event
        HideHealthBar();
        // Notify boss controller
        bossController.OnHeadDeath(1);

        // Start death sequence
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Wait for death animation
        yield return new WaitForSeconds(GetAnimationLength("head_1_die"));

        // Deactivate
        gameObject.SetActive(false);
    }

    private void FireProjectile()
    {
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

            // Find player and set direction
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 direction = (player.transform.position - projectileSpawnPoint.position).normalized;

                // Get rigidbody component if projectile has one
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * projectileSpeed;
                }
                else
                {
                    // Add a projectile script component if needed
                    Head_1_Projectile projectileScript = projectile.GetComponent<Head_1_Projectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.Initialize(direction, projectileSpeed);
                    }
                }
            }
        }
    }

    private void FireLaser()
    {
        if (laserPrefab != null && laserSpawnPoint != null)
        {
            // Destroy any existing laser
            if (currentLaser != null)
            {
                Destroy(currentLaser);
            }

            // Instantiate new laser at the laser spawn point
            currentLaser = Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.identity);

            // Set laser parent to keep it attached to the head
            currentLaser.transform.SetParent(laserSpawnPoint);

            // Configure the laser if it has a LaserController component
            BossLaser laserController = currentLaser.GetComponent<BossLaser>();
            if (laserController != null)
            {
                laserController.Initialize(laserDirection, laserDuration);
            }
        }
    }

    // Helper method to get animation length
    private float GetAnimationLength(string animName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animName)
            {
                return clip.length;
            }
        }
        return 1.0f; // Default fallback time
    }

    // Reset for reuse (when boss is defeated and player retries)
    public void Reset()
    {
        health = maxHealth;
        isHalfHealthTriggered = false;
        UpdateHealthUI();
        isDead = false;
        isActive = false;
        currentAttack1Loops = 0;

        // Clean up any active laser
        if (currentLaser != null)
        {
            Destroy(currentLaser);
            currentLaser = null;
        }

        // Reset animator parameters
        animator.SetBool(isAppearingHash, false);
        animator.SetBool(isDisappearingHash, false);
        animator.SetBool(isDeadHash, false);
        animator.SetBool(isVulnerableHash, false);
        animator.SetBool(isLaseringHash, false);
        animator.SetInteger(attackTypeHash, 0);
        animator.SetInteger(attack1LoopCounterHash, 0);

        gameObject.SetActive(false);
    }
    // Add this to both Head1Controller and Head2Controller
    public void ActivateForSpecificAttack()
    {
        if (isDead) return;

        gameObject.SetActive(true);
        isActive = true;
        animator.SetBool(isAppearingHash, true);
        ShowHealthBar();
        // Start a coroutine that just waits for appearance but doesn't start Attack1
        StartCoroutine(WaitForAppearAnimation());
    }

    private IEnumerator WaitForAppearAnimation()
    {
        // Wait for the appear animation to finish
        yield return new WaitForSeconds(GetAnimationLength("head_1_appear")); // or "head_2_appear" for Head2Controller

        // Reset appearance flag
        animator.SetBool(isAppearingHash, false);

        // No automatic attack is started
    }
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }
    public void ShowHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(true);
        }
    }

    // New method to hide health bar
    public void HideHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(false);
        }
    }
}