using Microsoft.Xna.Framework;
using System;

namespace ComboArena.Model
{
    /// <summary>
    /// Жёлтый враг - тяжёлый, медленный, но с большим
    /// запасом здоровья и высоким уроном.
    /// Базовая роль: поддержка на дистанции.
    /// Способность: Лазер - стреляет лучом, наносящим урон на расстоянии.
    /// </summary>
    public class YellowEnemy : Enemy
    {
        /// <summary>Тип врага - Yellow.</summary>
        public override EnemyType Type => EnemyType.Yellow;
        
        /// <summary>
        /// Создаёт жёлтого врага в указанной позиции.
        /// Характеристики: HP 60, скорость 60, урон 10, кулдаун атаки 1.8с,
        /// дальность обнаружения 500, награда опытом 35.
        /// </summary>
        public YellowEnemy(float x, float y)
            : base(x, y,
                width: 100,
                height: 80,
                maxHealth: 60,
                speed: 60,
                damage: 10,
                attackCooldown: 1.8f,
                detectionRange: 500,
                experienceReward: 35)
        {
        }

        /// <summary>
        /// Разблокирует способность "Лазер" в зависимости от тира:
        /// Elite - урон 5, дальность 300,
        /// Champion - урон 10, дальность 400,
        /// Boss - урон 15, дальность 500.
        /// </summary>
        protected override void UnlockAbilities()
        {
            switch (Tier)
            {
                case EnemyTier.Elite:
                    InitLaserAbility(damage: 5, range: 300f);
                    break;

                case EnemyTier.Champion:
                    InitLaserAbility(damage: 10, range: 400f);
                    break;

                case EnemyTier.Boss:
                    InitLaserAbility(damage: 15, range: 500f);
                    break;
            }
        }
    }
}
