using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    public void FinishRespawn()
    {
        player.respawnFinished(true);
    }
    public void comboEnding()
    {
        player.ComboAttackEnded();
    }
    public void Normal()
    {
        player.Normal();
    }
    public void EnableComboWindow()
    {
        player.EnableComboWindow();
    }
    public void EndingAnimationFinished()
    {
        player.EndingAnimationFinished();
    }
    public void EnableCancelWindow()
    {
        player.EnableCancelWindow();
    }
    public void DisableCancelWindow()
    {
        player.DisableCancelWindow();
    }
    public void ActivateVFX1()
    {
        player.ActivateAttackVFX(1);
    }

    public void ActivateVFX2()
    {
        player.ActivateAttackVFX(2);
    }

    public void ActivateVFX3()
    {
        player.ActivateAttackVFX(3);
    }
    public void ActivateSpecialVFX()
    {
        player.ActivateSpecialVFX();
    }
    public void PerformSpecial()
    {
        Debug.Log("Special");
        player.PerformSpecialDash();


    }
    public void EndingSpecial()
    {
        Debug.Log("Ending Special");
        player.EndSpecialAttack();
    }
}
