using System;
using UnityEngine;

namespace FishRumble
{
    /// <summary>
    /// 釣れた魚を表すクラス。耐久値の管理と、Swinging 中の振り回し攻撃判定を担う。
    /// </summary>
    public class Fish : MonoBehaviour
    {
        // Swinging 中に相手プレイヤーへ与えるダメージ量
        [SerializeField] private int swingAttackDamage = 10;

        [SerializeField] private int maxDurability = 10;
        private int currentDurability;

        // Swinging 状態のときだけ攻撃判定を有効にするフラグ
        private bool isAttackActive;

        /// <summary>攻撃がヒットしたときに発火するイベント。引数はダメージを受けたターゲット。</summary>
        public event Action<IDamageable> onAttackHit;

        public bool IsDepleted => currentDurability <= 0;

        /// <summary>
        /// 耐久値を最大値にリセットする。魚がかかった瞬間に呼ぶ。
        /// </summary>
        public void Initialize() => currentDurability = maxDurability;

        /// <summary>
        /// 耐久値を減らす。Swinging 中の一振りごとに呼ぶ。
        /// </summary>
        public void TakeDamage(int damage = 1)
            => currentDurability = Mathf.Max(0, currentDurability - damage);

        /// <summary>
        /// 振り回し攻撃判定のON/OFFを切り替える。FisherController が状態遷移時に呼ぶ。
        /// </summary>
        public void SetAttackActive(bool value) => isAttackActive = value;

        // 魚のコライダーが相手プレイヤーに衝突したときにダメージを与える。
        // ※ 魚の GameObject に KayakRider タグを持つ IDamageable コンポーネントが必要。
        private void OnCollisionEnter(Collision collision)
        {
            if (!isAttackActive) return;
            if (!collision.gameObject.CompareTag("KayakRider")) return;

            if (collision.gameObject.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(swingAttackDamage);
                onAttackHit?.Invoke(target);
            }
        }
    }
}
