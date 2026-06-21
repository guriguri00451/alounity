using System;
using UnityEngine;
using FishRumble;

/// <summary>
/// カヤック乗りの体力を管理する。IDamageable を実装し、Fish の振り回し攻撃ダメージを受け付ける。
/// </summary>
public class KayakRiderHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    private Rigidbody rb;
    private int playerID;
    public int PlayerID
    {
        get
        {
            return playerID;
        }
        
        set 
        {
            playerID = value;
        }
    }
    private int currentHp;

    public bool IsDead => currentHp <= 0;

    /// <summary>HPが0になったときに発火するイベント。GameManager等が購読してリスポーンを呼ぶ。</summary>
    public event Action onDead;

    void Awake()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int damage, int _playerID)
    {
        if(playerID == _playerID) return;

        if (IsDead) return;
        currentHp = Mathf.Max(0, currentHp - damage);
        Debug.Log($"[KayakRider] ダメージ {damage} 受けた。残りHP: {currentHp}/{maxHp}");
        if (IsDead)
            onDead?.Invoke();
    }

    /// <summary>
    /// HPを全回復してrespawnPointの位置・向きに戻す。
    /// </summary>
    public void Respawn()
    {
        currentHp = maxHp;
    }
}
