using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Hybrid data storage provider that combines local + cloud sync.
    /// Strategy: Always save to local first (for offline play), then sync to cloud in background.
    /// On load: Loads from local, then checks cloud for newer version and merges if needed.
    /// </summary>
    public class HybridDataStorageProvider : IDataStorageProvider {
        readonly LocalDataStorageProvider localProvider;
        readonly IDataStorageProvider cloudProvider;
        readonly bool enableAutoSync;

        public BackendPlatform Platform => cloudProvider.Platform;
        public bool IsInitialized { get; private set; }

        public event Action<string> OnSaveComplete;
        public event Action<string> OnLoadComplete;
        public event Action<string> OnError;

        /// <summary>
        /// Creates a hybrid provider with local + cloud sync.
        /// </summary>
        /// <param name="cloudProvider">The cloud provider to sync with (Steam, Android, Windows)</param>
        /// <param name="enableAutoSync">If true, automatically syncs to cloud after local save</param>
        public HybridDataStorageProvider(IDataStorageProvider cloudProvider, bool enableAutoSync = true) {
            this.localProvider = new LocalDataStorageProvider();
            this.cloudProvider = cloudProvider;
            this.enableAutoSync = enableAutoSync;
        }

        public async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                return true;
            }

            // Initialize both providers
            bool localSuccess = await localProvider.InitializeAsync();
            bool cloudSuccess = await cloudProvider.InitializeAsync();

            if (!localSuccess) {
                Debug.LogError("[HybridDataStorageProvider] Failed to initialize local provider");
                return false;
            }

            if (!cloudSuccess) {
                Debug.LogWarning("[HybridDataStorageProvider] Failed to initialize cloud provider - will work in offline mode");
            }

            // Subscribe to provider events
            localProvider.OnSaveComplete += HandleLocalSaveComplete;
            localProvider.OnLoadComplete += HandleLocalLoadComplete;
            localProvider.OnError += HandleLocalError;

            cloudProvider.OnSaveComplete += HandleCloudSaveComplete;
            cloudProvider.OnLoadComplete += HandleCloudLoadComplete;
            cloudProvider.OnError += HandleCloudError;

            IsInitialized = true;
            Debug.Log($"[HybridDataStorageProvider] Initialized with {cloudProvider.Platform} cloud sync");
            return true;
        }

        public void Shutdown() {
            if (localProvider != null) {
                localProvider.OnSaveComplete -= HandleLocalSaveComplete;
                localProvider.OnLoadComplete -= HandleLocalLoadComplete;
                localProvider.OnError -= HandleLocalError;
                localProvider.Shutdown();
            }

            if (cloudProvider != null) {
                cloudProvider.OnSaveComplete -= HandleCloudSaveComplete;
                cloudProvider.OnLoadComplete -= HandleCloudLoadComplete;
                cloudProvider.OnError -= HandleCloudError;
                cloudProvider.Shutdown();
            }

            IsInitialized = false;
        }

        /// <summary>
        /// Saves data to local storage first, then syncs to cloud (if auto-sync enabled).
        /// </summary>
        public async Task<ServiceResult> SaveAsync<T>(string slotId, T data) where T : SaveData {
            // Save to local first (this always works, even offline)
            ServiceResult localResult = await localProvider.SaveAsync(slotId, data);
            if (!localResult.Success) {
                return localResult;
            }

            // Auto-sync to cloud if enabled
            if (enableAutoSync && cloudProvider.IsInitialized) {
                _ = SyncToCloudAsync(slotId, data); // Fire and forget
            }

            OnSaveComplete?.Invoke(slotId);
            return ServiceResult.Successful();
        }

        /// <summary>
        /// Loads data from local storage, then checks cloud for newer version.
        /// </summary>
        public async Task<ServiceResult<T>> LoadAsync<T>(string slotId) where T : SaveData {
            // Load from local first
            ServiceResult<T> localResult = await localProvider.LoadAsync<T>(slotId);

            // If cloud provider is available, check for newer version
            if (cloudProvider.IsInitialized) {
                try {
                    ServiceResult<T> cloudResult = await cloudProvider.LoadAsync<T>(slotId);
                    if (cloudResult.Success && cloudResult.Data != null) {
                        // Compare timestamps and use newer version
                        if (ShouldUseCloudVersion(localResult.Data, cloudResult.Data)) {
                            Debug.Log($"[HybridDataStorageProvider] Cloud version is newer for slot {slotId}, using cloud data");
                            // Save cloud version to local
                            await localProvider.SaveAsync(slotId, cloudResult.Data);
                            OnLoadComplete?.Invoke(slotId);
                            return cloudResult;
                        }
                    }
                } catch (Exception ex) {
                    Debug.LogWarning($"[HybridDataStorageProvider] Failed to check cloud for slot {slotId}: {ex.Message}");
                }
            }

            OnLoadComplete?.Invoke(slotId);
            return localResult;
        }

        public async Task<ServiceResult> DeleteAsync(string slotId) {
            // Delete from both local and cloud
            ServiceResult localResult = await localProvider.DeleteAsync(slotId);

            if (cloudProvider.IsInitialized) {
                ServiceResult cloudResult = await cloudProvider.DeleteAsync(slotId);
                if (!cloudResult.Success) {
                    Debug.LogWarning($"[HybridDataStorageProvider] Failed to delete from cloud: {cloudResult.ErrorMessage}");
                }
            }

            return localResult;
        }

        public async Task<bool> ExistsAsync(string slotId) {
            // Check local first (faster)
            bool localExists = await localProvider.ExistsAsync(slotId);
            if (localExists) {
                return true;
            }

            // Check cloud if local doesn't exist
            if (cloudProvider.IsInitialized) {
                return await cloudProvider.ExistsAsync(slotId);
            }

            return false;
        }

        public async Task<ServiceResult<List<SaveMetadata>>> GetAllSavesMetadataAsync() {
            // Get from local (primary source)
            ServiceResult<List<SaveMetadata>> localResult = await localProvider.GetAllSavesMetadataAsync();

            // TODO: Merge with cloud metadata if needed
            return localResult;
        }

        /// <summary>
        /// Manually syncs all local saves to cloud.
        /// </summary>
        public async Task<ServiceResult> SyncWithCloudAsync() {
            if (!cloudProvider.IsInitialized) {
                return ServiceResult.Failed("Cloud provider not initialized");
            }

            try {
                // Get all local saves
                ServiceResult<List<SaveMetadata>> metadataResult = await localProvider.GetAllSavesMetadataAsync();
                if (!metadataResult.Success) {
                    return ServiceResult.Failed($"Failed to get local saves: {metadataResult.ErrorMessage}");
                }

                // Sync each save to cloud
                int syncedCount = 0;
                foreach (SaveMetadata metadata in metadataResult.Data) {
                    try {
                        // Load from local
                        ServiceResult<SaveData> loadResult = await localProvider.LoadAsync<SaveData>(metadata.saveId);
                        if (loadResult.Success && loadResult.Data != null) {
                            // Save to cloud
                            ServiceResult saveResult = await cloudProvider.SaveAsync(metadata.saveId, loadResult.Data);
                            if (saveResult.Success) {
                                syncedCount++;
                            }
                        }
                    } catch (Exception ex) {
                        Debug.LogWarning($"[HybridDataStorageProvider] Failed to sync slot {metadata.saveId}: {ex.Message}");
                    }
                }

                Debug.Log($"[HybridDataStorageProvider] Synced {syncedCount}/{metadataResult.Data.Count} saves to cloud");
                return ServiceResult.Successful();
            } catch (Exception ex) {
                return ServiceResult.Failed($"Cloud sync failed: {ex.Message}");
            }
        }

        async Task SyncToCloudAsync<T>(string slotId, T data) where T : SaveData {
            try {
                ServiceResult result = await cloudProvider.SaveAsync(slotId, data);
                if (result.Success) {
                    Debug.Log($"[HybridDataStorageProvider] Successfully synced slot {slotId} to cloud");
                } else {
                    Debug.LogWarning($"[HybridDataStorageProvider] Failed to sync slot {slotId} to cloud: {result.ErrorMessage}");
                }
            } catch (Exception ex) {
                Debug.LogWarning($"[HybridDataStorageProvider] Cloud sync error for slot {slotId}: {ex.Message}");
            }
        }

        bool ShouldUseCloudVersion(SaveData localData, SaveData cloudData) {
            if (localData == null) {
                return true;
            }
            if (cloudData == null) {
                return false;
            }
            // Use cloud if it's newer
            return cloudData.lastModified > localData.lastModified;
        }

        void HandleLocalSaveComplete(string slotId) {
            Debug.Log($"[HybridDataStorageProvider] Local save complete: {slotId}");
        }

        void HandleLocalLoadComplete(string slotId) {
            Debug.Log($"[HybridDataStorageProvider] Local load complete: {slotId}");
        }

        void HandleLocalError(string error) {
            Debug.LogError($"[HybridDataStorageProvider] Local error: {error}");
            OnError?.Invoke($"Local: {error}");
        }

        void HandleCloudSaveComplete(string slotId) {
            Debug.Log($"[HybridDataStorageProvider] Cloud save complete: {slotId}");
        }

        void HandleCloudLoadComplete(string slotId) {
            Debug.Log($"[HybridDataStorageProvider] Cloud load complete: {slotId}");
        }

        void HandleCloudError(string error) {
            Debug.LogWarning($"[HybridDataStorageProvider] Cloud error: {error}");
            OnError?.Invoke($"Cloud: {error}");
        }
    }
}
