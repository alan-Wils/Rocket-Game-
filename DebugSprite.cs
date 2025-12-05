using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DebugSprite : MonoBehaviour
{
    private SpriteRenderer sr;
    private Sprite lastSprite;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("DebugSprite requires a SpriteRenderer on the same GameObject.");
            enabled = false;
            return;
        }

        lastSprite = sr.sprite;
    }

    void Update()
    {
        if (sr.sprite != lastSprite)
        {
            if (sr.sprite == null)
                Debug.Log("SPRITE BECAME NULL THIS FRAME!");
            else
                Debug.Log("SPRITE CHANGED TO: " + sr.sprite.name);

            lastSprite = sr.sprite;
        }
    }
}
