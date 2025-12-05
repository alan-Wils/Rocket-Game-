using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;

    [SerializeField] private int currentHealth;

    public Sprite normalSprite;
    public Sprite damagedSprite;
    public Sprite explosionSprite;

    private SpriteRenderer sr;
    private bool isExploding = false;
    private bool damaged = false;

    public float knockbackForce = 20f;
    public float knockbackTime = 0.15f;

    private Rigidbody2D rb;
    private RocketController controller;

    [Header("UI Toggles")]
    public bool useTextUI = true;
    public bool useSliderUI = true;

    [Header("UI References")]
    public TextMeshProUGUI healthText;
    public Slider healthSlider;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<RocketController>();

        currentHealth = maxHealth;
        SetSprite(normalSprite);

        FindUI();
        InitializeSlider();
        UpdateHealthUI();

        StartCoroutine(InitializeUIOnNextFrame());
    }

    private IEnumerator InitializeUIOnNextFrame()
    {
        yield return null;
        FindUI();
        InitializeSlider();
        UpdateHealthUI();
    }

    private void FindUI()
    {
        if (useTextUI && healthText == null)
        {
            GameObject t = GameObject.Find("HealthText");
            if (t != null)
            {
                healthText = t.GetComponent<TextMeshProUGUI>();
            }
        }

        if (useSliderUI && healthSlider == null)
        {
            GameObject s = GameObject.Find("HealthSlider");
            if (s != null)
            {
                healthSlider = s.GetComponent<Slider>();
            }
        }
    }

    private void InitializeSlider()
    {
        if (!useSliderUI || healthSlider == null)
        {
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;

        // Force an initial update so listeners see the starting value.
        float initialValue = maxHealth - 0.01f;
        healthSlider.value = initialValue;
        healthSlider.onValueChanged.Invoke(initialValue);

        healthSlider.value = currentHealth;
        healthSlider.onValueChanged.Invoke(currentHealth);
    }

    public void TakeDamage(int amount)
    {
        Vector2 horizontalKick = Random.value < 0.5f ? Vector2.left : Vector2.right;
        TakeDamage(amount, horizontalKick);
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        if (isExploding)
        {
            return;
        }

        StartCoroutine(DoKnockback(hitDirection));

        currentHealth -= amount;
        UpdateHealthUI();

        if (!damaged && currentHealth <= maxHealth / 2 && currentHealth > 0)
        {
            damaged = true;
            SetSprite(damagedSprite);
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(Explode());
        }
    }

    public void AddHealth(int amount)
    {
        if (isExploding)
        {
            return;
        }

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (damaged && currentHealth > maxHealth / 2)
        {
            damaged = false;
            SetSprite(normalSprite);
        }

        UpdateHealthUI();
    }

    private IEnumerator DoKnockback(Vector2 dir)
    {
        if (controller != null)
        {
            controller.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        dir.y = 0;
        dir.Normalize();
        if (rb != null)
        {
            rb.velocity = dir * knockbackForce;
        }

        yield return new WaitForSeconds(knockbackTime);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (!isExploding && controller != null)
        {
            controller.enabled = true;
        }
    }

    private IEnumerator Explode()
    {
        isExploding = true;

        if (controller != null)
        {
            controller.enabled = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        SetSprite(explosionSprite);
        transform.localScale *= 1.1f;

        yield return new WaitForSeconds(0.2f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        if (useTextUI && healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
        }

        if (useSliderUI && healthSlider != null)
        {
            healthSlider.value = currentHealth;
            healthSlider.onValueChanged.Invoke(currentHealth);
        }
    }

    private void SetSprite(Sprite s)
    {
        // Only swap the sprite; avoid instantiating a new material every time which would
        // leak materials at runtime. Toggling enabled forces a refresh without allocations.
        sr.sprite = s;
        sr.enabled = false;
        sr.enabled = true;
    }
}
