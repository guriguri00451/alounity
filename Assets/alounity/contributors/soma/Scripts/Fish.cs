using UnityEngine;

namespace FishRumble
{
    /// <summary>
    /// 釣れた魚を表すクラス。耐久値を管理し、Swinging 状態でのダメージを受け付ける。
    /// </summary>
    public class Fish : MonoBehaviour
    {
        [SerializeField] private int maxDurability = 10;
        private int currentDurability;

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
    }
}
