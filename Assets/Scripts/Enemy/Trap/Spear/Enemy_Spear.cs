using System.Collections;
using UnityEngine;

public class SpearTrap : MonoBehaviour
{
    [SerializeField] private int damage;

    [Header("Firetrap Timers")]
    [SerializeField] private float activationDelay; //th?i gian tr??c khi b?y ???c kích ho?t
    [SerializeField] private float activeTime; //th?i gian b?y v?n ho?t ??ng
    private Animator anim;
    private SpriteRenderer spriteRend;

    private bool triggered; //when the trap gets triggered
    private bool active; //when the trap is active and can hurt the player

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            if (!triggered)
                StartCoroutine(ActivateTrap());

            if (active)
                collision.GetComponent<Health>().TakeDamage(damage);
        }
    }
    private IEnumerator ActivateTrap()
    {
        //turn the sprite red to notify the player and trigger the trap
        triggered = true;
        spriteRend.color = Color.red;

        //Wait for delay, activate trap, turn on animation, return color back to normal
        yield return new WaitForSeconds(activationDelay);
        spriteRend.color = Color.white; //turn the sprite back to its initial color
        active = true;
        anim.SetBool("activated", true);

        //Wait until X seconds, deactivate trap and reset all variables and animator
        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
        anim.SetBool("activated", false);
    }
}
