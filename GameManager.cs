using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Rounds")]
    public RoundData[] rounds;

    [Header("Settings")]
    public int currentRound = 0;
    public float roundDuration = 30f;
    public float timeBetweenRounds = 5f;

    [Header("Runtime")]
    public float asteroidSpawnMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
    public float lootDropMultiplier = 1f;

    [Header("Spawners")]
    public AsteroidSpawner asteroidSpawner;

    [Header("UI")]
    public TMP_Text roundTimerText;
    public TMP_Text roundStartCountdownText;
    public TMP_Text roundTitleText;

    public TMP_Text deathMessage;
    public Button playAgainButton;
    public Button goToMenuButton;

    private bool gameActive = false;
    private bool timingRound = false;
    private float currentRoundTime;

    [System.Serializable]
    public class RoundData
    {
        public string roundName = "Round";
        public float asteroidSpawnMultiplier = 1f;
        public float enemySpeedMultiplier = 1f;
        public float lootDropMultiplier = 1f;
        public int asteroidCount = 20;
        public int maxAsteroidsAlive = 10;
        public float minSpawnDelay = 0.2f;
        public float maxSpawnDelay = 1.2f;
        public int enemyCount = 0;
    }

    private void Awake()
    {
        // Persistent global manager
        if (Instance == null)
        {
            DetachChildrenBeforePersisting();
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Prevent Menu UI from persisting
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Prevent menu visuals from hitching a ride when this manager is persisted.
    /// If the GameManager lives on a UI root in the Menu scene, detach all
    /// children before calling DontDestroyOnLoad so the menu canvas/background
    /// remains in the menu scene and unloads normally.
    /// </summary>
    private void DetachChildrenBeforePersisting()
    {
        if (transform.childCount == 0)
            return;

        // Copy to list to avoid modifying while iterating children directly.
        var children = new List<Transform>(transform.childCount);
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(null, true);
        }
    }

    // Called every time ANY scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            // Menu scene: do not use gameplay UI
            asteroidSpawner = null;
            roundTimerText = null;
            roundStartCountdownText = null;
            roundTitleText = null;
            deathMessage = null;
            playAgainButton = null;
            goToMenuButton = null;

            gameActive = false;
            timingRound = false;
        }
        else
        {
            // Remove any menu UI that might have persisted with this manager
            CleanupMenuUI();

            // Reconnect UI and spawners in gameplay scene
            BindGameplayReferences(scene);

            ResetAndStartGameplay();
        }
    }

    /// <summary>
    /// Find and wire up gameplay references entirely in code so restarting from
    /// the menu always rebinds UI and spawners (even if nothing is assigned in
    /// the inspector or objects start inactive).
    /// </summary>
    private void BindGameplayReferences(Scene scene)
    {
        asteroidSpawner = FindInScene<AsteroidSpawner>(scene);

        roundTimerText = FindInSceneByName<TMP_Text>(scene, "RoundTimer");
        roundStartCountdownText = FindInSceneByName<TMP_Text>(scene, "RoundCountdown");
        roundTitleText = FindInSceneByName<TMP_Text>(scene, "RoundTitle");
        deathMessage = FindInSceneByName<TMP_Text>(scene, "DeathMessage");

        playAgainButton = FindInSceneByName<Button>(scene, "PlayAgain");
        goToMenuButton = FindInSceneByName<Button>(scene, "GoToMenu");
    }

    private T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            T result = root.GetComponentInChildren<T>(true);
            if (result != null)
                return result;
        }

        return null;
    }

    private T FindInSceneByName<T>(Scene scene, string targetName) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            T[] comps = root.GetComponentsInChildren<T>(true);
            foreach (var comp in comps)
            {
                if (comp.name == targetName)
                    return comp;
            }
        }

        return null;
    }

    /// <summary>
    /// When a gameplay scene loads (either from the menu or via Play Again),
    /// reset all runtime state and UI text before kicking off the round loop
    /// so countdowns and round titles restart properly.
    /// </summary>
    private void ResetAndStartGameplay()
    {
        StopAllCoroutines();

        timingRound = false;
        gameActive = false;
        currentRound = -1;

        if (roundTimerText != null)
            roundTimerText.text = Mathf.Ceil(roundDuration).ToString();

        if (roundStartCountdownText != null)
            roundStartCountdownText.text = "";

        if (roundTitleText != null && rounds != null && rounds.Length > 0)
            roundTitleText.text = rounds[0].roundName;

        HideDeathUI();
        StartGame();
    }

    private void CleanupMenuUI()
    {
        // Destroy any UI that is still riding along in the DontDestroyOnLoad
        // scene (e.g., the Menu canvas the GameManager lived on). This ensures
        // the menu is not visible behind gameplay scenes even if the canvas is
        // not a child of this GameManager.
        foreach (var canvas in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (canvas == null)
                continue;

            // Only remove canvases that persist in the DontDestroyOnLoad scene
            // and are not part of the newly loaded gameplay scene.
            if (canvas.gameObject.scene.name == "DontDestroyOnLoad")
                Destroy(canvas.gameObject);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;

        BindGameplayReferences(SceneManager.GetActiveScene());
        HideDeathUI();
        StartGame();
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;

        gameActive = true;
        currentRound = -1;
        StartCoroutine(RoundLoop());
    }

    private IEnumerator RoundLoop()
    {
        while (gameActive)
        {
            yield return StartCoroutine(StartNewRound());
            yield return new WaitForSeconds(roundDuration);
            EndCurrentRound();
            yield return new WaitForSeconds(timeBetweenRounds);
        }
    }

    private IEnumerator StartNewRound()
    {
        currentRound++;

        if (currentRound >= rounds.Length)
        {
            gameActive = false;
            yield break;
        }

        RoundData data = rounds[currentRound];

        if (roundTitleText != null)
            roundTitleText.text = data.roundName;

        yield return StartCoroutine(RoundStartCountdown());

        asteroidSpawnMultiplier = data.asteroidSpawnMultiplier;
        enemySpeedMultiplier = data.enemySpeedMultiplier;
        lootDropMultiplier = data.lootDropMultiplier;

        if (asteroidSpawner != null)
        {
            asteroidSpawner.minSpawnDelay = data.minSpawnDelay;
            asteroidSpawner.maxSpawnDelay = data.maxSpawnDelay;
            asteroidSpawner.StartSpawning(
                data.asteroidCount,
                data.asteroidSpawnMultiplier,
                data.maxAsteroidsAlive
            );
        }

        timingRound = true;
        StartCoroutine(RoundTimer());
    }

    private IEnumerator RoundStartCountdown()
    {
        int count = 3;
        while (count > 0)
        {
            if (roundStartCountdownText != null)
                roundStartCountdownText.text = count.ToString();

            yield return new WaitForSeconds(1f);
            count--;
        }

        if (roundStartCountdownText != null)
            roundStartCountdownText.text = "";
    }

    private IEnumerator RoundTimer()
    {
        currentRoundTime = roundDuration;

        while (timingRound)
        {
            currentRoundTime -= Time.deltaTime;

            if (roundTimerText != null)
                roundTimerText.text = Mathf.Ceil(currentRoundTime).ToString();

            if (currentRoundTime <= 0f)
                break;

            yield return null;
        }
    }

    private void EndCurrentRound()
    {
        timingRound = false;

        if (asteroidSpawner != null)
            asteroidSpawner.StopSpawning();
    }

    public void GameOver()
    {
        gameActive = false;
        timingRound = false;

        if (asteroidSpawner != null)
            asteroidSpawner.StopSpawning();

        StopAllCoroutines();
        ShowDeathUI();
    }

    private void ShowDeathUI()
    {
        if (deathMessage != null)
        {
            deathMessage.text = "YOU DIED";
            deathMessage.gameObject.SetActive(true);
        }

        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(true);

        if (goToMenuButton != null)
            goToMenuButton.gameObject.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideDeathUI()
    {
        if (deathMessage != null)
            deathMessage.gameObject.SetActive(false);

        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(false);

        if (goToMenuButton != null)
            goToMenuButton.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
