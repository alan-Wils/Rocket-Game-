using UnityEngine;

public class PointGem : MonoBehaviour
{
    public float fallSpeed = 1f;
    public float sideDrift = 0f;

    // Points given to the player when collected
    public int pointsValue = 10;

    // Optional pickup glow/spark effect
    public GameObject pickupEffect;

    private Vector2 burstVelocity;
    private float burstDamping = 4f;
    private float gravityBlendSpeed = 2f;
    private float gravityWeight = 0f;

    private bool collected = false;

    public void SetExplosionDirection(Vector2 dir)
    {
        burstVelocity = dir * Random.Range(3f, 5f);
    }

    void Update()
    {
        // Same asteroid-style motion
        gravityWeight = Mathf.MoveTowards(gravityWeight, 1f, gravityBlendSpeed * Time.deltaTime);
        burstVelocity = Vector2.Lerp(burstVelocity, Vector2.zero, burstDamping * Time.deltaTime);

        Vector2 fallMotion = new Vector2(sideDrift, -fallSpeed) * gravityWeight;
        Vector2 finalVelocity = (burstVelocity * (1 - gravityWeight)) + fallMotion;

        transform.position += (Vector3)(finalVelocity * Time.deltaTime);

        // Auto-destroy if off screen
        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            // Get the player's Inventory component
            Inventory inv = other.GetComponent<Inventory>();
            if (inv != null)
                inv.AddPoints(pointsValue);

            if (pickupEffect != null)
            {
                GameObject fx = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(fx, 0.35f);
            }

            Destroy(gameObject);
        }
    }
}
