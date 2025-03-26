using UnityEngine;

public class GunEffectController : MonoBehaviour
{
    public string gunSide; // Set this in the prefab to "Left" or "Right"

    private MantisBoss boss;
    private void Start()
    {
        // Automatically find the MantisBoss in parent hierarchy
        boss = GetComponentInParent<MantisBoss>();
    }

    // This will be called by the animation event
    public void OnFireBulletEvent()
    {
        if (boss != null)
            boss.TriggerBullet(gunSide);
    }

}
