using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private float minLoadTime = 1.5f; // Minimum loading time to show progress

    [Header("Loading Messages")]
    [SerializeField] private string[] loadingMessages;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageChangeInterval = 3f;

    private void Start()
    {
        // Reset UI
        progressBar.fillAmount = 0f;
        progressText.text = "0%";

        // Start the loading process
        StartCoroutine(LoadNextScene());

        // Start changing loading tips if we have any
        if (loadingMessages != null && loadingMessages.Length > 0 && messageText != null)
        {
            StartCoroutine(CycleLoadingMessages());
        }
    }

    private IEnumerator CycleLoadingMessages()
    {
        int messageIndex = 0;

        while (true)
        {
            messageText.text = loadingMessages[messageIndex];
            messageIndex = (messageIndex + 1) % loadingMessages.Length;
            yield return new WaitForSeconds(messageChangeInterval);
        }
    }

    private IEnumerator LoadNextScene()
    {
        // Get the scene to load from PlayerPrefs
        int sceneToLoad = PlayerPrefs.GetInt("SceneToLoad", 1); // Default to Level_1 if not set

        // Start async loading operation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;
        float progress = 0f;

        // Update the progress bar while loading
        while (!asyncLoad.isDone)
        {
            // Calculate real progress (0-0.9 from AsyncOperation)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update UI with smooth lerping
            progressBar.fillAmount = progress;
            progressText.text = Mathf.Floor(progress * 100f) + "%";

            // If loading is almost done and minimum time has passed
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minLoadTime)
            {
                // Allow scene activation
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}