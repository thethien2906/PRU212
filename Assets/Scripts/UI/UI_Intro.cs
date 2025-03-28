using UnityEngine;
using UnityEngine.SceneManagement;
public class UI_Intro : MonoBehaviour
{
    private UI_FadeEffect fadeEffect;
    [SerializeField] private RectTransform rectT;
    [SerializeField] private float scrollSpeed = 200;
    [SerializeField] private float offScreenPosition = 1800;
    private bool introSkipped;

    private void Awake()
    {
        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
        fadeEffect.ScreenFade(0, 2);
    }

    private void Start()
    {
        // Ensure the text starts below the screen
        rectT.anchoredPosition = new Vector2(rectT.anchoredPosition.x, -offScreenPosition);
    }

    private void Update()
    {
        // Scroll up
        rectT.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        Debug.Log("Current Position: " + rectT.anchoredPosition.y);

        // When text has scrolled past the top of the screen
        if (rectT.anchoredPosition.y > offScreenPosition)
            StartGame();
    }

    public void SkipIntro()
    {
        if (introSkipped == false)
        {
            scrollSpeed *= 10;
            introSkipped = true;
        }
        else
        {
            StartGame();
        }
    }

    private void StartGame() => fadeEffect.ScreenFade(1, 1, LoadFirstLevel);

    private void LoadFirstLevel()
    {
        // Get the index of the first level
        int sceneToLoad = 1; // Default to Level 1

        // Store which scene to load next
        PlayerPrefs.SetInt("SceneToLoad", sceneToLoad);
        PlayerPrefs.Save();

        // Load the loading scene
        SceneManager.LoadScene("LoadingScene");
    }
}