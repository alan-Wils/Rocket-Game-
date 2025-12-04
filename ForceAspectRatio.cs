using UnityEngine;

public class ForceAspectRatio : MonoBehaviour
{
    // Set your target aspect ratio here
    public float targetWidth = 20f;
    public float targetHeight = 25f;

    [Header("Letterbox")]
    public Color letterboxColor = Color.black;
    public bool createBackgroundCamera = true;

    private const string BackgroundCameraName = "LetterboxBackgroundCamera";

    void Start()
    {
        ApplyAspect();
        EnsureLetterboxBlackBars();
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

    private void EnsureLetterboxBlackBars()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        cam.backgroundColor = letterboxColor;
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (!createBackgroundCamera)
            return;

        Camera existing = GameObject.Find(BackgroundCameraName)?.GetComponent<Camera>();
        if (existing != null)
            return;

        GameObject bgCamObj = new GameObject(BackgroundCameraName);
        Camera bgCam = bgCamObj.AddComponent<Camera>();

        bgCam.depth = cam.depth - 1f;
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = letterboxColor;
        bgCam.cullingMask = 0;
        bgCam.rect = new Rect(0f, 0f, 1f, 1f);
        bgCam.orthographic = cam.orthographic;
        bgCam.orthographicSize = cam.orthographicSize;
        bgCam.nearClipPlane = cam.nearClipPlane;
        bgCam.farClipPlane = cam.farClipPlane;
    }
}
