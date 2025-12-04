using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (Camera.main.WorldToViewportPoint(transform.position).y > 1.2f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Big asteroid
        Asteroid big = other.GetComponent<Asteroid>();
        if (big != null)
        {
            big.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Small asteroid piece
        AsteroidPiece small = other.GetComponent<AsteroidPiece>();
        if (small != null)
        {
            small.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}
