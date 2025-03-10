using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        gameObject.SetActive(false); // Make sure flame is off at start
    }
}
