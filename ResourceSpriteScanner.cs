using UnityEngine;

public class ResourceSpriteScanner : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== RESOURCE SPRITE SCAN START ===");

        // Load ALL sprites in the entire Resources folder
        Sprite[] sprites = Resources.LoadAll<Sprite>("");

        if (sprites.Length == 0)
        {
            Debug.LogError("No sprites found in Resources. Check folder path: Assets/Resources/");
            return;
        }

        foreach (Sprite s in sprites)
        {
            Debug.Log("FOUND SPRITE: " + s.name);
        }

        Debug.Log("=== RESOURCE SPRITE SCAN END ===");
    }
}
