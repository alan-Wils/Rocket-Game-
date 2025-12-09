using UnityEngine;
using System.Collections.Generic;

public static class SpriteBank
{
    private static Dictionary<string, Sprite> bank;

    public static void LoadAllSprites()
    {
        bank = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("");

        foreach (Sprite s in sprites)
        {
            if (!bank.ContainsKey(s.name))
                bank.Add(s.name, s);
        }

        Debug.Log("SpriteBank loaded " + bank.Count + " sprites.");
    }

    public static Sprite Get(string name)
    {
        if (bank == null)
            LoadAllSprites();

        if (bank.TryGetValue(name, out Sprite result))
            return result;

        Debug.LogError("SpriteBank ERROR: Sprite not found: " + name);
        return null;
    }
}
