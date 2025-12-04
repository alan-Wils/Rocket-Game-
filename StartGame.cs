using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [Header("Scenes To Load")]
    public string[] scenes;

    [Header("Which Scene To Load")]
    public int sceneIndex = 0;

    public void PlayGame()
    {
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes assigned in the Inspector!");
            return;
        }

        if (sceneIndex < 0 || sceneIndex >= scenes.Length)
        {
            Debug.LogError("Scene index out of range!");
            return;
        }

        // Always load the scene in Single mode
        SceneManager.LoadScene(scenes[sceneIndex], LoadSceneMode.Single);
    }
}
