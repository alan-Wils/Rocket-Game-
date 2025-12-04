using UnityEngine;
using UnityEngine.Events;
using System.Collections;
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
            asteroidSpawner = FindObjectOfType<AsteroidSpawner>();
            TMP_Text[] texts = FindObjectsOfType<TMP_Text>();
            Button[] buttons = FindObjectsOfType<Button>();

            foreach (var t in texts)
            {
                if (t.name == "RoundTimer") roundTimerText = t;
                if (t.name == "RoundCountdown") roundStartCountdownText = t;
                if (t.name == "RoundTitle") roundTitleText = t;
                if (t.name == "DeathMessage") deathMessage = t;
            }

            foreach (var b in buttons)
            {
                if (b.name == "PlayAgain") playAgainButton = b;
                if (b.name == "GoToMenu") goToMenuButton = b;
            }
        }
    }

    private void CleanupMenuUI()
    {
        // If this GameManager lived on the Menu Canvas, its children would
        // persist because of DontDestroyOnLoad. Destroy any Canvas objects
        // that came along so the menu is not visible in gameplay scenes.
        foreach (var canvas in GetComponentsInChildren<Canvas>(true))
        {
            // Skip canvases that belong to the newly loaded scene
            if (canvas.gameObject.scene == SceneManager.GetActiveScene())
                continue;

            Destroy(canvas.gameObject);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;

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
