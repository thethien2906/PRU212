using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Player : MonoBehaviour
{
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
    [Header("Special Attack Cooldown")]
    [SerializeField] private float specialAttackCooldown = 1f;
    private float lastSpecialAttackTime = -Mathf.Infinity;


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
    [SerializeField] private Transform itemDropParent; // Tham chi?u ??n m?t transform ?? ch?a c�c item

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
        respawnFinished(false);

        originalCollisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
    }
    private void Update()
    {
        if (!canBeControlled)
            return;

        updateAirborneStatus();

        if (isKnockback)
            return;

        HandleComboAttack();
        HandleSpecialAttack();

        if (!isAttacking && !isEndingAttack && !isSpecialAttacking) // Block other movement inputs while attacking
        {
            handleInput();
            HandleWallSlide();
            HandleMovement();
            HandleDash(); // Add this line here
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

    }

    private void HandleSpecialAttack()
    {
        if (Input.GetKeyDown(KeyCode.K) && !isSpecialAttacking && !isAttacking && !isEndingAttack && Time.time > lastSpecialAttackTime + specialAttackCooldown)
        {
            if (mana.HasEnoughMana(100))
            {
                mana.ConsumeMana(100); 
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
            jump();
        } 
        else if (isWallDetected && !isGrounded)
        {
            WallJump();
        }
        else if (canDoubleJump )
        {
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

    }
    public void Knockback()
    {
        if (isKnockback)
        {
            return;
        }
        StartCoroutine(KnockbackRoutine());
        anim.SetTrigger("knockback");
   
        rb.linearVelocity = new Vector2(knockbackForce.x * -facingDir, knockbackForce.y);
    }

    public void Die()
    {
        // Instantiate death VFX
        GameObject newDeathVfx = Instantiate(deathVfx, transform.position, Quaternion.identity);

        // Drop all collected items
        DropCollectedItems();

        // Destroy the player object
        Destroy(gameObject);
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
        health.TakeDamage(damage);
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
            bufferedAttacks = Mathf.Min(bufferedAttacks + 1, 3); // Limit to max 3 attacks
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
                anim.SetTrigger("attack2");
            }
            else if (comboStep == 3)
            {
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
        Physics2D.SetLayerCollisionMask(gameObject.layer, specialAttackCollisionMask);
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

            // Check for collision with boss
            hit = Physics2D.Raycast(rb.position, new Vector2(facingDir, 0),
                                   stepSize, LayerMask.GetMask("Boss"));

            if (hit.collider != null && hit.collider.CompareTag("Boss"))
            {
                // Stop at the boss's position
                rb.MovePosition(hit.point);
                // Trigger any boss hit effects here
                break;
            }

            // Move to next position if no collision
            rb.MovePosition(nextPos);
            dashDistanceTraveled = Vector2.Distance(startPos, nextPos);
            elapsed += Time.deltaTime;
            yield return null;
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

        // Reset layer and collision mask after the delay
        gameObject.layer = originalLayer;
        Physics2D.SetLayerCollisionMask(gameObject.layer, originalCollisionMask);
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
        // Check if player is currently dashing
        if (isDashing)
            return;

        // Handle double-tap detection for dash
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            // Check if this is a double-tap (within the time window)
            if (Time.time - lastRightTapTime < doubleTapTimeWindow)
            {
                // Check if dash cooldown has passed
                if (Time.time - lastDashTime > dashCooldown)
                {
                    // Perform dash to the right
                    StartCoroutine(DashRoutine(1));

                }
            }
            // Update the last tap time
            lastRightTapTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            // Check if this is a double-tap (within the time window)
            if (Time.time - lastLeftTapTime < doubleTapTimeWindow)
            {
                // Check if dash cooldown has passed
                if (Time.time - lastDashTime > dashCooldown)
                {
                    // Perform dash to the left
                    StartCoroutine(DashRoutine(-1));
                }
            }
            // Update the last tap time
            lastLeftTapTime = Time.time;
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
        float dropSpacing = 0.5f; // Kho?ng c�ch gi?a c�c v?t ph?m
        Vector2 dropCenter = transform.position; // V? tr� trung t�m ?? nh? c�c v?t ph?m

        for (int i = 0; i < collectedItems.Count; i++)
        {
            GameObject item = collectedItems[i];
            if (item != null)
            {
                // T�nh to�n v? tr� nh? ra cho t?ng v?t ph?m theo h�ng ngang
                Vector2 dropPosition = dropCenter + new Vector2((i - collectedItems.Count / 2) * dropSpacing, 0);

                // T?m th?i k�ch ho?t v?t ph?m
                item.SetActive(true);

                // ??t v? tr� c?a v?t ph?m
                item.transform.position = dropPosition;

                // L?y component Rigidbody2D c?a v?t ph?m
                Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();

                if (itemRb != null)
                {
                    // T�nh to�n h??ng nh? ra ng?u nhi�n
                    Vector2 dropDirection = new Vector2(
                        UnityEngine.Random.Range(-1f, 1f),
                        UnityEngine.Random.Range(0.5f, 1f)
                    ).normalized;

                    // �p d?ng l?c nh? ra
                    itemRb.isKinematic = false;
                    itemRb.AddForce(dropDirection * itemDropForce, ForceMode2D.Impulse);
                }

                // N?u v?t ph?m c� parent ???c ch? ??nh, di chuy?n n� ??n ?�
                if (itemDropParent != null)
                {
                    item.transform.SetParent(itemDropParent);
                }

                // In ra tag c?a v?t ph?m
                Debug.Log("Dropped item with tag: " + item.tag);
            }
        }

        // X�a danh s�ch c�c v?t ph?m ?� nh?t
        collectedItems.Clear();
    }


}
