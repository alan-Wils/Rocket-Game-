using UnityEngine;

public class RubyGem : MonoBehaviour
{
    public float fallSpeed = 1f;
    public float sideDrift = 0f;

    // How many rubies the player receives
    public int rubyValue = 1;

    // Optional pickup effect
    public GameObject pickupEffect;

    private Vector2 burstVelocity;
    private float burstDamping = 4f;
    private float gravityBlendSpeed = 2f;
    private float gravityWeight = 0f;

    private bool collected = false;

    // Automatically found on spawn
    private Inventory inventory;

    public void SetExplosionDirection(Vector2 dir)
    {
        burstVelocity = dir * Random.Range(3f, 5f);
    }

    void Start()
    {
        // Auto-find the player's inventory (supports spawned player)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            inventory = player.GetComponent<Inventory>();
    }

    void Update()
    {
        // Same falling + burst motion
        gravityWeight = Mathf.MoveTowards(gravityWeight, 1f, gravityBlendSpeed * Time.deltaTime);
        burstVelocity = Vector2.Lerp(burstVelocity, Vector2.zero, burstDamping * Time.deltaTime);

        Vector2 fallMotion = new Vector2(sideDrift, -fallSpeed) * gravityWeight;
        Vector2 finalVelocity = (burstVelocity * (1 - gravityWeight)) + fallMotion;

        transform.position += (Vector3)(finalVelocity * Time.deltaTime);

        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            // Add rubies to the player's inventory
            if (inventory != null)
                inventory.AddRubies(rubyValue);

            if (pickupEffect != null)
            {
                GameObject fx = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(fx, 0.35f);
            }

            Destroy(gameObject);
        }
    }
}
