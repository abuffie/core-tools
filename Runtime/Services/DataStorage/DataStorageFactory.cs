using UnityEngine;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Factory for creating and initializing DataStorageService with the correct provider
    /// based on configuration settings.
    /// </summary>
    public static class DataStorageFactory {
        /// <summary>
        /// Creates and initializes a DataStorageService based on the provided configuration.
        /// </summary>
        public static DataStorageService Create(DataStorageConfiguration config) {
            if (config == null) {
                Debug.LogError("[DataStorageFactory] Configuration is null, using default local-only storage");
                config = DataStorageConfiguration.CreateDefault();
            }

            IDataStorageProvider provider = CreateProvider(config);

            DataStorageService service = new DataStorageService();
            service.SetProvider(provider);

            Debug.Log($"[DataStorageFactory] Created DataStorageService with {config.storageMode} mode");
            return service;
        }

        static IDataStorageProvider CreateProvider(DataStorageConfiguration config) {
            switch (config.storageMode) {
                case ServiceStorageMode.LocalOnly:
                    return CreateLocalProvider();

                case ServiceStorageMode.LocalWithCloudSync:
                    return CreateHybridProvider(config);

                case ServiceStorageMode.CloudOnly:
                    return CreateCloudProvider(config);

                default:
                    Debug.LogWarning($"[DataStorageFactory] Unknown storage mode {config.storageMode}, defaulting to LocalOnly");
                    return CreateLocalProvider();
            }
        }

        static IDataStorageProvider CreateLocalProvider() {
            Debug.Log("[DataStorageFactory] Creating Local-Only provider");
            return new LocalDataStorageProvider();
        }

        static IDataStorageProvider CreateHybridProvider(DataStorageConfiguration config) {
            Debug.Log($"[DataStorageFactory] Creating Hybrid (Local + {config.cloudPlatform} Cloud) provider");

            IDataStorageProvider cloudProvider = CreateCloudProviderByPlatform(config.cloudPlatform);
            return new HybridDataStorageProvider(cloudProvider, config.enableCloudSync);
        }

        static IDataStorageProvider CreateCloudProvider(DataStorageConfiguration config) {
            Debug.Log($"[DataStorageFactory] Creating Cloud-Only ({config.cloudPlatform}) provider");
            return CreateCloudProviderByPlatform(config.cloudPlatform);
        }

        static IDataStorageProvider CreateCloudProviderByPlatform(BackendPlatform platform) {
            switch (platform) {
                case BackendPlatform.Steam:
                    return new SteamDataStorageProvider();

                case BackendPlatform.GooglePlay:
                    return new AndroidDataStorageProvider();

                case BackendPlatform.UniversalWindows:
                    return new WindowsDataStorageProvider();

                case BackendPlatform.Local:
                    Debug.LogWarning("[DataStorageFactory] Local platform selected for cloud provider, using LocalDataStorageProvider");
                    return new LocalDataStorageProvider();

                default:
                    Debug.LogError($"[DataStorageFactory] Unsupported cloud platform {platform}, falling back to Local");
                    return new LocalDataStorageProvider();
            }
        }
    }
}
