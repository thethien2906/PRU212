using System.Collections.Generic;
using UnityEngine;

public class Shadows : MonoBehaviour
{
    public static Shadows me;
    public GameObject Sombra;
    public List<GameObject> pool = new List<GameObject>();
    private float cronometro;
    public float speed;
    public Color _color;

    // Reference to the child GameObject that has the SpriteRenderer
    [SerializeField] private GameObject spriteChild;

    void Awake()
    {
        me = this;
        // If no sprite child is manually assigned, try to find it
        if (spriteChild == null)
        {
            // Try to find child with SpriteRenderer
            spriteChild = GetComponentInChildren<SpriteRenderer>().gameObject;
        }
    }

    public GameObject GetShadows()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                pool[i].SetActive(true);
                pool[i].transform.position = transform.position;
                pool[i].transform.rotation = transform.rotation;

                // Get the sprite from the child GameObject
                Sprite playerSpriter = spriteChild.GetComponent<SpriteRenderer>().sprite;
                // Apply to shadow
                pool[i].GetComponent<SpriteRenderer>().sprite = playerSpriter;
                pool[i].GetComponent<Solid>()._color = _color;
                return pool[i];
            }
        }

        GameObject obj = Instantiate(Sombra, transform.position, transform.rotation);

        // Get the sprite from the child GameObject
        Sprite playerSprite = spriteChild.GetComponent<SpriteRenderer>().sprite;
        // Apply to new shadow
        obj.GetComponent<SpriteRenderer>().sprite = playerSprite;
        obj.GetComponent<Solid>()._color = _color;
        pool.Add(obj);
        return obj;
    }

    public void Sombras_skill()
    {
        cronometro += speed * Time.deltaTime;
        if (cronometro > 1)
        {
            GetShadows();
            cronometro = 0;
        }
    }
    public void Sombras_skill_jump()
    {
        cronometro += speed * 0.3f* Time.deltaTime;
        if (cronometro > 1)
        {
            GetShadows();
            cronometro = 0;
        }
    }
}