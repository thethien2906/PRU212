using System.Collections;
using UnityEngine;

public class BossShockwave : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float initialScale = 0.5f;
    [SerializeField] private float finalScale = 2.0f;
    [SerializeField] private float growDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color initialColor = Color.white;
    [SerializeField] private Color finalColor = new Color(1, 1, 1, 0);
    [SerializeField] private GameObject impactEffectPrefab;

    private Vector2 direction;
    private float speed;
    private bool isGrowing = true;
    private float growTimer = 0f;
    private Vector3 originalScale;

    private void Awake()
    {
        // Get reference if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Store original scale
        originalScale = transform.localScale;

        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector2 direction, float speed)
    {
        this.direction = direction;
        this.speed = speed;

        // Set initial scale
        transform.localScale = originalScale * initialScale;

        // Orient toward movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Start the grow effect
        StartCoroutine(GrowEffect());
    }

    private void Update()
    {
        // Move in the set direction
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    private IEnumerator GrowEffect()
    {
        // Initialize color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = initialColor;
        }

        while (growTimer < growDuration)
        {
            growTimer += Time.deltaTime;
            float t = growTimer / growDuration;

            // Apply scaling based on curve
            float scaleMultiplier = Mathf.Lerp(initialScale, finalScale, scaleCurve.Evaluate(t));
            transform.localScale = originalScale * scaleMultiplier;

            // Optional: fade out towards the end
            if (spriteRenderer != null && t > 0.7f)
            {
                float fadeT = (t - 0.7f) / 0.3f;
                spriteRenderer.color = Color.Lerp(initialColor, finalColor, fadeT);
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if collided with player
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.PlaySFX(62);
            // Deal damage to player
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Show impact effect
            SpawnImpactEffect(other.transform.position);
        }
    }

    private void SpawnImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.identity);

            // Automatically destroy impact effect after a short time
            Destroy(impact, 2f);
        }
    }
}