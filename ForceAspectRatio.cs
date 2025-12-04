using UnityEngine;

public class ForceAspectRatio : MonoBehaviour
{
    // Set your target aspect ratio here
    public float targetWidth = 20f;
    public float targetHeight = 25f;

    void Start()
    {
        ApplyAspect();
    }

    void ApplyAspect()
    {
        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        float scaleHeight = windowAspect / targetAspect;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("ForceAspectRatio: No Main Camera found in the scene.");
            return;
        }

        // If the window is too tall (letterbox)
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1.0f - scaleHeight) * 0.5f;

            cam.rect = rect;
        }
        else // If the window is too wide (pillarbox)
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) * 0.5f;
            rect.y = 0f;

            cam.rect = rect;
        }
    }
}
