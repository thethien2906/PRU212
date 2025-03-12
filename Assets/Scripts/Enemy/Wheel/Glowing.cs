using UnityEngine;

public class Glowing : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        gameObject.SetActive(false); 
    }

}
