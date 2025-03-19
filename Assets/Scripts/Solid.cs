using UnityEngine;

public class Solid : MonoBehaviour
{
    private SpriteRenderer myRenderer;
    private Shader myMaterial;
    public Color _color;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myMaterial = Shader.Find("GUI/Text Shader");
    }
    void ColorSprite() { 
        myRenderer.material.shader = myMaterial;
        myRenderer.color = _color;
    }
    public void Finish() { 
        gameObject.SetActive(false);
    }
    
    // Update is called once per frame
    void Update()
    {
        ColorSprite();
    }
}
