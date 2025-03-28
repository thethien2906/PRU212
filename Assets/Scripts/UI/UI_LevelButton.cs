using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    private int levelIndex;
    private string sceneName;

    // Loading Scene
    private const string LOADING_SCENE = "LoadingScene";

    public void SetupButton(int newLevelIndex)
    {
        levelIndex = newLevelIndex;
        levelNumberText.text = "Level " + levelIndex;
        sceneName = "Level_" + levelIndex;

        // Display best time if available
        if (bestTimeText != null)
        {
            bestTimeText.text = TimerInfoText();
        }
    }

    public void LoadLevel()
    {
        int difficultyIndex = ((int)DifficultyManager.instance.difficulty);
        PlayerPrefs.SetInt("GameDifficulty", difficultyIndex);

        // Instead of loading the scene directly, set up for loading scene
        PlayerPrefs.SetInt("SceneToLoad", levelIndex);
        PlayerPrefs.Save();

        // Load the loading scene
        SceneManager.LoadScene(LOADING_SCENE);
    }

    private string TimerInfoText()
    {
        float timerValue = PlayerPrefs.GetFloat("Level" + levelIndex + "BestTime", 99);

        // Convert seconds to minutes:seconds format
        int minutes = Mathf.FloorToInt(timerValue / 60);
        int seconds = Mathf.FloorToInt(timerValue % 60);

        return "Best Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}