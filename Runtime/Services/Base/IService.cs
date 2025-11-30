using System;
using System.Threading.Tasks;

namespace Aarware.Services {
    /// <summary>
    /// Base interface for all services in the system.
    /// Services provide high-level functionality and delegate platform-specific work to providers.
    /// </summary>
    public interface IService {
        /// <summary>
        /// Initializes the service. Called once when the service is first created.
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Shuts down the service and cleans up resources.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Whether the service has been initialized and is ready to use.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Event raised when the service initialization state changes.
        /// </summary>
        event Action<bool> OnInitializationChanged;
    }
}
