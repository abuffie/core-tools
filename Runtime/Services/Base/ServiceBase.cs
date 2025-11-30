using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services {
    /// <summary>
    /// Abstract base class for services with provider support.
    /// </summary>
    public abstract class ServiceBase<TProvider> : IService where TProvider : IServiceProvider {
        protected TProvider currentProvider;

        public bool IsInitialized { get; protected set; }
        public event Action<bool> OnInitializationChanged;

        protected ServiceBase() {
            IsInitialized = false;
        }

        /// <summary>
        /// Sets the provider for this service.
        /// </summary>
        public virtual void SetProvider(TProvider provider) {
            if (IsInitialized) {
                Debug.LogWarning($"[{GetType().Name}] Attempting to set provider while service is initialized. Shutdown first.");
                return;
            }
            currentProvider = provider;
        }

        /// <summary>
        /// Gets the current provider.
        /// </summary>
        public TProvider GetProvider() {
            return currentProvider;
        }

        public virtual async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                Debug.LogWarning($"[{GetType().Name}] Service already initialized.");
                return true;
            }

            if (currentProvider == null) {
                Debug.LogError($"[{GetType().Name}] No provider set. Cannot initialize service.");
                return false;
            }

            bool success = await currentProvider.InitializeAsync();
            if (success) {
                IsInitialized = true;
                OnInitializationChanged?.Invoke(true);
                Debug.Log($"[{GetType().Name}] Initialized successfully with provider: {currentProvider.GetType().Name}");
            } else {
                Debug.LogError($"[{GetType().Name}] Failed to initialize provider.");
            }

            return success;
        }

        public virtual void Shutdown() {
            if (!IsInitialized) {
                return;
            }

            currentProvider?.Shutdown();
            IsInitialized = false;
            OnInitializationChanged?.Invoke(false);
            Debug.Log($"[{GetType().Name}] Shutdown complete.");
        }
    }
}
