using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled;
    private int originalLayer;
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float defaultGravityScale;
    private bool canDoubleJump;

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
            HandleFlip();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // freeze horizontal movement during attack
        }

        HandleCollision();
        HandleAnimations();
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }
    private void WallJump()
    {
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
        if (canSlide == false)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifer);

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
        GameObject newDeathVfx = Instantiate(deathVfx, transform.position, Quaternion.identity);
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

        while (elapsed < specialDashDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, endPos, elapsed / specialDashDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(endPos); // Ensure perfect placement
    }
    public void EndSpecialAttack()
    {
        anim.SetTrigger("endingAttack");
        isSpecialAttacking = false;
        lastSpecialAttackTime = Time.time;
        gameObject.layer = originalLayer;

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
}
