using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_InGame : MonoBehaviour
{
    public static UI_InGame instance;
    public UI_FadeEffect fadeEffect { get; private set; } // read-only

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject pauseUI;

    private bool isPaused;

    // Loading Scene
    private const string LOADING_SCENE = "LoadingScene";

    private void Awake()
    {
        instance = this;
        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
    }

    private void Start()
    {
        fadeEffect.ScreenFade(0, 1);

        // Ensure the game is not paused when starting a level
        Time.timeScale = 1;
        isPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            PauseButton();
    }

    public void PauseButton()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        pauseUI.SetActive(true);
        Debug.Log("Paused");
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        pauseUI.SetActive(false);
        Debug.Log("Unpaused");
    }

    public void GoToMainMenuButton()
    {
        // IMPORTANT: Reset time scale before switching scenes
        Time.timeScale = 1;

        // Use loading scene to go back to the main menu (index 0)
        PlayerPrefs.SetInt("SceneToLoad", 0);
        PlayerPrefs.Save();

        // Add a fade effect if desired
        fadeEffect.ScreenFade(1, 0.5f, () => SceneManager.LoadScene(LOADING_SCENE));
    }

    public void UpdateTimerUI(float timer)
    {
        // Format time as minutes:seconds
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // This ensures Time.timeScale is reset when the scene is unloaded
    private void OnDestroy()
    {
        Time.timeScale = 1;
    }
}