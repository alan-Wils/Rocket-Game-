using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    // ----------------------------------
    // POINTS
    // ----------------------------------
    public int points = 0;
    public TextMeshProUGUI pointsText;

    // ----------------------------------
    // RUBIES
    // ----------------------------------
    public int rubies = 0;                    // NEW: ruby counter
    public TextMeshProUGUI rubyText;          // NEW: optional TMP box for rubies

    void Start()
    {
        // Auto-find Points text
        if (pointsText == null)
        {
            GameObject ptsObj = GameObject.Find("PointText");
            if (ptsObj != null)
                pointsText = ptsObj.GetComponent<TextMeshProUGUI>();
        }

        // Auto-find Ruby text (optional — only if you name it)
        if (rubyText == null)
        {
            GameObject rObj = GameObject.Find("RubyText");
            if (rObj != null)
                rubyText = rObj.GetComponent<TextMeshProUGUI>();
        }

        UpdateUI();
    }

    // ----------------------------------
    // POINT METHODS
    // ----------------------------------
    public void AddPoints(int amount)
    {
        points += amount;
        UpdateUI();
    }

    public bool SpendPoints(int amount)
    {
        if (points >= amount)
        {
            points -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    // ----------------------------------
    // RUBY METHODS (NEW)
    // ----------------------------------
    public void AddRubies(int amount)
    {
        rubies += amount;
        UpdateUI();
        Debug.Log("Rubies: " + rubies);
    }

    public bool SpendRubies(int amount)
    {
        if (rubies >= amount)
        {
            rubies -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    // ----------------------------------
    // UI UPDATE
    // ----------------------------------
    private void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = "Points: " + points;

        if (rubyText != null)
            rubyText.text = "Rubies: " + rubies;
    }
}
