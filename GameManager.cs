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
            ReconnectGameplayUI(scene);
            asteroidSpawner = FindObjectOfType<AsteroidSpawner>();

            RestartGameAfterSceneLoad();
        }
    }

    /// <summary>
    /// Reacquire gameplay UI references even if the scene authoring keeps them
    /// disabled by default. Using Resources.FindObjectsOfTypeAll allows us to
    /// locate inactive UI so we can show/hide it during restarts.
    /// </summary>
    /// <param name="scene">The newly loaded gameplay scene.</param>
    private void ReconnectGameplayUI(Scene scene)
    {
        roundTimerText = null;
        roundStartCountdownText = null;
        roundTitleText = null;
        deathMessage = null;
        playAgainButton = null;
        goToMenuButton = null;

        foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (t.gameObject.scene != scene)
                continue;

            if (t.name == "RoundTimer") roundTimerText = t;
            if (t.name == "RoundCountdown") roundStartCountdownText = t;
            if (t.name == "RoundTitle") roundTitleText = t;
            if (t.name == "DeathMessage") deathMessage = t;
        }

        foreach (var b in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (b.gameObject.scene != scene)
                continue;

            if (b.name == "PlayAgain") playAgainButton = b;
            if (b.name == "GoToMenu") goToMenuButton = b;
        }
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

        HideDeathUI();
        StartGame();
    }

    /// <summary>
    /// When a gameplay scene is (re)loaded via Play Again, restart all runtime
    /// state and begin the round loop again. This is invoked from OnSceneLoaded
    /// because the GameManager is not recreated when scenes change.
    /// </summary>
    private void RestartGameAfterSceneLoad()
    {
        StopAllCoroutines();
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
