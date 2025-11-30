using UnityEngine;

namespace Aarware.Services.Networking {
    /// <summary>
    /// Configuration for networking/multiplayer services.
    /// </summary>
    [CreateAssetMenu(fileName = "NetworkingConfiguration", menuName = "Aarware/Networking Configuration", order = 5)]
    public class NetworkingConfiguration : ScriptableObject {
        [Header("Platform Configuration")]
        [Tooltip("Networking mode: Local Only (offline/no multiplayer), Local+Cloud (LAN with matchmaking), or Cloud Only")]
        public ServiceStorageMode storageMode = ServiceStorageMode.CloudOnly;

        [Tooltip("Network platform to use (when not using Local Only)")]
        public NetworkingPlatform networkPlatform = NetworkingPlatform.Photon;

        [Header("Connection Settings")]
        [Tooltip("Maximum number of players per room")]
        public int maxPlayersPerRoom = 4;

        [Tooltip("Timeout for connection attempts (seconds)")]
        public float connectionTimeout = 10f;

        [Tooltip("Enable auto-reconnect on disconnect")]
        public bool autoReconnect = true;

        [Header("Photon Settings")]
        [Tooltip("Photon App ID (if using Photon)")]
        public string photonAppId = "";

        [Tooltip("Photon region (leave empty for auto)")]
        public string photonRegion = "";

        [Header("Custom Backend Settings")]
        [Tooltip("Server URL (for custom backends)")]
        public string customServerUrl = "";

        [Tooltip("Server port")]
        public int customServerPort = 7777;

        [Tooltip("Use SSL/TLS for custom backend")]
        public bool useSecureConnection = true;

        [Header("Steam Settings")]
        [Tooltip("Steam App ID")]
        public uint steamAppId = 0;

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static NetworkingConfiguration CreateDefault() {
            NetworkingConfiguration config = CreateInstance<NetworkingConfiguration>();
            config.storageMode = ServiceStorageMode.CloudOnly;
            config.networkPlatform = NetworkingPlatform.Photon;
            config.maxPlayersPerRoom = 4;
            config.connectionTimeout = 10f;
            config.autoReconnect = true;
            config.photonAppId = "";
            config.photonRegion = "";
            config.customServerUrl = "";
            config.customServerPort = 7777;
            config.useSecureConnection = true;
            config.steamAppId = 0;
            return config;
        }
    }
}
