using System;

namespace Avoidance.Core.Events
{
    public sealed class GameEvent
    {
        private event Action _listeners;

        public void Subscribe(Action listener) => _listeners += listener;
        public void Unsubscribe(Action listener) => _listeners -= listener;
        public void Raise() => _listeners?.Invoke();
    }
}
