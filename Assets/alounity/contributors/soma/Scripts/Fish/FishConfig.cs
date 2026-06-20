using System.Collections.Generic;
using FishRumble;
using UnityEngine;

/// <summary>
/// 魚の種類と出現率を一括管理するScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "FishConfig", menuName = "Fish/Fish Config")]
public class FishConfig : ScriptableObject
{
    public List<FishEntry> fishes = new();

    /// <summary>
    /// spawnWeight に基づいた重み付き抽選で魚のプレハブを返す。
    /// </summary>
    public GameObject GetFish()
    {
        int totalWeight = 0;
        foreach (var entry in fishes)
            totalWeight += entry.spawnWeight;

        int roll = Random.Range(0, totalWeight);
        foreach (var entry in fishes)
        {
            roll -= entry.spawnWeight;
            if (roll < 0)
                return entry.prefab;
        }

        return fishes[^1].prefab;
    }
}
