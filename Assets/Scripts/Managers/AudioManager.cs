using System.Collections;
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
        // Check if current scene is level 1
        else if (SceneManager.GetActiveScene().name == "Level_1")
        {
            // Play level 1 music
            PlayBGM(0);
        }
        // check if current scene is level 2
        else if (SceneManager.GetActiveScene().name == "Level_2")
        {
            // Play level 2 music
            PlayBGM(1);
        }
        // check if current scene is level 3
        else if (SceneManager.GetActiveScene().name == "Level_3")
        {
            // Play level 3 music
            PlayBGM(2);
        }
        else
        {
            // For non-main menu scenes, stop music
            CancelInvoke(nameof(PlayMusicIfNeeded));
            for (int i = 0; i < bgm.Length; i++)
            {
                bgm[i].Stop();
            }
            
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

    public void PlaySFX(int sfxToPlay)
    {
        if (sfxToPlay >= sfx.Length)
            return;

        //if (randomPitch)
        //    sfx[sfxToPlay].pitch = Random.Range(.9f, 1.1f);

        sfx[sfxToPlay].Play();
    }
    public void PlaySFXwithRandomPitch(int sfxToPlay, bool randomPitch = true)
    {
        if (sfxToPlay >= sfx.Length)
            return;

        if (randomPitch)
            sfx[sfxToPlay].pitch = Random.Range(.85f, 1.15f);

        sfx[sfxToPlay].Play();
    }
    public void PlaySFXByDuration(int sfxToPlay, float duration)
    {
        if (sfxToPlay >= sfx.Length)
            return;
        sfx[sfxToPlay].Play();

        StartCoroutine(StopSFXAfterDuration(sfxToPlay, duration));
    }
    private IEnumerator StopSFXAfterDuration(int sfxIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        sfx[sfxIndex].Stop();
    }
    // Add these methods to your AudioManager class
    public void PlaySFXLooped(int sfxToPlay)
    {
        if (sfxToPlay >= sfx.Length)
            return;

        // Enable looping
        sfx[sfxToPlay].loop = true;
        sfx[sfxToPlay].Play();
    }

    public void PlaySFXLoopedByDuration(int sfxToPlay, float duration)
    {
        if (sfxToPlay >= sfx.Length)
            return;

        sfx[sfxToPlay].loop = true;
        sfx[sfxToPlay].Play();

        StartCoroutine(StopSFXLoopAfterDuration(sfxToPlay, duration));
    }

    private IEnumerator StopSFXLoopAfterDuration(int sfxIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        sfx[sfxIndex].loop = false;
        sfx[sfxIndex].Stop();
    }

    public void LowerBGMVolumeSlowly()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].volume = bgm[i].volume - 0.1f;
        }
        
    }
    public void StopSFX(int sfxToStop) => sfx[sfxToStop].Stop();
}