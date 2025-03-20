
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

    void Awake()
    {
        me = this;
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
                pool[i].GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
                pool[i].GetComponent<Solid>()._color = _color;
                return pool[i];
            }

        }
        GameObject obj = Instantiate(Sombra,transform.position,transform.rotation);
        obj.GetComponent<SpriteRenderer>().sprite= GetComponent<SpriteRenderer>().sprite;
        obj.GetComponent<Solid>()._color = _color;
        pool.Add(obj);
        return obj;
    }
    public void Sombras_skill() {
        cronometro += speed * Time.deltaTime;
        if (cronometro > 1) {
            GetShadows();
            cronometro = 0;
        }
    }
}
