using UnityEngine;
using FishRumble;

/// <summary>
/// カヤック乗りの体力を管理する。IDamageable を実装し、Fish の振り回し攻撃ダメージを受け付ける。
/// </summary>
public class KayakRiderHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    private int currentHp;

    public bool IsDead => currentHp <= 0;

    void Awake() => currentHp = maxHp;

    public void TakeDamage(int damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        Debug.Log($"[KayakRider] ダメージ {damage} 受けた。残りHP: {currentHp}/{maxHp}");
    }
}
