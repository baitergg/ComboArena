using Microsoft.Xna.Framework;

namespace ComboArena.Model
{
    /// <summary>
    /// Красный враг - лёгкий, быстрый, но слабый.
    /// Базовая роль: мобильный юнит для постоянного давления.
    /// Способность: Ярость - при низком HP сильно ускоряется.
    /// </summary>
    public class RedEnemy : Enemy
    {
        /// <summary>Тип врага - Red.</summary>
        public override EnemyType Type => EnemyType.Red;
        
        /// <summary>
        /// Создаёт красного врага в указанной позиции.
        /// Характеристики: HP 20, скорость 120, урон 3, кулдаун атаки 0.8с,
        /// дальность обнаружения 300, награда опытом 10.
        /// </summary>
        public RedEnemy(float x, float y)
            : base(x, y,
                width: 100,
                height: 80,
                maxHealth: 20,
                speed: 120,
                damage: 3,
                attackCooldown: 0.8f,
                detectionRange: 300,
                experienceReward: 10)
        {
        }

        /// <summary>
        /// Разблокирует способность "Ярость" в зависимости от тира:
        /// Elite - множитель скорости 1.5x,
        /// Champion - множитель 2.0x,
        /// Boss - множитель 2.5x + базовая скорость увеличена на 30%.
        /// </summary>
        protected override void UnlockAbilities()
        {
            switch (Tier)
            {
                case EnemyTier.Elite:
                    InitEnrageAbility(1.5f);
                    break;

                case EnemyTier.Champion:
                    InitEnrageAbility(2.0f);
                    break;

                case EnemyTier.Boss:
                    Speed = BaseSpeed * 1.3f;
                    InitEnrageAbility(2.5f);
                    break;
            }
        }
    }
}
