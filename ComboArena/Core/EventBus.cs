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

        public void Clear()
        {
            _listeners.Clear();
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
        public string Type => "HealthChanged";

        public object Entity { get; }

        public float OldHealth { get; }

        public float NewHealth { get; }
        
        public HealthChangedEvent(object entity, float oldHealth, float newHealth)
        {
            Entity = entity;
            OldHealth = oldHealth;
            NewHealth = newHealth;
        }
    }
    
    public class EnemyDeathEvent : IEvent
    {
        public string Type => "EnemyDeath";

        public Enemy Enemy { get; }

        public Vector2 Position { get; }

        public float ExperienceReward { get; }

        public EnemyDeathEvent(Enemy enemy, Vector2 position, float experienceReward)
        {
            Enemy = enemy;
            Position = position;
            ExperienceReward = experienceReward;
        }
    }

    public class LevelUpEvent : IEvent
    {
        public string Type => "LevelUp";

        public int NewLevel { get; }

        public LevelUpEvent(int newLevel)
        {
            NewLevel = newLevel;
        }
    }
    
    public class ComboEvent : IEvent
    {
        public string Type => "Combo";

        public int ComboCount { get; }

        public int MaxCombo { get; }

        public float DamageMultiplier { get; }

        public float ExperienceMultiplier { get; }

        public ComboEvent(int comboCount, int maxCombo, float damageMultiplier, float experienceMultiplier)
        {
            ComboCount = comboCount;
            MaxCombo = maxCombo;
            DamageMultiplier = damageMultiplier;
            ExperienceMultiplier = experienceMultiplier;
        }
    }
    
    public class PerkSelectionEvent : IEvent
    {
        public string Type => "PerkSelection";

        public bool IsChoosing { get; }

        public List<Perk> OfferedPerks { get; }

        public PerkSelectionEvent(bool isChoosing, List<Perk> offeredPerks)
        {
            IsChoosing = isChoosing;
            OfferedPerks = offeredPerks;
        }
    }
}
