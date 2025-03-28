using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Phase Components")]
    [SerializeField] private Head1Controller head1;
    [SerializeField] private Head2Controller head2;
    [SerializeField] private GameObject bossBody;
    [SerializeField] private GameObject bossHand;

    [Header("Boss Settings")]
    [SerializeField] private int bossHealth = 500;
    [SerializeField] private float phase2TriggerDelay = 3f;
    [SerializeField] private Slider healthSlider;

    [Header("Phase 2 Attack Prefabs")]
    [SerializeField] private GameObject laserRainPrefabReference;
    [SerializeField] private GameObject bossHandPrefab;
    [SerializeField] private GameObject plasmaProjectilePrefab;

    [Header("Phase 2 Attack Positions")]
    [SerializeField] private Transform plasmaSpawnPoint;

    [Header("Phase 2 Timing")]
    [SerializeField] private float laserRainDuration = 5f;
    [SerializeField] private float bossHandDuration = 4f;
    [SerializeField] private float plasmaProjectileDuration = 3f;
    [SerializeField] private float attackCyclePause = 2f;

    private Animator bossBodyAnimator;
    // State tracking
    private bool isPhase1 = true;
    private bool head1Dead = false;
    private bool head2Dead = false;
    private int currentSequenceStep = 0;
    private void Awake()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = bossHealth;
            healthSlider.value = bossHealth;
            healthSlider.gameObject.SetActive(false);
        }
    }
    public void InitializeBoss()
    {
        StartCoroutine(InitializeBossSequence());
    }

    private IEnumerator InitializeBossSequence()
    {
        // Ensure boss body is active
        if (bossBody != null)
        {
            bossBody.SetActive(true);

            // Get the Animator component from the bossBody
            bossBodyAnimator = bossBody.GetComponent<Animator>();
        }

        // Trigger intro animation
        if (bossBodyAnimator != null)
        {
            AudioManager.instance.PlaySFX(65);
            bossBodyAnimator.SetTrigger("Intro");

            // Wait for intro animation to complete
            yield return new WaitForSeconds(GetAnimationLength("Nova_Body_WakeuUp") + 1);
        }

        // Set initial state after intro
        isPhase1 = true;
        head1Dead = false;
        head2Dead = false;
        currentSequenceStep = 0;


        // Start the Phase 1 sequence
        StartCoroutine(StartPhase1());
    }
    // Helper method to get animation length
    private float GetAnimationLength(string animName)
    {
        AnimationClip[] clips = bossBodyAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animName)
            {
                return clip.length;
            }
        }
        return 1.0f; // Default fallback time
    }

    private IEnumerator StartPhase1()
    {
        Debug.Log("[BossController] StartPhase1 called");
        // Short delay before boss appears
        yield return new WaitForSeconds(2f);

        // Start the first sequence
        AdvancePhase1Sequence();
    }

    private void AdvancePhase1Sequence()
    {
        // If both heads are dead, start phase 2
        if (head1Dead && head2Dead)
        {
            StartCoroutine(TransitionToPhase2());
            return;
        }

        // Skip dead heads in the sequence
        if ((currentSequenceStep == 0 && head1Dead) ||
            (currentSequenceStep == 1 && head2Dead))
        {
            currentSequenceStep++;
            if (currentSequenceStep > 2) currentSequenceStep = 0;
            AdvancePhase1Sequence();
            return;
        }

        // Execute current sequence step
        switch (currentSequenceStep)
        {
            case 0: // Head 1 attack
                AudioManager.instance.PlaySFX(63);
                if (head1 != null) head1.Activate();
                break;

            case 1: // Head 2 attack
                AudioManager.instance.PlaySFX(63);
                if (head2 != null) head2.Activate();
                break;

            case 2: // Both heads laser attack
                StartCoroutine(CombinedLaserAttack());
                break;
        }
    }

    private IEnumerator CombinedLaserAttack()
    {
        // First: Activate Head 1 and perform laser attack
        if (!head1Dead && head1 != null)
        {
            head1.ActivateForSpecificAttack(); // Use new method that doesn't auto-start Attack1

            // Wait for appear animation to finish
            yield return new WaitForSeconds(GetHeadAnimationLength(1, "head_1_appear"));

            // Now that the head has appeared, start the laser attack
            head1.PerformAttack2();

            // Wait for Head 1's full laser attack sequence to complete (estimated)
            // This includes the laser duration, vulnerability, and disappear animations
            yield return new WaitForSeconds(7f); // Adjust time based on your actual durations
        }

        // Then: Activate Head 2 and perform laser attack
        if (!head2Dead && head2 != null)
        {
            head2.ActivateForSpecificAttack(); // Use new method that doesn't auto-start Attack1

            // Wait for appear animation to finish
            yield return new WaitForSeconds(GetHeadAnimationLength(2, "head_2_appear"));

            // Now start the laser attack
            head2.PerformAttack2();

            // Wait for Head 2's full attack sequence to complete (estimated)
            yield return new WaitForSeconds(5f); // Adjust time based on your actual durations
        }

        // Reset sequence step for next round
        currentSequenceStep = 0;
        AdvancePhase1Sequence();
    }

    // Add this helper to BossController
    private float GetHeadAnimationLength(int headNumber, string animName)
    {
        // Default fallback values based on head
        if (headNumber == 1)
        {
            return 1.0f; // Default for Head1, adjust as needed
        }
        else
        {
            return 1.0f; // Default for Head2, adjust as needed
        }
    }

    public void OnHeadCompleteSequence(int headNumber)
    {
        // Advance sequence when a head completes its attack
        if ((headNumber == 1 && currentSequenceStep == 0) ||
            (headNumber == 2 && currentSequenceStep == 1))
        {
            currentSequenceStep++;
            AdvancePhase1Sequence();
        }
    }

    public void OnHeadDeath(int headNumber)
    {
        // Mark head as dead
        if (headNumber == 1)
        {
            head1Dead = true;
            Debug.Log("Head 1 is dead");
        }
        else if (headNumber == 2)
        {
            head2Dead = true;
            Debug.Log("Head 2 is dead");
        }

        // Immediately check if both heads are dead
        if (head1Dead && head2Dead)
        {
            StopAllCoroutines();

            AudioManager.instance.PlayBGM(7);
            StartCoroutine(TransitionToPhase2());
            return;
        }

        // If only one head is dead, make sure the sequence continues properly
        AdvancePhase1Sequence();
    }

    private IEnumerator TransitionToPhase2()
    {
        Debug.Log("Phase 2");
        ShowHealthBar();
        // Wait before starting phase 2
        yield return new WaitForSeconds(phase2TriggerDelay);

        // Change phase
        isPhase1 = false;

        // Trigger attack animation in the boss body
        if (bossBodyAnimator != null)
        {
            bossBodyAnimator.SetBool("isAttacking", true);
            // Wait for attack animation to transition
            yield return new WaitForSeconds(2f);
        }

        // Start phase 2 attacks
        StartCoroutine(Phase2AttackSequence());
    }

    private IEnumerator Phase2AttackSequence()
    {
        while (bossHealth > 0)
        {
            // First attack: Laser Rain
            yield return StartCoroutine(LaserRainAttack());

            // Pause between attacks
            yield return new WaitForSeconds(attackCyclePause);

            // Second attack: Boss Hand Attack
            yield return StartCoroutine(BossHandAttack());

            // Pause between attacks
            yield return new WaitForSeconds(attackCyclePause);

            // Third attack: Plasma Projectile Attack
            yield return StartCoroutine(PlasmaProjectileAttack());

            // Pause between attacks
            yield return new WaitForSeconds(attackCyclePause);

            // Fourth attack: Second Boss Hand Attack (as per your requirements)
            yield return StartCoroutine(BossHandAttack());

            // Pause between attacks
            yield return new WaitForSeconds(attackCyclePause);
        }

        // If we exit the loop, boss is defeated
        StartCoroutine(BossDefeated());
    }

    private IEnumerator LaserRainAttack()
    {
        AudioManager.instance.PlaySFX(69);

        Debug.Log("[BossController] Starting Laser Rain Attack");

        // Spawn the laser rain prefab
        if (laserRainPrefabReference != null)
        {
            // Instantiate the laser rain prefab at an appropriate position
            Vector3 spawnPosition = transform.position + new Vector3(4f, 1f, 0f); // Offset 5 units to the right
            GameObject laserRainInstance = Instantiate(
                laserRainPrefabReference,
                spawnPosition,
                Quaternion.identity
            );

            // Get the LaserRainPrefab component and activate it
            LaserRainPrefab laserRainController = laserRainInstance.GetComponent<LaserRainPrefab>();
            if (laserRainController != null)
            {
                laserRainController.ActivateLaserPillars();

                // Wait for the full duration of the laser rain attack
                yield return new WaitForSeconds(laserRainDuration);
            }
            else
            {
                Debug.LogError("[BossController] LaserRainPrefab component not found on instantiated object");
                // Destroy the object if component not found
                Destroy(laserRainInstance);
                yield return new WaitForSeconds(1f); // Brief wait to prevent instant loop
            }
        }
        else
        {
            Debug.LogError("[BossController] Laser Rain Prefab Reference is null");
            yield return new WaitForSeconds(1f); // Brief wait to prevent instant loop
        }
    }
    // Add this new method to your BossController class
    private IEnumerator BossHandAttack()
    {
        Debug.Log("[BossController] Starting Boss Hand Attack");

        // Find player reference first to ensure it exists
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("[BossController] Player not found in scene. Cannot perform hand attack!");
            yield return new WaitForSeconds(1f);
            yield break;
        }

        // Spawn the hand prefab
        if (bossHandPrefab != null)
        {
            // Instantiate the hand prefab
            GameObject handInstance = Instantiate(
                bossHandPrefab,
                transform.position,
                Quaternion.identity
            );

            // Get the BossHandController component
            BossHandController handController = handInstance.GetComponent<BossHandController>();
            if (handController != null)
            {
                // Set the reference to the boss controller
                handController.SetBossController(this);

                // Pass the player reference to the hand controller
                handController.SetPlayer(playerTransform);

                // Position and activate the hand
                handController.PositionAndActivate();

                // Wait for the hand attack to complete
                yield return new WaitForSeconds(bossHandDuration);
            }
            else
            {
                Debug.LogError("[BossController] BossHandController component not found on instantiated object");
                // Destroy the object if component not found
                Destroy(handInstance);
                yield return new WaitForSeconds(1f); // Brief wait to prevent instant loop
            }
        }
        else
        {
            Debug.LogError("[BossController] Boss Hand Prefab is null");
            yield return new WaitForSeconds(1f); // Brief wait to prevent instant loop
        }
    }
    // Add this method to your BossController class
    // Add this method to your BossController class
    private IEnumerator PlasmaProjectileAttack()
    {
        Debug.Log("[BossController] Starting Plasma Projectile Attack");

        // Check if plasma spawn point and prefab exist
        if (plasmaSpawnPoint == null || plasmaProjectilePrefab == null)
        {
            Debug.LogError("[BossController] Missing plasma spawn point or prefab");
            yield return new WaitForSeconds(1f);
            yield break;
        }

        // Find player position to determine direction
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        Vector2 directionToPlayer = Vector2.right; // Default direction

        if (playerTransform != null)
        {
            directionToPlayer = (playerTransform.position - plasmaSpawnPoint.position).normalized;
        }
        AudioManager.instance.PlaySFX(70);
        // Create initial projectile aimed at player
        GameObject projectile = Instantiate(
            plasmaProjectilePrefab,
            plasmaSpawnPoint.position,
            Quaternion.identity
        );

        PlasmaProjectile plasmaScript = projectile.GetComponent<PlasmaProjectile>();
        if (plasmaScript != null)
        {
            // Initialize with default values from the projectile itself
            plasmaScript.Initialize(directionToPlayer);
            AudioManager.instance.PlaySFX(71);
        }
        else
        {
            Debug.LogError("[BossController] PlasmaProjectile component not found on prefab");
            Destroy(projectile);
        }

        // Short delay before firing more projectiles
        yield return new WaitForSeconds(0.5f);

        // Fire additional projectiles in different directions
        Vector2[] additionalDirections = new Vector2[]
        {
        new Vector2(1f, 0.5f).normalized,   // Diagonal up-right
        new Vector2(-1f, 0.5f).normalized,  // Diagonal up-left
        new Vector2(1f, -0.5f).normalized,  // Diagonal down-right
        new Vector2(-1f, -0.5f).normalized  // Diagonal down-left
        };
        AudioManager.instance.PlaySFX(71);
        foreach (Vector2 direction in additionalDirections)
        {
            GameObject additionalProjectile = Instantiate(
                plasmaProjectilePrefab,
                plasmaSpawnPoint.position,
                Quaternion.identity
            );

            PlasmaProjectile additionalPlasmaScript = additionalProjectile.GetComponent<PlasmaProjectile>();
            if (additionalPlasmaScript != null)
            {
                additionalPlasmaScript.Initialize(direction);
            }
            else
            {
                Destroy(additionalProjectile);
            }

            yield return new WaitForSeconds(0.3f); // Short delay between shots
        }

        // Wait for the projectiles to travel and split
        yield return new WaitForSeconds(plasmaProjectileDuration);
    }
    public void TakeDamagePhase2(int damage)
    {
        // Only apply damage during phase 2
        if (!isPhase1)
        {
            bossHealth -= damage;
            Debug.Log("Health:" + bossHealth);
            // Visual feedback
            StartCoroutine(FlashBodyDamaged());
            UpdateHealthUI();
            // Check if boss is defeated
            if (bossHealth <= 0)
            {
                StopAllCoroutines();
                StartCoroutine(BossDefeated());
            }
        }
    }

    private IEnumerator FlashBodyDamaged()
    {
        // Visual feedback for taking damage
        SpriteRenderer bodyRenderer = bossBody?.GetComponent<SpriteRenderer>();
        if (bodyRenderer != null)
        {
            Color originalColor = bodyRenderer.color;
            bodyRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            bodyRenderer.color = originalColor;
        }
    }

    private IEnumerator BossDefeated()
    {
        // Play death animations and effects
        if (bossBodyAnimator != null)
        {
            // Stop attack animation
            bossBodyAnimator.SetBool("isAttacking", false);

            // Play death animation
            AudioManager.instance.PlaySFX(56);
            AudioManager.instance.LowerBGMVolumeSlowly();
            bossBodyAnimator.SetBool("isDead",true);
            HideHealthBar();
            // Wait for animation to play
            yield return new WaitForSeconds(5f);
        }

        // Disable boss components
        if (bossBody != null) bossBody.SetActive(false);
        if (bossHand != null) bossHand.SetActive(false);

        // Trigger victory condition
        yield return new WaitForSeconds(1f);
        Debug.Log("Boss Defeated!");

        // Notify game manager or level controller
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            GameManager.instance.LevelFinished();
        }
    }
    public void ResetBoss()
    {
        // Reset health
        bossHealth = 500;
        UpdateHealthUI();
        // Reset all components
        if (head1 != null) head1.Reset();
        if (head2 != null) head2.Reset();

        // Reinitialize
        InitializeBoss();
    }
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = bossHealth;
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