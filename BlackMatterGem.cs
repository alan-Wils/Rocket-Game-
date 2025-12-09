using UnityEngine;

public class BlackMatterGem : MonoBehaviour
{
    public float fallSpeed = 1f;
    public float sideDrift = 0f;

    // Amount of health the gem adds to the player
    public int healAmount = 1;

    // Optional pick-up flash/shine effect
    public GameObject hitFlashEffect;

    private Vector2 burstVelocity;
    private float burstDamping = 4f;
    private float gravityBlendSpeed = 2f;
    private float gravityWeight = 0f;

    private bool hasHealedPlayer = false;

    public void SetExplosionDirection(Vector2 dir)
    {
        // Keep same asteroid burst animation
        burstVelocity = dir * Random.Range(3f, 5f);
    }

    void Update()
    {
        // same movement system as your asteroid
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
        // LASERS DO NOTHING
        if (other.CompareTag("Laser"))
            return;

        // PLAYER PICKUP — only once
        if (other.CompareTag("Player") && !hasHealedPlayer)
        {
            hasHealedPlayer = true;

            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.AddHealth(healAmount); // Make sure your PlayerHealth has AddHealth()

            Vector3 hitPos = other.ClosestPoint(transform.position);

            if (hitFlashEffect != null)
            {
                GameObject fx = Instantiate(hitFlashEffect, hitPos, Quaternion.identity);
                Destroy(fx, 0.35f);
            }

            Destroy(gameObject); // gem disappears after giving health
        }
    }
}
