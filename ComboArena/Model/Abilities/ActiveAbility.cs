using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ComboArena.Model.Abilities
{
    /// <summary>
    /// Тип активной способности игрока.
    /// </summary>
    public enum AbilityType
    {
        FireBurst,

        Barrier
    }

    /// <summary>
    /// Базовый класс для активных способностей игрока.
    /// Активируются по нажатию клавиши E.
    /// Содержит КД, состояние готовности и базовую логику активации/обновления.
    /// </summary>
    public abstract class ActiveAbility
    {
        public string Name { get; protected set; }

        public string Description { get; protected set; }

        public AbilityType Type { get; protected set; }

        public float Cooldown { get; protected set; }

        public float CurrentCooldown { get; private set; }

        public bool IsReady => CurrentCooldown <= 0;

        public bool IsActive { get; protected set; }

        protected ActiveAbility()
        {
            CurrentCooldown = 0;
        }

        public virtual void Activate(Player player, List<Enemy> enemies)
        {
            if (!IsReady) return;
            IsActive = true;
            CurrentCooldown = Cooldown;
        }

        public virtual void Update(float delta, Player player, List<Enemy> enemies)
        {
            if (CurrentCooldown > 0)
                CurrentCooldown -= delta;
        }

        public float GetCooldownProgress()
        {
            return 1f - (CurrentCooldown / Cooldown);
        }
    }
}