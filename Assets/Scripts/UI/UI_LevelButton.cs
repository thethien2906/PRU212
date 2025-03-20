using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    private int levelIndex;
    private string sceneName;

    public void SetupButton(int newLevelIndex)
    {
        levelIndex = newLevelIndex;
        levelNumberText.text = "Level " + levelIndex;
        sceneName = "Level_" + levelIndex;
    }

    public void LoadLevel()
    {
        int difficultyIndex = ((int)DifficultyManager.instance.difficulty);
        PlayerPrefs.SetInt("GameDifficulty", difficultyIndex);
        SceneManager.LoadScene(sceneName);
    }

    private string TimerInfoText()
    {
        float timerValue = PlayerPrefs.GetFloat("Level" + levelIndex + "BestTime", 99);
        return "Best Time: " + timerValue.ToString("00");
    }
}