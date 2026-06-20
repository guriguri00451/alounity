using System;
using FishRumble;
using UnityEngine;

/// <summary>
/// 魚1種類分のプレハブと出現重みをまとめたデータクラス。
/// </summary>
[Serializable]
public class FishEntry
{
    public GameObject prefab;
    public int spawnWeight = 1;
}
