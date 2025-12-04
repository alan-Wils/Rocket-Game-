using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Asteroid : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public int quantity = 1;

        public bool randomChanceEnabled = false;
        [Range(0, 100)]
        public int spawnChance = 100;
    }

    [Header("Movement")]
    public float fallSpeed = 1.5f;

    [Header("Health")]
    public int health = 5;
    public int crackedThreshold = 3;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite crackedSprite;
    public Sprite explosionSprite;

    [Header("Visual Effects")]
    public GameObject hitFlashEffect;
    public GameObject explosionEffect; // optional (explosion VFX prefab)

    [Header("Player Damage")]
    public int playerCollisionDamage = 1;

    [Header("Spawn Settings")]
    public List<SpawnEntry> spawnPrefabs = new List<SpawnEntry>();
    public float spawnRadius = 1.0f;

    [Header("Points")]
    public int pointsPerAsteroid = 20;

    private SpriteRenderer sr;
    private bool isCracked = false;
    private bool isExploding = false;

    private Inventory inventory;
    [HideInInspector] public AsteroidSpawner spawner; // IMPORTANT FOR alive count

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Safety fallback
        if (sr != null && normalSprite != null)
            sr.sprite = normalSprite;

        // Find player inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            inventory = player.GetComponent<Inventory>();
    }

    private void Update()
    {
        if (!isExploding)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }

        // Off-screen cleanup
        if (Camera.main.WorldToViewportPoint(transform.position).y < -0.2f)
            Destroy(gameObject);
    }

    // ---------------------------------------------------------
    // DAMAGE HANDLING
    // ---------------------------------------------------------
    public void TakeDamage(int dmg)
    {
        if (isExploding) return;

        health -= dmg;

        // Crack sprite
        if (!isCracked && health <= crackedThreshold)
        {
            if (crackedSprite != null && sr != null)
                sr.sprite = crackedSprite;

            isCracked = true;
            return;
        }

        // Destroy asteroid
        if (health <= 0)
        {
            StartCoroutine(Explode());
        }
    }

    // ---------------------------------------------------------
    // EXPLOSION
    // ---------------------------------------------------------
    private IEnumerator Explode()
    {
        isExploding = true;
        fallSpeed = 0f;

        // Explosion sprite
        if (explosionSprite != null && sr != null)
            sr.sprite = explosionSprite;

        // Optional explosion effect prefab
        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 1.2f);
        }

        // Slight scale-up effect
        transform.localScale *= 1.15f;

        // Award points
        AwardPoints();

        // Spawn loot pieces
        SpawnPieces();

        yield return new WaitForSeconds(0.15f);

        Destroy(gameObject);
    }

    // ---------------------------------------------------------
    // SPAWN LOOT PIECES
    // ---------------------------------------------------------
    private void SpawnPieces()
    {
        foreach (SpawnEntry entry in spawnPrefabs)
        {
            if (entry.prefab == null || entry.quantity <= 0)
                continue;

            // Chance to spawn
            if (entry.randomChanceEnabled)
            {
                int roll = Random.Range(0, 101);
                if (roll > entry.spawnChance)
                    continue;
            }

            // Spawn count
            for (int i = 0; i < entry.quantity; i++)
            {
                Vector2 randCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 offset = new Vector3(randCircle.x, randCircle.y, 0f);

                GameObject pieceObj = Instantiate(entry.prefab, transform.position + offset, Quaternion.identity);

                // Apply explosion direction to pieces
                AsteroidPiece piece = pieceObj.GetComponent<AsteroidPiece>();
                if (piece != null)
                {
                    Vector2 dir = offset.normalized;
                    piece.SetExplosionDirection(dir);
                }
            }
        }
    }

    // ---------------------------------------------------------
    // COLLISIONS
    // ---------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Laser hit
        if (other.CompareTag("Laser"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
            return;
        }

        // Player hit
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(playerCollisionDamage);

            Vector3 hitPos = other.ClosestPoint(transform.position);

            if (hitFlashEffect != null)
            {
                GameObject fx = Instantiate(hitFlashEffect, hitPos, Quaternion.identity);
                Destroy(fx, 0.35f);
            }

            TakeDamage(1);
        }
    }

    // ---------------------------------------------------------
    // POINT REWARD
    // ---------------------------------------------------------
    private void AwardPoints()
    {
        if (inventory != null)
            inventory.AddPoints(pointsPerAsteroid);
    }

    // ---------------------------------------------------------
    // INFORM SPAWNER THAT THIS ASTEROID IS GONE
    // ---------------------------------------------------------
    private void OnDestroy()
    {
        if (spawner != null)
            spawner.AsteroidDestroyed();
    }
}
