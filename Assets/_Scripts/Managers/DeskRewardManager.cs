using DG.Tweening;
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

        if (this.currentSpawnIndex >= this.spawnPoints.Count)
        {
            this.currentSpawnIndex = 0;
        }

        Transform targetPoint = this.spawnPoints[this.currentSpawnIndex];

        foreach (Transform child in targetPoint)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }

        // Spawn DIRECTLY as a child of the target point. This prevents weird scale recalculations.
        GameObject newReward = Instantiate(prefab, targetPoint);

        // Force the position/rotation to be perfectly zeroed out on the spawn slot
        newReward.transform.localPosition = Vector3.zero;
        newReward.transform.localRotation = Quaternion.identity;

        // Grab the scale, crush it, and tween it
        Vector3 targetScale = newReward.transform.localScale;
        newReward.transform.localScale = Vector3.zero;

        // .SetUpdate(true) forces the animation to play even if the game is paused!
        newReward.transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

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