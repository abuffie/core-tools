using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aarware.Services {
    /// <summary>
    /// Central service locator/registry for accessing services throughout the application.
    /// </summary>
    public static class ServiceManager {
        static readonly Dictionary<Type, IService> services = new Dictionary<Type, IService>();

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        public static void RegisterService<T>(T service) where T : IService {
            Type serviceType = typeof(T);

            if (services.ContainsKey(serviceType)) {
                Debug.LogWarning($"[ServiceManager] Service {serviceType.Name} already registered. Replacing with new instance.");
                services[serviceType] = service;
            } else {
                services.Add(serviceType, service);
                Debug.Log($"[ServiceManager] Service {serviceType.Name} registered.");
            }
        }

        /// <summary>
        /// Gets a registered service instance.
        /// </summary>
        public static T GetService<T>() where T : IService {
            Type serviceType = typeof(T);

            if (services.TryGetValue(serviceType, out IService service)) {
                return (T)service;
            }

            Debug.LogError($"[ServiceManager] Service {serviceType.Name} not found. Did you register it?");
            return default;
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        public static bool HasService<T>() where T : IService {
            return services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregisters a service.
        /// </summary>
        public static void UnregisterService<T>() where T : IService {
            Type serviceType = typeof(T);

            if (services.Remove(serviceType)) {
                Debug.Log($"[ServiceManager] Service {serviceType.Name} unregistered.");
            }
        }

        /// <summary>
        /// Shuts down all services and clears the registry.
        /// </summary>
        public static void ShutdownAll() {
            foreach (var service in services.Values) {
                service?.Shutdown();
            }
            services.Clear();
            Debug.Log("[ServiceManager] All services shutdown.");
        }
    }
}
