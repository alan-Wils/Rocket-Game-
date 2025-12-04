using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceAspectRatio : MonoBehaviour
{
    // Set your target aspect ratio here
    public float targetWidth = 20f;
    public float targetHeight = 25f;

    [Header("Letterbox")]
    public Color letterboxColor = Color.black;
    public bool createBackgroundCamera = true;

    private const string BackgroundCameraName = "LetterboxBackgroundCamera";

    private static ForceAspectRatio instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSingletonExists()
    {
        if (instance != null)
            return;

        ForceAspectRatio existing = FindFirstObjectByType<ForceAspectRatio>();
        if (existing != null)
        {
            instance = existing;
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject(nameof(ForceAspectRatio));
        instance = go.AddComponent<ForceAspectRatio>();
    }

    void Start()
    {
        letterboxColor = Color.black;
        ApplyAspect();
        EnsureLetterboxBlackBars();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-apply after scene load so every scene gets the forced aspect ratio
        StartCoroutine(ApplyOnNextFrame());
    }

    private System.Collections.IEnumerator ApplyOnNextFrame()
    {
        yield return null;
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

        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (!createBackgroundCamera)
            return;

        Camera existing = GameObject.Find(BackgroundCameraName)?.GetComponent<Camera>();
        if (existing != null)
        {
            ConfigureBackgroundCamera(existing, cam);
            return;
        }

        GameObject bgCamObj = new GameObject(BackgroundCameraName);
        Camera bgCam = bgCamObj.AddComponent<Camera>();

        ConfigureBackgroundCamera(bgCam, cam);
    }

    private void ConfigureBackgroundCamera(Camera bgCam, Camera referenceCam)
    {
        bgCam.depth = referenceCam.depth - 1f;
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask = 0;
        bgCam.rect = new Rect(0f, 0f, 1f, 1f);
        bgCam.orthographic = referenceCam.orthographic;
        bgCam.orthographicSize = referenceCam.orthographicSize;
        bgCam.nearClipPlane = referenceCam.nearClipPlane;
        bgCam.farClipPlane = referenceCam.farClipPlane;
    }
}
