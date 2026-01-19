using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (handlers.TryGetValue(type, out var existing))
                handlers[type] = Delegate.Combine(existing, handler);
            else
                handlers[type] = handler;
            
            
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (!handlers.TryGetValue(type, out var existing))
                return;

            var current = Delegate.Remove(existing, handler);

            if (current == null)
                handlers.Remove(type);
            else
                handlers[type] = current;
        }

        public void Publish<T>(T evt)
        {
            var type = typeof(T);

            if (handlers.TryGetValue(type, out var del))
                ((Action<T>)del)?.Invoke(evt);
        }
    }
}