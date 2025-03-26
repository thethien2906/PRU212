using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class FinalBossTrigger : MonoBehaviour
{
    public float targetY = 0.28f;
    [SerializeField] private BossController bossController;
    [SerializeField] private GameObject barrierTilemap; // Reference to the barrier tilemap object
    [SerializeField] private float barrierActivationDelay = 2f; // Time before barrier activates

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggered) return;

            // Adjust camera
            CinemachinePositionComposer composer = CameraManager.instance.virtualCam.GetComponent<CinemachinePositionComposer>();
            if (composer != null)
            {
                composer.Composition.ScreenPosition.y = targetY;
            }
            else
            {
                Debug.LogError("CinemachineTransposer not found on virtualCam.");
            }

            // Initialize boss
            if (bossController != null)
            {
                Debug.Log("Boss entered");
                triggered = true; // Prevent re-triggering
                bossController.InitializeBoss();
                Debug.Log("Boss initialized");

                // Start the barrier activation coroutine
                StartCoroutine(ActivateBarrierAfterDelay());
            }
        }
    }

    private IEnumerator ActivateBarrierAfterDelay()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(barrierActivationDelay);

        // Activate the barrier tilemap
        if (barrierTilemap != null)
        {
            barrierTilemap.SetActive(true);
            Debug.Log("Arena barrier activated");

        }
        else
        {
            Debug.LogError("Barrier tilemap reference is missing!");
        }
    }
}