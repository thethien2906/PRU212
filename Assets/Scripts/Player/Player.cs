using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled;
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

    private bool isGrounded;
    private bool isAirborne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;
    private bool facingRight = true;
    private int facingDir = 1;

    private Health health;

    [Header("VFX")]
    [SerializeField] private GameObject deathVfx;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
    }
    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        respawnFinished(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Knockback();
        }
        if (canBeControlled == false)
        {
            return;
        }
        updateAirborneStatus();
        if (isKnockback)
        {
            return;
        }
        handleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimations();

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
}
