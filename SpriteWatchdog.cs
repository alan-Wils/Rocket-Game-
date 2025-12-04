using UnityEngine;

public class SpriteWatchdog : MonoBehaviour
{
    private SpriteRenderer sr;
    private Sprite lastSprite;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sr.sprite != lastSprite)
        {
            Debug.LogError("SPRITE CHANGE DETECTED! New sprite = " + (sr.sprite ? sr.sprite.name : "NULL"));
            lastSprite = sr.sprite;
        }
    }
}
