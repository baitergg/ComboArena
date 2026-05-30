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
        /// <summary>Длительность действия щита в секундах.</summary>
        public float Duration { get; private set; }

        /// <summary>Таймер оставшегося времени действия щита.</summary>
        public float Timer { get; private set; }

        /// <summary>true, если щит в настоящий момент активен.</summary>
        public bool IsBarrierActive => IsActive;

        /// <summary>Радиус отталкивания врагов в пикселях.</summary>
        public float PushRadius { get; }

        /// <summary>Сила отталкивания врагов.</summary>
        public float PushForce { get; }
        
        /// <summary>
        /// Создаёт способность "Щит" с базовыми параметрами:
        /// кулдаун 8 сек, длительность 2.5 сек, радиус толчка 130 пикселей.
        /// </summary>
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
        
        /// <summary>
        /// Активирует щит: делает игрока неуязвимым и мгновенно отталкивает врагов.
        /// </summary>
        public override void Activate(Player player, List<Enemy> enemies)
        {
            if (!IsReady) return;
            base.Activate(player, enemies);

            Timer = Duration;
            player.IsInvulnerable = true;

            // Мгновенный толчок при активации
            PushEnemiesAway(player, enemies);
        }

        /// <summary>
        /// Обновляет состояние щита каждый кадр:
        /// уменьшает таймер, постоянно отталкивает врагов внутри радиуса,
        /// деактивирует щит по истечении времени.
        /// </summary>
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
        
        /// <summary>
        /// Отталкивает всех врагов в радиусе PushRadius от игрока.
        /// Сила толчка уменьшается к краю радиуса.
        /// </summary>
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
        
        /// <summary>
        /// Увеличивает длительность щита на указанное количество секунд.
        /// </summary>
        public void UpgradeDuration(float addSeconds)
        {
            Duration += addSeconds;
        }

        /// <summary>
        /// Уменьшает кулдаун щита, умножая его на множитель.
        /// </summary>
        public void UpgradeCooldown(float multiplier)
        {
            Cooldown *= multiplier;
        }
    }
}