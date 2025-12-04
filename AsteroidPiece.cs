using UnityEngine;

public class AsteroidPiece : MonoBehaviour
{
    public float fallSpeed = 1f;
    public float sideDrift = 0f;

    public Sprite explosionSprite;
    public int health = 2;

    // Damage dealt to the player
    public int damageToPlayer = 1;

    // Points awarded when asteroid is destroyed
    public int pointsPerAsteroid = 5;

    // Optional hit-flash effect
    public GameObject hitFlashEffect;

    private Vector2 burstVelocity;
    private float burstDamping = 4f;
    private float gravityBlendSpeed = 2f;
    private float gravityWeight = 0f;

    private SpriteRenderer sr;
    private bool isExploding = false;

    // Prevents multiple player hits
    private bool hasHitPlayer = false;

    // Cached reference to Inventory
    private Inventory inventory;

    public void SetExplosionDirection(Vector2 dir)
    {
        burstVelocity = dir * Random.Range(3f, 5f);
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Auto-find inventory from the spawned player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            inventory = player.GetComponent<Inventory>();
    }

    void Update()
    {
        if (isExploding)
            return;

        gravityWeight = Mathf.MoveTowards(gravityWeight, 1f, gravityBlendSpeed * Time.deltaTime);
        burstVelocity = Vector2.Lerp(burstVelocity, Vector2.zero, burstDamping * Time.deltaTime);

        Vector2 fallMotion = new Vector2(sideDrift, -fallSpeed) * gravityWeight;
        Vector2 finalVelocity = (burstVelocity * (1 - gravityWeight)) + fallMotion;

        transform.position += (Vector3)(finalVelocity * Time.deltaTime);

        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    public void TakeDamage(int dmg)
    {
        if (isExploding)
            return;

        health -= dmg;

        if (health <= 0)
        {
            AwardPoints();  // Add points ONLY when destroyed by damage (i.e., laser)
            Explode();
        }
    }

    void Explode()
    {
        if (isExploding)
            return;

        isExploding = true;

        sr.sprite = explosionSprite;
        fallSpeed = 0f;
        burstVelocity = Vector2.zero;

        transform.localScale *= 1.05f;

        Destroy(gameObject, 0.15f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Laser hit
        if (other.CompareTag("Laser"))
        {
            TakeDamage(1);     // This will award points if it kills the asteroid
            Destroy(other.gameObject);
            return;
        }

        // Player hit — only allow one hit
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;

            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damageToPlayer);

            Vector3 hitPos = other.ClosestPoint(transform.position);

            if (hitFlashEffect != null)
            {
                GameObject fx = Instantiate(hitFlashEffect, hitPos, Quaternion.identity);
                Destroy(fx, 0.35f);
            }

            // NO MORE POINTS HERE
            // Previously: AwardPoints();

            Explode();
        }
    }

    // Add points to the inventory
    private void AwardPoints()
    {
        if (inventory != null)
            inventory.AddPoints(pointsPerAsteroid);
    }
}
