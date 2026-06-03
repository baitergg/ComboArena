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
        public float Radius { get; private set; }

        private float Damage { get; set; }

        public float AnimationTimer { get; private set; }

        private const float AnimationDuration = 0.4f;

        public FireBurst()
        {
            Type = AbilityType.FireBurst;
            Name = "Fire Burst";
            Description = "Deals AoE fire damage around the player";
            Cooldown = 5f;
            Radius = 150f;
            Damage = 25f;
        }

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

        public void UpgradeRadius(float addRadius)
        {
            Radius += addRadius;
        }

        public void UpgradeDamage(float addDamage)
        {
            Damage += addDamage;
        }

        public void UpgradeCooldown(float multiplier)
        {
            Cooldown *= multiplier;
        }
    }
}