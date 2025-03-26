using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private bool active;
    [SerializeField] private bool canBeReactivated;

    [SerializeField] private Transform respawnPosition; // Position to respawn at (optional)

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // If no specific respawn position is set, use this object's position
        if (respawnPosition == null)
            respawnPosition = transform;
    }

    private void Start()
    {
        canBeReactivated = GameManager.instance.canReactivate;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (active && canBeReactivated == false)
            return;

        Player player = collision.GetComponent<Player>();
        if (player != null)
            ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        active = true;

        if (anim != null)
            anim.SetTrigger("activate");

        GameManager.instance.UpdateRespawnPosition(respawnPosition);
        Debug.Log("Checkpoint activated at " + respawnPosition.position);
    }
}