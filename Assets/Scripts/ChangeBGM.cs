using UnityEngine;

public class ChangeBGM : MonoBehaviour
{
    [SerializeField] private int bgmID;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            AudioManager.instance.PlayBGM(bgmID);
        }
    }
}
