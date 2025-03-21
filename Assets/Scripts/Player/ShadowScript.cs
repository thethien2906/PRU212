using System.Collections.Generic;
using UnityEngine;
public class ShadowScript : MonoBehaviour
{
    public static ShadowScript me;
    public GameObject Sombra;
    public List<GameObject> pool = new List<GameObject>();  // This is initialized but empty
    private float cronometro;
    public float speed;
    public Color _color;

    void Awake()
    {
        me = this;
        // Initialize the pool with some objects to avoid the error
        InitializePool(5); // Pre-create 5 shadow objects
    }

    // New method to initialize the pool
    private void InitializePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(Sombra, transform.position, transform.rotation);
            obj.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
            obj.GetComponent<Solid>()._color = _color;
            obj.SetActive(false); // Start inactive
            pool.Add(obj);
        }
    }

    public GameObject GetShadows()
    {
        // Safety check to prevent null references
        if (pool == null)
        {
            pool = new List<GameObject>();
        }

        // Check if Sombra prefab is assigned
        if (Sombra == null)
        {
            Debug.LogError("Shadow prefab (Sombra) is not assigned in the inspector!");
            return null;
        }

        // Look for an inactive object in the pool
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && !pool[i].activeInHierarchy)
            {
                pool[i].SetActive(true);
                pool[i].transform.position = transform.position;
                pool[i].transform.rotation = transform.rotation;
                pool[i].GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
                pool[i].GetComponent<Solid>()._color = _color;
                return pool[i];
            }
        }

        // If no inactive object is found, create a new one
        GameObject obj = Instantiate(Sombra, transform.position, transform.rotation);
        obj.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
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
}