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

    public class PlayerMovedEvent : IEvent
    {
        public string Type => "PlayerMoved";
        public float X { get; }
        public float Y { get; }

        public PlayerMovedEvent(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class EnemySpawnedEvent : IEvent
    {
        public string Type => "EnemySpawned";
        public object Enemy { get; }

        public EnemySpawnedEvent(object enemy)
        {
            Enemy = enemy;
        }
    }

    public class AttackEvent : IEvent
    {
        public string Type => "Attack";
        public object Attacker { get; }
        public object Target { get; }
        public float Damage { get; }

        public AttackEvent(object attacker, object target, float damage)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
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
}