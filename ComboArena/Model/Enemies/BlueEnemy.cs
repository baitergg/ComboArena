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
        public override EnemyType Type => EnemyType.Blue;
        
        public BlueEnemy(float x, float y)
            : base(x, y,
                width: 143,
                height: 80,
                maxHealth: 40,
                speed: 90,
                damage: 6,
                attackCooldown: 1.2f,
                detectionRange: 400,
                experienceReward: 20)
        {
        }
        
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
