using UnityEngine;
using UnityEngine.SceneManagement; // Add this namespace

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource[] sfx;
    [SerializeField] private AudioSource[] bgm;
    [SerializeField] private int bgmIndex;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // The name of your main menu scene
    [SerializeField] private int mainMenuMusicIndex = 0; // Index of the main menu music in bgm array

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        if (bgm.Length <= 0)
            return;

        // Register to scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Check current scene on start
        CheckCurrentScene();
    }

    private void OnDestroy()
    {
        // Unregister from scene change events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check which scene was loaded
        CheckCurrentScene();
    }

    private void CheckCurrentScene()
    {
        // Check if current scene is main menu
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            // Play main menu music
            PlayBGM(mainMenuMusicIndex);
        }
        else
        {
            // For non-main menu scenes, continue with random music
            InvokeRepeating(nameof(PlayMusicIfNeeded), 0, 2);
        }
    }

    public void PlayMusicIfNeeded()
    {
        if (bgm[bgmIndex].isPlaying == false)
            PlayRandomBGM();
    }

    public void PlayRandomBGM()
    {
        bgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(bgmIndex);
    }

    public void PlayBGM(int bgmToPlay)
    {
        if (bgm.Length <= 0)
        {
            Debug.LogWarning("You have no music on audio manager!");
            return;
        }

        // Stop all currently playing BGM
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }

        bgmIndex = bgmToPlay;
        bgm[bgmToPlay].Play();
    }

    public void PlaySFX(int sfxToPlay, bool randomPitch = true)
    {
        if (sfxToPlay >= sfx.Length)
            return;

        if (randomPitch)
            sfx[sfxToPlay].pitch = Random.Range(.9f, 1.1f);

        sfx[sfxToPlay].Play();
    }

    public void StopSFX(int sfxToStop) => sfx[sfxToStop].Stop();
}