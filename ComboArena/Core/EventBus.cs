using ComboArena.Model;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ComboArena.Core
{
    public interface IEvent
    {
        string Type { get; }
    }

    public class EventBus
    {
        private static EventBus _instance;

        public static EventBus Instance => _instance ??= new EventBus();

        private readonly Dictionary<string, List<Action<IEvent>>> _listeners;

        private EventBus()
        {
            _listeners = new Dictionary<string, List<Action<IEvent>>>();
        }

        public void Subscribe(string eventType, Action<IEvent> listener)
        {
            if (!_listeners.TryGetValue(eventType, out var value))
            {
                value = [];
                _listeners[eventType] = value;
            }
            value.Add(listener);
        }

        public void Unsubscribe(string eventType, Action<IEvent> listener)
        {
            if (_listeners.TryGetValue(eventType, out var subscriber))
            {
                subscriber.Remove(listener);
            }
        }

        public void Publish(IEvent evt)
        {
            var eventType = evt.Type;
            if (!_listeners.TryGetValue(eventType, out var subscriber)) return;

            var handlers = new List<Action<IEvent>>(subscriber);
            foreach (var handler in handlers)
                handler?.Invoke(evt);
        }
    }

    public class HealthChangedEvent : IEvent
    {
        /// <summary>Тип события: "HealthChanged".</summary>
        public string Type => "HealthChanged";

        /// <summary>Сущность, у которой изменилось здоровье.</summary>
        public object Entity { get; }

        /// <summary>Значение здоровья до изменения.</summary>
        public float OldHealth { get; }

        /// <summary>Значение здоровья после изменения.</summary>
        public float NewHealth { get; }
        
        public HealthChangedEvent(object entity, float oldHealth, float newHealth)
        {
            Entity = entity;
            OldHealth = oldHealth;
            NewHealth = newHealth;
        }
    }

    /// <summary>
    /// Событие смерти врага. Публикуется, когда здоровье врага падает до 0.
    /// </summary>
    public class EnemyDeathEvent : IEvent
    {
        /// <summary>Тип события: "EnemyDeath".</summary>
        public string Type => "EnemyDeath";

        /// <summary>Умерший враг.</summary>
        public Enemy Enemy { get; }

        /// <summary>Позиция, где умер враг.</summary>
        public Vector2 Position { get; }

        /// <summary>Базовая награда опытом врага.</summary>
        public float ExperienceReward { get; }

        public EnemyDeathEvent(Enemy enemy, Vector2 position, float experienceReward)
        {
            Enemy = enemy;
            Position = position;
            ExperienceReward = experienceReward;
        }
    }

    /// <summary>
    /// Событие повышения уровня игрока.
    /// </summary>
    public class LevelUpEvent : IEvent
    {
        /// <summary>Тип события: "LevelUp".</summary>
        public string Type => "LevelUp";

        /// <summary>Новый уровень игрока.</summary>
        public int NewLevel { get; }

        public LevelUpEvent(int newLevel)
        {
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// Событие изменения комбо-счётчика игрока.
    /// Публикуется при увеличении комбо или сбросе.
    /// </summary>
    public class ComboEvent : IEvent
    {
        /// <summary>Тип события: "Combo".</summary>
        public string Type => "Combo";

        /// <summary>Текущее количество комбо-ударов подряд.</summary>
        public int ComboCount { get; }

        /// <summary>Максимальное достигнутое комбо за сессию.</summary>
        public int MaxCombo { get; }

        /// <summary>Множитель урона от комбо.</summary>
        public float DamageMultiplier { get; }

        /// <summary>Множитель опыта от комбо.</summary>
        public float ExperienceMultiplier { get; }

        public ComboEvent(int comboCount, int maxCombo, float damageMultiplier, float experienceMultiplier)
        {
            ComboCount = comboCount;
            MaxCombo = maxCombo;
            DamageMultiplier = damageMultiplier;
            ExperienceMultiplier = experienceMultiplier;
        }
    }

    /// <summary>
    /// Событие начала/окончания выбора перка.
    /// Публикуется при открытии экрана выбора перка или при выборе перка.
    /// </summary>
    public class PerkSelectionEvent : IEvent
    {
        /// <summary>Тип события: "PerkSelection".</summary>
        public string Type => "PerkSelection";

        /// <summary>true, если игрок выбирает перк; false — выбор завершён.</summary>
        public bool IsChoosing { get; }

        /// <summary>Список предлагаемых перков (актуален только при IsChoosing == true).</summary>
        public List<Perk> OfferedPerks { get; }

        public PerkSelectionEvent(bool isChoosing, List<Perk> offeredPerks)
        {
            IsChoosing = isChoosing;
            OfferedPerks = offeredPerks;
        }
    }
}
