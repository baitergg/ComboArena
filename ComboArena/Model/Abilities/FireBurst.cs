using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ComboArena.Model.Abilities
{
    /// <summary>
    /// Огненный взрыв (Fire Burst) - активная способность,
    /// наносящая AoE урон всем врагам в радиусе вокруг игрока.
    /// Активируется по нажатию клавиши E.
    /// </summary>
    public class FireBurst : ActiveAbility
    {
        /// <summary>Радиус взрыва в пикселях.</summary>
        public float Radius { get; private set; }

        /// <summary>Урон, наносимый каждому врагу в радиусе.</summary>
        public float Damage { get; private set; }

        /// <summary>Таймер анимации взрыва.</summary>
        public float AnimationTimer { get; private set; }

        /// <summary>Длительность анимации взрыва в секундах.</summary>
        private const float AnimationDuration = 0.4f;

        /// <summary>
        /// Создаёт способность "Огненный взрыв" с базовыми параметрами:
        /// кулдаун 5 сек, радиус 150 пикселей, урон 25.
        /// </summary>
        public FireBurst()
        {
            Type = AbilityType.FireBurst;
            Name = "Fire Burst";
            Description = "Deals AoE fire damage around the player";
            Cooldown = 5f;
            Radius = 150f;
            Damage = 25f;
        }

        /// <summary>
        /// Активирует огненный взрыв: наносит урон всем врагам в радиусе
        /// и запускает анимацию.
        /// </summary>
        public override void Activate(Player player, List<Enemy> enemies)
        {
            if (!IsReady) return;
            base.Activate(player, enemies);

            AnimationTimer = AnimationDuration;

            // Наносим урон всем врагам в радиусе
            var playerCenter = player.Position + new Vector2(player.Width / 2, player.Height / 2);

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                var enemyCenter = enemy.Position + new Vector2(enemy.Width / 2, enemy.Height / 2);
                var distance = Vector2.Distance(playerCenter, enemyCenter);

                if (distance <= Radius)
                {
                    enemy.TakeDamage(Damage);
                }
            }
        }

        /// <summary>
        /// Обновляет состояние взрыва каждый кадр:
        /// уменьшает таймер анимации и деактивирует способность по завершении.
        /// </summary>
        public override void Update(float delta, Player player, List<Enemy> enemies)
        {
            base.Update(delta, player, enemies);

            if (AnimationTimer > 0)
            {
                AnimationTimer -= delta;
                if (AnimationTimer <= 0)
                {
                    IsActive = false;
                }
            }
        }

        /// <summary>
        /// Увеличивает радиус взрыва на указанное значение.
        /// </summary>
        public void UpgradeRadius(float addRadius)
        {
            Radius += addRadius;
        }

        /// <summary>
        /// Увеличивает урон взрыва на указанное значение.
        /// </summary>
        public void UpgradeDamage(float addDamage)
        {
            Damage += addDamage;
        }

        /// <summary>
        /// Уменьшает кулдаун взрыва, умножая его на множитель.
        /// </summary>
        public void UpgradeCooldown(float multiplier)
        {
            Cooldown *= multiplier;
        }
    }
}