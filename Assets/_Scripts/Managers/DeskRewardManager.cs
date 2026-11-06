using System.Collections.Generic;
using UnityEngine;

public class DeskRewardManager : Singleton<DeskRewardManager>
{
    [Tooltip("Drag up to 5 empty GameObjects positioned around the player's desk here.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private int currentSpawnIndex = 0;

    public void SpawnReward(GameObject prefab)
    {
        if (prefab == null || this.spawnPoints.Count == 0) return;

        // Loop back to the first spot if the desk is completely full (overwrites oldest item)
        if (this.currentSpawnIndex >= this.spawnPoints.Count)
        {
            this.currentSpawnIndex = 0;
        }

        Transform targetPoint = this.spawnPoints[this.currentSpawnIndex];

        // Destroy whatever is currently sitting on this specific spawn point
        foreach (Transform child in targetPoint)
        {
            Destroy(child.gameObject);
        }

        // Spawn the new item and parent it to the spawn point
        GameObject newReward = Instantiate(prefab, targetPoint.position, targetPoint.rotation);
        newReward.transform.SetParent(targetPoint);

        this.currentSpawnIndex++;

        Debug.Log($"Spawned reward at desk position {this.currentSpawnIndex} / {this.spawnPoints.Count}");
    }

    // You can call this from your GameManager or DayManager at the start of a new work week/level!
    public void ClearAllRewards()
    {
        this.currentSpawnIndex = 0;
        foreach (Transform point in this.spawnPoints)
        {
            foreach (Transform child in point)
            {
                Destroy(child.gameObject);
            }
        }
    }
}