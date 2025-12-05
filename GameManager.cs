using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        HideDeathUI();
        StartGame();
    }

    // ---------------------------------------------------------
    // START GAME
    // ---------------------------------------------------------
    public void StartGame()
    {
        gameActive = true;
        currentRound = -1;
        StartCoroutine(RoundLoop());
    }

    // ---------------------------------------------------------
    // MAIN LOOP
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // START ROUND
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // COUNTDOWN
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // TIMER
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // END ROUND
    // ---------------------------------------------------------
    private void EndCurrentRound()
    {
        timingRound = false;

        if (asteroidSpawner != null)
            asteroidSpawner.StopSpawning();
    }

    // ---------------------------------------------------------
    // GAME OVER CALLED BY PLAYERHEALTH
    // ---------------------------------------------------------
    public void GameOver()
    {
        gameActive = false;
        timingRound = false;

        if (asteroidSpawner != null)
            asteroidSpawner.StopSpawning();

        StopAllCoroutines();
        ShowDeathUI();
    }

    // ---------------------------------------------------------
    // UI FUNCTIONS
    // ---------------------------------------------------------
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

        // -------------------------------------------------
        // MAKE CURSOR VISIBLE WHEN GAME OVER SCREEN OPENS
        // -------------------------------------------------
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

        // Optional: hide cursor when gameplay starts
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ---------------------------------------------------------
    // BUTTON CALLBACKS
    // ---------------------------------------------------------
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
