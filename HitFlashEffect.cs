using UnityEngine;

public class HitFlashEffect : MonoBehaviour
{
    public float lifetime = 0.15f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
