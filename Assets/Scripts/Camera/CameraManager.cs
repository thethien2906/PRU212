using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public CinemachineCamera virtualCam;

    [Header("Screen Shake")]
    [SerializeField] private Vector2 shakeVelocity;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void ScreenShake(float shakeDirection)
    {
        impulseSource.DefaultVelocity = new Vector2(shakeVelocity.x * shakeDirection, shakeVelocity.y);
        impulseSource.GenerateImpulse();
    }
    public void OnPlayerRespawn(GameObject newPlayer)
    {
        if (virtualCam == null)
        {
            Debug.LogError("virtualCam is null!");
            return;
        }

        virtualCam.Follow = newPlayer.transform;
        virtualCam.LookAt = newPlayer.transform;
    }

}

