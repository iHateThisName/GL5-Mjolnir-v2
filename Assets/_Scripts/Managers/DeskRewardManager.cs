using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DeskRewardManager : Singleton<DeskRewardManager>
{
    [Tooltip("Drag up to 5 empty GameObjects positioned around the player's desk here.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Audio")]
    [Tooltip("The sound effect to play when a reward pops onto the desk.")]
    [SerializeField] private AudioClip popSoundEffect;
    [SerializeField, Range(0f, 1f)] private float popVolume = 0.8f;

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

        // Spawn DIRECTLY as a child of the target point.
        GameObject newReward = Instantiate(prefab, targetPoint);

        newReward.transform.localPosition = Vector3.zero;
        newReward.transform.localRotation = Quaternion.identity;

        Vector3 targetScale = newReward.transform.localScale;
        newReward.transform.localScale = Vector3.zero;

        // Play the pop sound effect using the AudioManager pool
        if (AudioManager.Instance != null && this.popSoundEffect != null)
        {
            AudioManager.Instance.PlaySFX(this.popSoundEffect, this.popVolume);
        }

        newReward.transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        this.currentSpawnIndex++;

        Debug.Log($"Spawned reward at desk position {this.currentSpawnIndex} / {this.spawnPoints.Count}");
    }

    public void ClearAllRewards()
    {
        this.currentSpawnIndex = 0;
        foreach (Transform point in this.spawnPoints)
        {
            foreach (Transform child in point)
            {
                child.DOKill();
                Destroy(child.gameObject);
            }
        }
    }
}