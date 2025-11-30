using System;
using System.Threading.Tasks;

namespace Aarware.Services {
    /// <summary>
    /// Base interface for all service providers.
    /// Providers implement platform-specific functionality (Steam, Android, local, etc.).
    /// Note: Platform property is defined in service-specific interfaces (IAccountProvider, IStatsProvider, etc.)
    /// since different services use different platform types (BackendPlatform vs NetworkingPlatform).
    /// </summary>
    public interface IServiceProvider {
        /// <summary>
        /// Initializes the provider.
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Shuts down the provider and cleans up resources.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Whether the provider is initialized and ready.
        /// </summary>
        bool IsInitialized { get; }
    }

    /// <summary>
    /// Backend platforms for Account, Stats, Achievements, Leaderboards, and Data Storage services.
    /// </summary>
    public enum BackendPlatform {
        Local,
        Custom,
        Steam,
        GooglePlay,         // Android Google Play Services
        UniversalWindows,   // Universal Windows Platform
        // Future platforms (not yet implemented):
        // Apple,
        // PlayStation,
        // Xbox,
        // Nintendo
    }

    /// <summary>
    /// Networking platforms for multiplayer/networking services.
    /// </summary>
    public enum NetworkingPlatform {
        Local,      // Offline or LAN
        Custom,     // Custom backend
        Photon,     // Photon Unity Networking
        Steam       // Steam P2P networking
    }

    /// <summary>
    /// Supported service platforms.
    /// OBSOLETE: Use BackendPlatform for backend services or NetworkingPlatform for networking.
    /// </summary>
    [System.Obsolete("Use BackendPlatform for backend services or NetworkingPlatform for networking services")]
    public enum ServicePlatform {
        Local,
        Steam,
        Android,
        Windows,
        Photon,
        Custom
    }
}
