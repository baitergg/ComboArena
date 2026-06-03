using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ComboArena.Model.Abilities
{
    /// <summary>
    /// Щит (Barrier) - активная способность, дающая временную неуязвимость
    /// и отталкивающая врагов в радиусе действия.
    /// Активируется по нажатию клавиши E.
    /// </summary>
    public class Barrier : ActiveAbility
    {
        public float Duration { get; private set; }

        public float Timer { get; private set; }

        public bool IsBarrierActive => IsActive;

        public float PushRadius { get; }

        public float PushForce { get; }
        
        public Barrier()
        {
            Type = AbilityType.Barrier;
            Name = "Barrier";
            Description = "Become invulnerable and push enemies away";
            Cooldown = 8f;
            Duration = 2.5f;
            PushRadius = 130f;
            PushForce = 300f;
        }
        
        public override void Activate(Player player, List<Enemy> enemies)
        {
            if (!IsReady) return;
            base.Activate(player, enemies);

            Timer = Duration;
            player.IsInvulnerable = true;

            // Мгновенный толчок при активации
            PushEnemiesAway(player, enemies);
        }

        public override void Update(float delta, Player player, List<Enemy> enemies)
        {
            base.Update(delta, player, enemies);

            if (IsActive)
            {
                Timer -= delta;

                // Постоянное отталкивание врагов внутри радиуса
                PushEnemiesAway(player, enemies);

                if (Timer <= 0)
                {
                    IsActive = false;
                    player.IsInvulnerable = false;
                }
            }
        }
        
        private void PushEnemiesAway(Player player, List<Enemy> enemies)
        {
            var playerCenter = player.Position + new Vector2(player.Width / 2, player.Height / 2);

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                var enemyCenter = enemy.Position + new Vector2(enemy.Width / 2, enemy.Height / 2);
                var direction = enemyCenter - playerCenter;
                var distance = direction.Length();

                if (distance < PushRadius && distance > 1f)
                {
                    direction.Normalize();
                    // Сила толчка уменьшается к краю радиуса
                    var force = PushForce * (1f - distance / PushRadius);
                    enemy.MoveBy(direction * force);
                }
            }
        }
        
        public void UpgradeDuration(float addSeconds)
        {
            Duration += addSeconds;
        }

        public void UpgradeCooldown(float multiplier)
        {
            Cooldown *= multiplier;
        }
    }
}