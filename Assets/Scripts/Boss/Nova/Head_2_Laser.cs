using System.Collections;
using UnityEngine;

public class Head_2_Laser : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private int damagePerSecond = 30;
    [SerializeField] private float maxLength = 100f;
    [SerializeField] private float growSpeed = 20f;
    [SerializeField] private float initialWidth = 0.1f;
    [SerializeField] private float finalWidth = 1.0f;
    [SerializeField] private float growTime = 0.3f;
    [SerializeField] private LayerMask collisionLayers;

    [Header("Visual References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject hitEffect;

    private Vector2 direction;
    private float activeTime;
    private bool isFullyExtended = false;
    private float currentLength = 0f;
    private float currentWidth;

    private void Awake()
    {
        // Get line renderer if not assigned
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // Default direction if not set
        direction = Vector2.right;

        // Configure line renderer
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            currentWidth = initialWidth;

            // Initial width
            lineRenderer.startWidth = initialWidth;
            lineRenderer.endWidth = initialWidth;
        }
    }

    public void Initialize(Vector2 direction, float duration)
    {
        this.direction = direction.normalized;
        this.activeTime = duration;

        // Start laser growth
        StartCoroutine(GrowLaser());
    }

    private IEnumerator GrowLaser()
    {
        float elapsed = 0f;

        // Grow width
        while (elapsed < growTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growTime;

            currentWidth = Mathf.Lerp(initialWidth, finalWidth, t);
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = currentWidth;
                lineRenderer.endWidth = currentWidth;
            }

            yield return null;
        }

        // Fully grow the laser
        isFullyExtended = true;

        // Start damage checking
        StartCoroutine(CheckForDamage());
    }

    private void Update()
    {
        // Update the laser visualization
        UpdateLaser();
    }

    private void UpdateLaser()
    {
        if (lineRenderer != null)
        {
            // Start position is at the transform
            Vector3 startPos = transform.position;

            // If not fully extended yet, grow the laser
            if (!isFullyExtended)
            {
                currentLength += growSpeed * Time.deltaTime;
                if (currentLength > maxLength)
                {
                    currentLength = maxLength;
                }
            }
            else
            {
                currentLength = maxLength;
            }

            // Raycast to find obstacles
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, currentLength, collisionLayers);

            // Determine end position
            Vector3 endPos;
            if (hit.collider != null)
            {
                endPos = hit.point;

                // Show hit effect at collision point
                ShowHitEffect(endPos);
            }
            else
            {
                endPos = startPos + new Vector3(direction.x, direction.y, 0) * currentLength;
            }

            // Set line renderer positions
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
    }

    private void ShowHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            // Position hit effect
            hitEffect.transform.position = position;

            // Ensure hit effect is active
            hitEffect.SetActive(true);
        }
    }

    private IEnumerator CheckForDamage()
    {
        while (gameObject.activeInHierarchy)
        {
            // Calculate laser end point
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + new Vector3(direction.x, direction.y, 0) * currentLength;

            // Check for player in the laser path
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, currentLength, collisionLayers);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                // Get player health and apply damage
                Health playerHealth = hit.collider.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damagePerSecond);
                }
            }

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        // Visual debugging for laser direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * maxLength);
    }
}