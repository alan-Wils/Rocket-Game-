using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    [Tooltip("Camera used for scaling. Defaults to Camera.main if not assigned.")]
    public Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("BackgroundScaler requires a SpriteRenderer.");
            enabled = false;
            return;
        }

        if (sr.sprite == null)
        {
            Debug.LogError("BackgroundScaler: SpriteRenderer is missing a sprite.");
            enabled = false;
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogError("BackgroundScaler: No camera available to scale against.");
            enabled = false;
            return;
        }

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        if (spriteWidth <= 0f || spriteHeight <= 0f)
        {
            Debug.LogError("BackgroundScaler: Invalid sprite dimensions.");
            enabled = false;
            return;
        }

        float worldHeight = targetCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * targetCamera.aspect;

        transform.localScale = new Vector3(worldWidth / spriteWidth, worldHeight / spriteHeight, 1f);
    }
}
