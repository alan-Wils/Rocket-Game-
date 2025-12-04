using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidSpawner : MonoBehaviour
{
    public static AsteroidSpawner Instance;

    [Header("Asteroid Prefab")]
    public GameObject asteroidPrefab;

    [Header("Spawn Timing")]
    public float minSpawnDelay = 0.2f;
    public float maxSpawnDelay = 1.2f;

    private float leftX;
    private float rightX;

    private int maxAsteroids = 5;
    private int asteroidsToSpawn = 0;
    private int asteroidsAlive = 0;

    private bool spawningActive = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Camera cam = Camera.main;

        // Correct world-bounds calculation even with a forced aspect ratio
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        leftX = cam.transform.position.x - halfWidth + 0.5f;
        rightX = cam.transform.position.x + halfWidth - 0.5f;
    }

    // ---------------------------------------------------------
    // CALLED BY GAME MANAGER
    // ---------------------------------------------------------
    public void StartSpawning(int amount, float multiplier, int maxAlive)
    {
        asteroidsToSpawn = Mathf.RoundToInt(amount * multiplier);
        maxAsteroids = maxAlive;

        if (!spawningActive)
        {
            spawningActive = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        spawningActive = false;
    }

    // ---------------------------------------------------------
    // MAIN SPAWN LOOP
    // ---------------------------------------------------------
    private IEnumerator SpawnRoutine()
    {
        while (spawningActive)
        {
            if (asteroidsToSpawn <= 0)
            {
                spawningActive = false;
                yield break;
            }

            if (asteroidsAlive >= maxAsteroids)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            SpawnOneAsteroid();

            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }

    // ---------------------------------------------------------
    // SPAWN ONE ASTEROID
    // ---------------------------------------------------------
    private void SpawnOneAsteroid()
    {
        float spawnX = Random.Range(leftX, rightX);

        Vector3 pos = new Vector3(spawnX, transform.position.y, 0f);

        GameObject a = Instantiate(asteroidPrefab, pos, Quaternion.identity);

        asteroidsAlive++;
        asteroidsToSpawn--;

        Asteroid aScript = a.GetComponent<Asteroid>();
        if (aScript != null)
            aScript.spawner = this;
    }

    public void AsteroidDestroyed()
    {
        asteroidsAlive--;
    }
}
