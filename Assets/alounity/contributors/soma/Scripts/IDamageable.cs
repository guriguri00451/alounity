namespace FishRumble
{
    /// <summary>
    /// ダメージを受けられるオブジェクトが実装するインターフェース。
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}
