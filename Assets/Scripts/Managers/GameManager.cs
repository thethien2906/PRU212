using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private UI_InGame inGameUI;

    [Header("Level Managment")]
    [SerializeField] private float levelTimer;
    [SerializeField] private int currentLevelIndex;
    private int nextLevelIndex;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay;
    public Player player;

    [Header("Checkpoints")]
    public bool canReactivate;

    // Loading Scene
    private const string LOADING_SCENE = "LoadingScene";

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        inGameUI = UI_InGame.instance;

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        nextLevelIndex = currentLevelIndex + 1;
    }

    private void Update()
    {
        levelTimer += Time.deltaTime;

        inGameUI.UpdateTimerUI(levelTimer);
    }

    public void UpdateRespawnPosition(Transform newRespawnPoint) => respawnPoint = newRespawnPoint;

    public void RespawnPlayer()
    {
        //DifficultyManager difficultyManager = DifficultyManager.instance;

        //if (difficultyManager != null && difficultyManager.difficulty == DifficultyType.Hard)
        //    return;
        StartCoroutine(RespawnCourutine());
    }

    private IEnumerator RespawnCourutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        GameObject newPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        CameraManager.instance.OnPlayerRespawn(newPlayer);
        player = newPlayer.GetComponent<Player>();
        AudioManager.instance.PlaySFX(UnityEngine.Random.Range(34, 39));

        Debug.Log("Respawned");
    }

    public void CreateObject(GameObject prefab, Transform target, float delay = 0)
    {
        StartCoroutine(CreateObjectCourutine(prefab, target, delay));
    }
    private IEnumerator CreateObjectCourutine(GameObject prefab, Transform target, float delay)
    {
        Vector3 newPosition = target.position;

        yield return new WaitForSeconds(delay);

        GameObject newObject = Instantiate(prefab, newPosition, Quaternion.identity);
    }
    public void LevelFinished()
    {
        SaveLevelProgression();
        SaveBestTime();

        LoadNextSceneWithLoading();
    }

    private void SaveBestTime()
    {
        float lastTime = PlayerPrefs.GetFloat("Level" + currentLevelIndex + "BestTime", 99);

        if (levelTimer < lastTime)
            PlayerPrefs.SetFloat("Level" + currentLevelIndex + "BestTime", levelTimer);
    }
    private void SaveLevelProgression()
    {
        PlayerPrefs.SetInt("Level" + nextLevelIndex + "Unlocked", 1);

        if (NoMoreLevels() == false)
            PlayerPrefs.SetInt("ContinueLevelNumber", nextLevelIndex);
    }

    public void RestartLevel()
    {
        UI_InGame.instance.fadeEffect.ScreenFade(1, .75f, () => LoadSceneWithLoading(currentLevelIndex));
    }

    private void LoadSceneWithLoading(int sceneIndex)
    {
        // Store the scene to load in PlayerPrefs
        PlayerPrefs.SetInt("SceneToLoad", sceneIndex);
        PlayerPrefs.Save();

        // Load the loading scene
        SceneManager.LoadScene(LOADING_SCENE);
    }

    private void LoadNextSceneWithLoading()
    {
        UI_FadeEffect fadeEffect = UI_InGame.instance.fadeEffect;

        if (NoMoreLevels())
        {
            // For TheEnd scene
            int theEndIndex = SceneManager.sceneCountInBuildSettings - 1;
            fadeEffect.ScreenFade(1, 3f, () => LoadSceneWithLoading(theEndIndex));
        }
        else
        {
            // For the next level
            fadeEffect.ScreenFade(1, 1f, () => LoadSceneWithLoading(nextLevelIndex));
        }
    }

    private bool NoMoreLevels()
    {
        int lastLevelIndex = SceneManager.sceneCountInBuildSettings - 4; // We have main menu, loading scene and The End scene, that's why we use number 3
        bool noMoreLevels = currentLevelIndex == lastLevelIndex;

        return noMoreLevels;
    }
}