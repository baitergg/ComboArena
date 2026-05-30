using Microsoft.Xna.Framework;

namespace ComboArena.Model
{
    /// <summary>
    /// Синий враг - средний по характеристикам.
    /// Выносливее красного, но медленнее.
    /// Базовая роль: сталкер, который может резко сокращать дистанцию.
    /// Способность: Рывок - резкое ускорение в сторону игрока.
    /// </summary>
    public class BlueEnemy : Enemy
    {
        /// <summary>Тип врага - Blue.</summary>
        public override EnemyType Type => EnemyType.Blue;

        /// <summary>
        /// Создаёт синего врага в указанной позиции.
        /// Характеристики: HP 40, скорость 90, урон 6, кулдаун атаки 1.2с,
        /// дальность обнаружения 400, награда опытом 20.
        /// </summary>
        public BlueEnemy(float x, float y)
            : base(x, y,
                width: 100,
                height: 80,
                maxHealth: 40,
                speed: 90,
                damage: 6,
                attackCooldown: 1.2f,
                detectionRange: 400,
                experienceReward: 20)
        {
        }

        /// <summary>
        /// Разблокирует способность "Рывок" в зависимости от тира:
        /// Elite - кулдаун 4с, скорость 2.5x от базовой,
        /// Champion - кулдаун 3с, скорость 3.5x,
        /// Boss - кулдаун 2с, скорость 4.5x + базовая скорость увеличена на 20%.
        /// </summary>
        protected override void UnlockAbilities()
        {
            switch (Tier)
            {
                case EnemyTier.Elite:
                    InitRushAbility(cooldown: 4f, speed: BaseSpeed * 2.5f);
                    break;

                case EnemyTier.Champion:
                    InitRushAbility(cooldown: 3f, speed: BaseSpeed * 3.5f);
                    break;

                case EnemyTier.Boss:
                    Speed = BaseSpeed * 1.2f;
                    InitRushAbility(cooldown: 2f, speed: BaseSpeed * 4.5f);
                    break;
            }
        }
    }
}
