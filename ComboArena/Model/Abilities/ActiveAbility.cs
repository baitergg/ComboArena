using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ComboArena.Model.Abilities
{
    /// <summary>
    /// Тип активной способности игрока.
    /// </summary>
    public enum AbilityType
    {
        /// <summary>Огненный взрыв - AoE урон вокруг игрока.</summary>
        FireBurst,

        /// <summary>Щит - временная неуязвимость + отталкивание врагов.</summary>
        Barrier
    }

    /// <summary>
    /// Базовый класс для активных способностей игрока.
    /// Активируются по нажатию клавиши E.
    /// Содержит кулдаун, состояние готовности и базовую логику активации/обновления.
    /// </summary>
    public abstract class ActiveAbility
    {
        /// <summary>Название способности (например, "Fire Burst").</summary>
        public string Name { get; protected set; }

        /// <summary>Описание способности.</summary>
        public string Description { get; protected set; }

        /// <summary>Тип способности (FireBurst или Barrier).</summary>
        public AbilityType Type { get; protected set; }

        /// <summary>Полный кулдаун способности в секундах.</summary>
        public float Cooldown { get; protected set; }

        /// <summary>Текущий оставшийся кулдаун в секундах.</summary>
        public float CurrentCooldown { get; private set; }

        /// <summary>true, если способность готова к использованию (кулдаун прошёл).</summary>
        public bool IsReady => CurrentCooldown <= 0;

        /// <summary>true, если способность в настоящий момент активна (действует).</summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Инициализирует способность с нулевым кулдауном (готова к использованию).
        /// </summary>
        protected ActiveAbility()
        {
            CurrentCooldown = 0;
        }

        /// <summary>
        /// Активирует способность. Если способность не готова (кулдаун не прошёл) - ничего не делает.
        /// Устанавливает кулдаун и флаг IsActive.
        /// </summary>
        public virtual void Activate(Player player, List<Enemy> enemies)
        {
            if (!IsReady) return;
            IsActive = true;
            CurrentCooldown = Cooldown;
        }

        /// <summary>
        /// Обновляет состояние способности каждый кадр.
        /// Уменьшает текущий кулдаун.
        /// </summary>
        public virtual void Update(float delta, Player player, List<Enemy> enemies)
        {
            if (CurrentCooldown > 0)
                CurrentCooldown -= delta;
        }

        /// <summary>
        /// Возвращает прогресс кулдауна от 0 до 1, где 1 = способность готова.
        /// </summary>
        public float GetCooldownProgress()
        {
            return 1f - (CurrentCooldown / Cooldown);
        }
    }
}