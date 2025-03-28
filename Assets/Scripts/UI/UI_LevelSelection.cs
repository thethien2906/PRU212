using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_LevelSelection : MonoBehaviour
{
    [SerializeField] private UI_LevelButton buttonPrefab;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private bool[] levelsUnlocked;

    // Loading Scene
    private const string LOADING_SCENE = "LoadingScene";

    private void Start()
    {
        LoadLevelsInfo();
        CreateLevelButtons();
    }

    private void CreateLevelButtons()
    {
        // Changed to account for LoadingScene
        int levelsAmount = SceneManager.sceneCountInBuildSettings - 3; // MainMenu, LoadingScene, TheEnd

        for (int i = 1; i < levelsAmount; i++)
        {
            if (IsLevelUnlocked(i) == false)
                return;

            UI_LevelButton newButton = Instantiate(buttonPrefab, buttonsParent);
            newButton.SetupButton(i);
        }
    }

    private bool IsLevelUnlocked(int levelIndex) => levelsUnlocked[levelIndex];

    private void LoadLevelsInfo()
    {
        // Changed to account for LoadingScene
        int levelsAmount = SceneManager.sceneCountInBuildSettings - 3; // MainMenu, LoadingScene, TheEnd

        levelsUnlocked = new bool[levelsAmount];

        for (int i = 1; i < levelsAmount; i++)
        {
            bool levelUnlocked = PlayerPrefs.GetInt("Level" + i + "Unlocked", 0) == 1;
            if (levelUnlocked)
                levelsUnlocked[i] = true;
        }

        levelsUnlocked[1] = true; // First level always unlocked
    }

    // Add this method to be called by the UI_LevelButton
    public void LoadLevelWithLoading(int levelIndex)
    {
        // Store the scene to load in PlayerPrefs
        PlayerPrefs.SetInt("SceneToLoad", levelIndex);
        PlayerPrefs.Save();

        // Load the loading scene
        SceneManager.LoadScene(LOADING_SCENE);
    }
}