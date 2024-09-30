using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    #region Load Level
    public Action OnLoadedScene;
    public Action OnLoadScene;

    [BoxGroup("Param")]
    private float timeTransition = 1f;
    [Scene, SerializeField] private string scene_1;
    [Scene, SerializeField] private string scene_2;
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadLevelCanvas(Menu menu)
    {
        OnLoadedScene?.Invoke();
        yield return new WaitForSeconds(timeTransition);
        OnLoadedScene?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        OnLoadScene?.Invoke();

        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Prevent the scene from activating until it's fully loaded
        operation.allowSceneActivation = false;

        // While the scene is loading
        while (!operation.isDone)
        {
            // Get the progress (operation.progress ranges from 0 to 0.9)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Update the UI progress bar (optional)
            //if (progressBar != null)
            //{
            //    progressBar.value = progress;
            //}

            // Debug log to show progress percentage in the console
            Debug.Log($"Loading progress: {progress * 100}%");

            // If the loading is complete (progress is at 0.9), activate the scene
            if (operation.progress >= 0.9f)
            {
                // You can add a UI to press a button to activate the scene, or just auto-activate
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Scene has fully loaded
        Debug.Log("Scene loaded completely.");
        OnLoadedScene?.Invoke();
    }
    #endregion
}
