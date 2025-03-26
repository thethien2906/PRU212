using System.Collections;
using UnityEngine;

public class LaserRainPrefab : MonoBehaviour
{
    [Header("Laser Pillar Components")]
    [SerializeField] private GameObject[] laserPillars;

    [Header("Timing Settings")]
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float activeDuration = 2.0f;
    [SerializeField] private float totalDuration = 5.0f;

    // Main activation method called by BossController
    public void ActivateLaserPillars()
    {
        StartCoroutine(LaserRainSequence());
    }

    private IEnumerator LaserRainSequence()
    {
        // Activate all laser pillars in warning state
        foreach (GameObject pillar in laserPillars)
        {
            pillar.SetActive(true);
            LaserPillar pillarScript = pillar.GetComponent<LaserPillar>();
            if (pillarScript != null)
            {
                pillarScript.SetWarningState();
            }
        }

        // Wait for warning duration
        yield return new WaitForSeconds(warningDuration);

        // Transition to damaging state
        foreach (GameObject pillar in laserPillars)
        {
            LaserPillar pillarScript = pillar.GetComponent<LaserPillar>();
            if (pillarScript != null)
            {
                pillarScript.SetDamagingState();
            }
        }

        // Keep lasers active for the specified duration
        yield return new WaitForSeconds(activeDuration);

        // Transition back to warning state (fade out)
        foreach (GameObject pillar in laserPillars)
        {
            LaserPillar pillarScript = pillar.GetComponent<LaserPillar>();
            if (pillarScript != null)
            {
                pillarScript.SetWarningState();
            }
        }

        // Wait for a short fade-out period
        yield return new WaitForSeconds(totalDuration - warningDuration - activeDuration);

        // Deactivate all pillars and destroy the prefab
        foreach (GameObject pillar in laserPillars)
        {
            pillar.SetActive(false);
        }

        Destroy(gameObject);
    }
}