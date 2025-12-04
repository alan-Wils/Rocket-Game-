using UnityEngine;

public class RocketController : MonoBehaviour
{
    public float yPosition = -4f;
    public float moveSpeed = 15f;
    public float tiltAmount = 25f;

    public GameObject laserPrefab;
    public Transform shootPoint;
    public float shootCooldown = 0.15f;

    private float leftX;
    private float rightX;

    private float shootTimer;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        UpdateMovementBounds();
    }

    void Update()
    {
        MoveRocket();
        ShootLaser();
    }

    void UpdateMovementBounds()
    {
        Camera cam = Camera.main;

        float zDist = Mathf.Abs(cam.transform.position.z - transform.position.z);

        float camLeft = cam.rect.xMin * Screen.width;
        float camRight = cam.rect.xMax * Screen.width;

        float worldLeft = cam.ScreenToWorldPoint(new Vector3(camLeft, 0, zDist)).x;
        float worldRight = cam.ScreenToWorldPoint(new Vector3(camRight, 0, zDist)).x;

        leftX = worldLeft + 0.5f;
        rightX = worldRight - 0.5f;
    }

    void MoveRocket()
    {
        float mousePercentX = Mathf.Clamp01(Input.mousePosition.x / Screen.width);

        float targetX = Mathf.Lerp(leftX, rightX, mousePercentX);

        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * moveSpeed);

        transform.position = new Vector3(newX, yPosition, transform.position.z);

        float diff = targetX - newX;
        float angle = -diff * tiltAmount;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void ShootLaser()
    {
        shootTimer -= Time.deltaTime;

        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && shootTimer <= 0f)
        {
            Instantiate(laserPrefab, shootPoint.position, shootPoint.rotation);
            shootTimer = shootCooldown;
        }
    }

    // ---------------------------------------------------------
    // NEW METHOD — Called by Asteroid after pushing the player
    // ---------------------------------------------------------
    public void RestoreAim()
    {
        // Instantly re-align rocket to follow the mouse movement again.
        float mousePercentX = Mathf.Clamp01(Input.mousePosition.x / Screen.width);

        float targetX = Mathf.Lerp(leftX, rightX, mousePercentX);

        // Set the X position directly (no smoothing)
        transform.position = new Vector3(targetX, yPosition, transform.position.z);

        // Reset tilt
        transform.rotation = Quaternion.identity;
    }
}
