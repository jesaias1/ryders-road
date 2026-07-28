using System;
using System.Collections.Generic;

namespace Avoidance.Core.Services
{
    public sealed class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Service {type.Name} is already registered.");
            }

            _services.Add(type, service);
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var value))
            {
                service = (T)value;
                return true;
            }

            service = null;
            return false;
        }
    }

    public static class GameServices
    {
        public static ServiceContainer Current { get; private set; }

        public static void Publish(ServiceContainer services)
        {
            Current = services ?? throw new ArgumentNullException(nameof(services));
        }

        public static void Clear(ServiceContainer expected)
        {
            if (ReferenceEquals(Current, expected))
            {
                Current = null;
            }
        }
    }
}
