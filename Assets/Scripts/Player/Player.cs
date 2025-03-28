using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class Player : MonoBehaviour
{
    private DifficultyType gameDifficulty;
    private GameManager gameManager;

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled;
    private int originalLayer;
    // Dust Effect
    public ParticleSystem dust;
    private bool isSliding = false;
    private float dustInterval = 0.1f; // Time between dust effects
    private float lastDustTime = 0f;
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float defaultGravityScale;
    private bool canDoubleJump;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float doubleTapTimeWindow = 0.25f;
    private float lastDashTime = -Mathf.Infinity;
    private float lastLeftTapTime = -Mathf.Infinity;
    private float lastRightTapTime = -Mathf.Infinity;
    private bool isDashing = false;
    private float dashDuration = 0.2f;

    [Header("Buffer && Coyote Jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpPressed = -1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpPressed = -1;

    [Header("Wall")]
    [SerializeField] private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Vector2 knockbackForce;
    private bool isKnockback;


    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask groundLayer;


    [Header("Combo Attack")]
    [SerializeField] private float comboResetTime = 3f;
    [SerializeField] private float comboInputBufferTime = 0.5f; // Time window to buffer combo inputs
    private int comboStep = 0;
    private float lastComboTime;
    private bool isAttacking;
    private bool canQueueNextAttack = false;
    private int bufferedAttacks = 0; // Track how many attacks are buffered
    private float lastBufferTime;    // Track when the last buffer input was received
    private bool isEndingAttack = false;

    [Header("Special Attack")]
    [SerializeField] private float specialDashDistance = 5f;
    [SerializeField] private float specialDashDuration = 0.1f; // Very fast, but still smooth
    [SerializeField] private GameObject specialAttackVFX;
    private bool isSpecialAttacking;
    private bool isInSpecialAttackLayerState = false;

    [Header("Special Attack Cooldown")]
    [SerializeField] private float specialAttackCooldown = 1f;
    private float lastSpecialAttackTime = -Mathf.Infinity;

    [Header("Animation Cancel")]
    [SerializeField] private bool enableAnimationCancel = true;
    [SerializeField] private float cancelCooldown = 0.1f;
    private float lastCancelTime = -Mathf.Infinity;
    private bool isInCancelableState = false;


    private bool isGrounded;
    private bool isAirborne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;
    private bool facingRight = true;
    private int facingDir = 1;

    private Health health;
    private Mana mana;

    [Header("VFX")]
    [SerializeField] private GameObject deathVfx;
    [SerializeField] private GameObject attack1VFX;
    [SerializeField] private GameObject attack2VFX;
    [SerializeField] private GameObject attack3VFX;
    [SerializeField] private float vfxDuration = 0.5f;
    [SerializeField] private float vfxSpecialDuration = 2f;


    [Header("Special Attack Collision")]
    [SerializeField] private LayerMask originalCollisionMask; // Store the original collision mask
    [SerializeField]
    private LayerMask specialAttackCollisionMask;

    //ch?t nh? item
    [Header("Inventory")]
    private List<GameObject> collectedItems = new List<GameObject>();
    [SerializeField] private float itemDropForce = 5f;
    [SerializeField] private Transform itemDropParent; // Tham chi?u ??n m?t transform ?? ch?a các item

    public void CollectItem(GameObject item)
    {
        if (item != null)
        {
            collectedItems.Add(item);
        }
    }

    //

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
        mana = GetComponent<Mana>();
    }
    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        gameManager = GameManager.instance;
        UpdateGameDifficulty();
        respawnFinished(false);

        originalCollisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
    }
    private void Update()
    {
        if (!canBeControlled)
            return;

        updateAirborneStatus();
        if (!isSpecialAttacking && isInSpecialAttackLayerState && Time.time > lastSpecialAttackTime + 2.5f)
        {
            Debug.LogWarning("Detected stuck special attack layer state! Force resetting layer.");
            gameObject.layer = originalLayer;
            isInSpecialAttackLayerState = false;
        }
        if (isKnockback)
            return;

        // Always check for cancel inputs even during attacks
        if (isAttacking || isEndingAttack || isSpecialAttacking)
        {
            CheckForCancelInputs();
        }

        HandleComboAttack();
        HandleSpecialAttack();
        if (!isAttacking && !isEndingAttack && !isSpecialAttacking) // Block other movement inputs while attacking
        {
            handleInput();
            HandleDash();
            HandleWallSlide();
            HandleMovement();
            HandleFlip();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // freeze horizontal movement during attack
        }

        HandleCollision();
        HandleAnimations();
        if (isDashing)
        {
            Shadows.me.Sombras_skill();
        }
        if (!isGrounded && !isSliding)
        {
            Shadows.me.Sombras_skill_jump();
        }
    }

    private void HandleSpecialAttack()
    {
        if (Input.GetKeyDown(KeyCode.K) && !isSpecialAttacking && !isAttacking && !isEndingAttack && Time.time > lastSpecialAttackTime + specialAttackCooldown)
        {
            if (mana.HasEnoughMana(100))
            {
                mana.ConsumeMana(100);
                AudioManager.instance.PlaySFX(28);
                TriggerSpecialAttack();
            }
            else
            {
                Debug.Log("Not enough mana!");
            }
        }
    }


    private void updateAirborneStatus()
    {
        if (isAirborne && isGrounded)
        {
            HandleLanding();
        }
        if (!isGrounded && !isAirborne)
        {
            BecomeAirborne();
        }
    }

    private void BecomeAirborne()
    {
        isAirborne = true;
        if (rb.linearVelocity.y < 0)
        {
            ActivateCoyoteJump();
        }
    }

    private void HandleLanding()
    {
        AudioManager.instance.PlaySFX(11);
        isAirborne = false;
        canDoubleJump = true;
        CreateDust(); // Dust when landing
        AttemptBufferJump();
    }


    private void handleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");


        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpButton();
            RequestBufferJump();
        }
        CheckForCancelInputs();
    }
    private void CheckForCancelInputs()
    {
        // Only check for animation cancel if enabled and in a cancelable state
        if (!enableAnimationCancel || !isInCancelableState || Time.time - lastCancelTime < cancelCooldown)
            return;

        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

        // Right dash input
        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)))
        {
            if (isShiftHeld || Time.time - lastRightTapTime < doubleTapTimeWindow)
            {
                CancelAnimation(1);
                return;
            }
            lastRightTapTime = Time.time;
        }

        // Left dash input
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            if (isShiftHeld || Time.time - lastLeftTapTime < doubleTapTimeWindow)
            {
                CancelAnimation(-1);
                return;
            }
            lastLeftTapTime = Time.time;
        }
    }

    private void CancelAnimation(int direction)
    {
        if (Time.time - lastDashTime <= dashCooldown)
            return;

        // Cancel current animations
        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("attack3");
        anim.ResetTrigger("endingAttack");
        anim.ResetTrigger("special");

        // Reset attack states
        isAttacking = false;
        isEndingAttack = false;
        isSpecialAttacking = false;

        // Mark that we performed a cancel
        lastCancelTime = Time.time;
        isInCancelableState = false;

        // Start the dash
        StartCoroutine(DashRoutine(direction));

        // Optional: Play a cancel VFX
        CreateDust();
    }
    private void RequestBufferJump()
    {
        if (isAirborne)
        {
            bufferJumpPressed = Time.time;
        }
    }
    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpPressed + bufferJumpWindow)
        {
            bufferJumpPressed = Time.time - 1;
            jumpButton();
        }
    }
    private void ActivateCoyoteJump()
    {
        coyoteJumpPressed = Time.time;
    }
    private void CancelCoyoteJump()
    {
        coyoteJumpPressed = Time.time - 1;
    }
    private void jumpButton()
    {
        bool coyoteJumpAvailable = Time.time < coyoteJumpPressed + coyoteJumpWindow;
        if (isGrounded || coyoteJumpAvailable)
        {
            AudioManager.instance.PlaySFX(10);
            jump();
        } 
        else if (isWallDetected && !isGrounded)
        {
            AudioManager.instance.PlaySFX(10);
            WallJump();
        }
        else if (canDoubleJump )
        {
            AudioManager.instance.PlaySFX(10);
            DoubleJump();
        }
        CancelCoyoteJump();
    }
    private void jump()
    {
        if (isGrounded) // Dust only when jumping off the ground
        {
            CreateDust();
        }
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void DoubleJump()
    {
        // No dust here or create a different effect if you want
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }
    private void WallJump()
    {
        // add dust
        CreateDust();
        canDoubleJump = true;
        rb.linearVelocity = new Vector2(-facingDir * wallJumpForce.x, wallJumpForce.y);
        Flip();
        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }
    private IEnumerator WallJumpRoutine()
    {

        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }


    private void HandleWallSlide()
    {
        bool canSlide = isWallDetected && !isGrounded && rb.linearVelocity.y < 0;
        float yModifer = yInput < 0 ? 1 : .5f;

        if (canSlide)
        {
            // Check if we just started sliding
            if (!isSliding)
            {
                isSliding = true;
                CreateDust(); // Initial dust effect
            }

            // Create dust at intervals while sliding
            if (Time.time > lastDustTime + dustInterval)
            {
                CreateDust();
                lastDustTime = Time.time;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifer);
        }
        else
        {
            isSliding = false;
        }
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockback = true;
        yield return new WaitForSeconds(knockbackDuration);
        isKnockback = false;

        // Double-check that attack states are reset after knockback ends
        if (isAttacking || isEndingAttack || isSpecialAttacking)
        {
            isAttacking = false;
            isEndingAttack = false;
            isSpecialAttacking = false;
            anim.SetTrigger("Normal");
        }
    }
    public void Knockback()
    {
        if (isKnockback)
        {
            return;
        }

        // Reset attack states when knocked back
        isAttacking = false;
        isEndingAttack = false;
        isSpecialAttacking = false;
        bufferedAttacks = 0;
        comboStep = 0;

        // Reset layer if we're in special attack state
        if (isInSpecialAttackLayerState)
        {
            gameObject.layer = originalLayer;
            isInSpecialAttackLayerState = false;
            Debug.Log("Forced layer reset due to knockback: " + gameObject.layer);
        }

        // Reset animation triggers
        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("attack3");
        anim.ResetTrigger("endingAttack");
        anim.ResetTrigger("special");

        AudioManager.instance.PlaySFX(UnityEngine.Random.Range(7, 9));
        CameraManager.instance.ScreenShake(facingDir);
        StartCoroutine(KnockbackRoutine());
        anim.SetTrigger("knockback");

        rb.linearVelocity = new Vector2(knockbackForce.x * -facingDir, knockbackForce.y);
    }

    public void Die()
    {
        
        GameObject newDeathVfx = Instantiate(deathVfx, transform.position, Quaternion.identity);

        // Get the current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Check if we're in Level_4
        if (currentSceneName == "Level_4")
        {
            AudioManager.instance.PlaySFX(UnityEngine.Random.Range(5,7));

            // Drop all collected items
            DropCollectedItems();

            // Destroy the player object
            Destroy(gameObject);

            // Reset the entire scene instead of respawning
            GameManager.instance.RestartLevel();
        }
        else
        {
            // Original behavior - destroy and respawn
            AudioManager.instance.PlaySFX(UnityEngine.Random.Range(39, 42));

            // Drop all collected items
            DropCollectedItems();

            Destroy(gameObject);
            GameManager.instance.RespawnPlayer();
        }
    }
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isWallDetected = Physics2D.Raycast(transform.position,Vector2.right * facingDir, wallCheckDistance, groundLayer);
    }
    public void respawnFinished(bool finished)
    {
        if (finished)
        {
            rb.gravityScale = defaultGravityScale;
            canBeControlled = true;
            cd.enabled = true;
        }
        else
        {
            rb.gravityScale = 0;
            canBeControlled = false;
            cd.enabled = false;
        }
    }
    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleMovement()
    {
        if (isWallDetected)
        {
            return;
        }
        if (isWallJumping)
        {
            return;
        }
        if (isDashing)
        {
            return; // Don't change velocity while dashing
        }
        rb.linearVelocity = new Vector2(xInput * speed, rb.linearVelocity.y);
    }

    private void HandleFlip()
    {
        if(xInput < 0 && facingRight || xInput > 0 && !facingRight)
        {
            Flip();
            
        }
    }
    private void Flip()
    {
        facingDir *= -1;
        transform.Rotate(0f, 180f, 0f);
        facingRight = !facingRight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }
    public void TakeDamage(int damage)
    {
        // Apply difficulty multiplier to damage
        float modifiedDamage = damage;

        switch (gameDifficulty)
        {
            case DifficultyType.Easy:
                modifiedDamage = damage * 0.1f; // Reduce damage by half
                break;
            case DifficultyType.Normal:
                modifiedDamage = damage * 0.5f;
                break;
            case DifficultyType.Hard:
                break;
        }

        // Convert back to integer (round down)
        int finalDamage = Mathf.FloorToInt(modifiedDamage);

        // Ensure minimum damage of 1 (optional)
        finalDamage = Mathf.Max(1, finalDamage);

        // Pass the modified damage to the health component
        health.TakeDamage(finalDamage);
    }

    private void HandleComboAttack()
{
    // Check if combo has timed out
    if (Time.time > lastComboTime + comboResetTime && comboStep > 0)
    {
        ResetCombo();
    }

    // Clean up expired buffers
    if (bufferedAttacks > 0 && Time.time > lastBufferTime + comboInputBufferTime)
    {
        bufferedAttacks = 0;
    }

    if (Input.GetKeyDown(KeyCode.J))
    {
        lastBufferTime = Time.time;
        
        if (!isAttacking)
        {
            // Start the first attack
            StartCombo();
        }
        else
        {
            // Buffer additional attacks (up to 3 total)
            bufferedAttacks = Mathf.Min(bufferedAttacks + 1, 3 - comboStep + 1);
        }
    }
}

    private void StartCombo()
    {
        AudioManager.instance.PlaySFX(0);
        comboStep = 1;
        lastComboTime = Time.time;
        anim.SetTrigger("attack1");
        isAttacking = true;
        
    }

    private void ResetCombo()
    {
        comboStep = 0;
        isAttacking = false;
        canQueueNextAttack = false;
    }

    public void EnableComboWindow()
    {
        canQueueNextAttack = true;
    }

    public void ComboAttackEnded()
    {
        canQueueNextAttack = false;

        // Check if we have buffered attacks
        if (bufferedAttacks > 0)
        {
            bufferedAttacks--;
            comboStep++;
            lastComboTime = Time.time;

            if (comboStep == 2)
            {

                AudioManager.instance.PlaySFX(1);
                anim.SetTrigger("attack2");
            }
            else if (comboStep == 3)
            {

                AudioManager.instance.PlaySFX(2);
                anim.SetTrigger("attack3");
            }
            else
            {
                // If we've completed the full combo
                anim.SetTrigger("endingAttack");
                isAttacking = false;
                isEndingAttack = true;
                comboStep = 0;
            }
        }
        else
        {
            // No more buffered attacks, end the combo
            anim.SetTrigger("endingAttack");
            isAttacking = false;
            isEndingAttack = true;
            comboStep = 0;
        }
    }

    public void Normal()
    {
        anim.SetTrigger("Normal");
    }
    public void EndingAnimationFinished()
    {
        isEndingAttack = false;
    }

    public void ActivateAttackVFX(int attackNumber)
    {
        GameObject vfx = null;

        switch (attackNumber)
        {
            case 1:
                vfx = attack1VFX;
                break;
            case 2:
                vfx = attack2VFX;
                break;
            case 3:
                vfx = attack3VFX;
                break;
        }

        if (vfx != null)
        {
            vfx.SetActive(true);
            Collider2D hitbox = vfx.GetComponent<Collider2D>();
            if (hitbox != null) hitbox.enabled = true;

            StartCoroutine(DeactivateVFX(vfx, hitbox));
        }
    }

    private IEnumerator DeactivateVFX(GameObject vfx, Collider2D hitbox)
    {
        yield return new WaitForSeconds(vfxDuration);
        vfx.SetActive(false);
        if (hitbox != null) hitbox.enabled = false;

    }



    private void TriggerSpecialAttack()
    {
        isSpecialAttacking = true;
        anim.SetTrigger("special");
        rb.linearVelocity = Vector2.zero; // Freeze current motion
        originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("SpecialAttack");
        isInSpecialAttackLayerState = true;
    }
    public void PerformSpecialDash()
    {
        StartCoroutine(SpecialDashRoutine());
    }

    private IEnumerator SpecialDashRoutine()
    {
        float elapsed = 0f;
        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + new Vector2(facingDir * specialDashDistance, 0);
        RaycastHit2D hit;
        float dashDistanceTraveled = 0f;
        float stepSize = 0.5f; // Check for collision every 0.5 units

        while (elapsed < specialDashDuration && dashDistanceTraveled < specialDashDistance)
        {
            // Calculate the next position
            float currentProgress = elapsed / specialDashDuration;
            Vector2 nextPos = Vector2.Lerp(startPos, endPos, currentProgress);

            // Check for collision with boss only
            hit = Physics2D.Raycast(rb.position, new Vector2(facingDir, 0),
                                   stepSize, LayerMask.GetMask("Boss"));

            if (hit.collider != null && hit.collider.CompareTag("Boss"))
            {
                // Stop at the boss's position
                rb.MovePosition(hit.point);
                // Trigger any boss hit effects here
                break;
            }

            while (elapsed < specialDashDuration)
            {
                rb.MovePosition(Vector2.Lerp(startPos, endPos, elapsed / specialDashDuration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            rb.MovePosition(endPos);
        }

        // Only move to end position if we didn't hit anything
        if (dashDistanceTraveled >= specialDashDistance)
        {
            rb.MovePosition(endPos);
        }
    }
    public void EndSpecialAttack()
    {
        anim.SetTrigger("endingAttack");
        isSpecialAttacking = false;
        lastSpecialAttackTime = Time.time;

        // Start coroutine to delay both layer and collision mask reset
        StartCoroutine(DelayedSpecialAttackReset());
    }

    private IEnumerator DelayedSpecialAttackReset()
    {
        // Wait for 2 seconds before resetting anything
        yield return new WaitForSeconds(2f);
        gameObject.layer = originalLayer;
        isInSpecialAttackLayerState = false; // Clear flag
        Debug.Log("Resetting layer and collision mask:" + gameObject.layer);
    }

    public void ActivateSpecialVFX()
    {
        if (specialAttackVFX != null)
        {
            specialAttackVFX.SetActive(true);
            StartCoroutine(DeactivateSpecialVFX(specialAttackVFX));
        }
    }
    private IEnumerator DeactivateSpecialVFX(GameObject vfx)
    {
        yield return new WaitForSeconds(vfxSpecialDuration);
        vfx.SetActive(false);
    }

    // Dash double click left or right 
    private void HandleDash()
    {
        if (isDashing)
            return;

        // Don't allow normal dashing during non-cancelable attack phases
        if (isAttacking && !isInCancelableState)
            return;

        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

        // --- Right Dash ---
        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)))
        {
            if (isShiftHeld)
            {
                TryDash(1);
            }
            else
            {
                if (Time.time - lastRightTapTime < doubleTapTimeWindow)
                {
                    TryDash(1);
                }
                lastRightTapTime = Time.time;
            }
        }

        // --- Left Dash ---
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            if (isShiftHeld)
            {
                TryDash(-1);
            }
            else
            {
                if (Time.time - lastLeftTapTime < doubleTapTimeWindow)
                {
                    TryDash(-1);
                }
                lastLeftTapTime = Time.time;
            }
        }
    }

    private void TryDash(int direction)
    {
        AudioManager.instance.PlaySFX(9);
        if (Time.time - lastDashTime > dashCooldown)
        {
            StartCoroutine(DashRoutine(direction));
        }
    }


    private IEnumerator DashRoutine(int direction)
    {
        // Store the original gravity
        float originalGravity = rb.gravityScale;

        // Set dash state and update last dash time
        isDashing = true;
        lastDashTime = Time.time;

        // Reduce gravity during dash
        rb.gravityScale = originalGravity * 0.5f;

        // Set dash velocity
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

        // Trigger dash animation
        anim.SetTrigger("dash");

        // Wait for dash duration
        yield return new WaitForSeconds(dashDuration);

        // Restore original gravity
        rb.gravityScale = originalGravity;

        // End dash state
        isDashing = false;
    }
    // create dust
    void CreateDust() { 
        dust.Play();
    }
    private void UpdateGameDifficulty()
    {
        DifficultyManager difficultyManager = DifficultyManager.instance;

        if (difficultyManager != null)
            gameDifficulty = difficultyManager.difficulty;
    }

   
    public void EnableCancelWindow()
    {
        isInCancelableState = true;
    }


    public void DisableCancelWindow()
    {
        isInCancelableState = false;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for health items
        if (collision.CompareTag("addBlood"))
        {
            CollectItem(collision.gameObject);
            collision.gameObject.SetActive(false);
            Debug.Log("Collected item with tag: " + collision.tag);
        }

        // Check for mana items
        if (collision.CompareTag("AddMana"))
        {
            CollectItem(collision.gameObject);
            collision.gameObject.SetActive(false);
            Debug.Log("Collected item with tag: " + collision.tag);
        }
    }

    private void DropCollectedItems()
    {
        float dropSpacing = 0.5f; // Kho?ng cách gi?a các v?t ph?m
        Vector2 dropCenter = transform.position; // V? trí trung tâm ?? nh? các v?t ph?m

        for (int i = 0; i < collectedItems.Count; i++)
        {
            GameObject item = collectedItems[i];
            if (item != null)
            {
                // Tính toán v? trí nh? ra cho t?ng v?t ph?m theo hàng ngang
                Vector2 dropPosition = dropCenter + new Vector2((i - collectedItems.Count / 2) * dropSpacing, 0);

                // T?m th?i kích ho?t v?t ph?m
                item.SetActive(true);

                // ??t v? trí c?a v?t ph?m
                item.transform.position = dropPosition;

                // L?y component Rigidbody2D c?a v?t ph?m
                Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();

                if (itemRb != null)
                {
                    // Tính toán h??ng nh? ra ng?u nhiên
                    Vector2 dropDirection = new Vector2(
                        UnityEngine.Random.Range(-1f, 1f),
                        UnityEngine.Random.Range(0.5f, 1f)
                    ).normalized;

                    // Áp d?ng l?c nh? ra
                    itemRb.isKinematic = false;
                    itemRb.AddForce(dropDirection * itemDropForce, ForceMode2D.Impulse);
                }

                // N?u v?t ph?m có parent ???c ch? ??nh, di chuy?n nó ??n ?ó
                if (itemDropParent != null)
                {
                    item.transform.SetParent(itemDropParent);
                }

                // In ra tag c?a v?t ph?m
                Debug.Log("Dropped item with tag: " + item.tag);
            }
        }

        // Xóa danh sách các v?t ph?m ?ã nh?t
        collectedItems.Clear();
    }


}
