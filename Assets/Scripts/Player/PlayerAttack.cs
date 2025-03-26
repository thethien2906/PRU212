using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private GameObject GameObject;

    public void HideSprite()
    {
       if (GameObject != null) GameObject.SetActive(false);
    }
}
