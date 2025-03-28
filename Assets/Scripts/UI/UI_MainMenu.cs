using UnityEngine;
using UnityEngine.SceneManagement;
public class UI_MainMenu : MonoBehaviour
{
    private UI_FadeEffect fadeEffect;
    public string FirstLevelName;
    [SerializeField] private GameObject[] uiElements;
    [SerializeField] private GameObject continueButton;
    // Scene Names
    private const string LOADING_SCENE = "LoadingScene";
    [SerializeField] private string introSceneName = "Intro";

    private void Awake()
    {
        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
    }

    private void Start()
    {
        if (HasLevelProgression())
            continueButton.SetActive(true);
        fadeEffect.ScreenFade(0, 1.5f);
    }

    public void SwitchUI(GameObject uiToEnable)
    {
        foreach (GameObject ui in uiElements)
        {
            ui.SetActive(false);
        }
        uiToEnable.SetActive(true);
    }

    public void NewGame()
    {
        // Store that we're starting a new game (to distinguish from continuing)
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();

        // Load the intro scene directly
        fadeEffect.ScreenFade(1, 1.5f, () => SceneManager.LoadScene(introSceneName));
    }

    private void LoadLevelWithLoading(int sceneIndex)
    {
        // Store the scene to load in PlayerPrefs
        PlayerPrefs.SetInt("SceneToLoad", sceneIndex);
        PlayerPrefs.Save();
        // Load the loading scene
        SceneManager.LoadScene(LOADING_SCENE);
        Debug.Log(LOADING_SCENE);
    }

    private bool HasLevelProgression()
    {
        bool hasLevelProgression = PlayerPrefs.GetInt("ContinueLevelNumber", 0) > 0;
        return hasLevelProgression;
    }

    public void ContinueGame()
    {
        int difficultyIndex = PlayerPrefs.GetInt("GameDifficulty", 1);
        int levelToLoad = PlayerPrefs.GetInt("ContinueLevelNumber", 0);
        //DifficultyManager.instance.LoadDifficulty(difficultyIndex);
        LoadLevelWithLoading(levelToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}