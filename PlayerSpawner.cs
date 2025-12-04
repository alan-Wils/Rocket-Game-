using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;   // assign your Player prefab here
    public Vector2 startPosition = new Vector2(0, -4);  // default starting point

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner: No player prefab assigned!");
            return;
        }

        Instantiate(playerPrefab, startPosition, Quaternion.identity);
    }
}
