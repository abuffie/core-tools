using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Service for managing GAME SAVE DATA across different platforms (player progress, inventory, etc.).
    /// For device settings and session tokens, use SettingsStorage instead.
    ///
    /// Storage Modes:
    /// - Local Only: Saves to local files only (no cloud)
    /// - Local + Cloud Sync: Saves to local files first, then syncs to cloud (offline support)
    /// - Cloud Only: Saves to cloud only (requires connection)
    /// </summary>
    public class DataStorageService : ServiceBase<IDataStorageProvider> {
        /// <summary>
        /// Event fired when data is successfully saved.
        /// </summary>
        public event Action<string> OnSaveComplete;

        /// <summary>
        /// Event fired when data is successfully loaded.
        /// </summary>
        public event Action<string> OnLoadComplete;

        /// <summary>
        /// Event fired when save or load fails.
        /// </summary>
        public event Action<string> OnError;

        public override void SetProvider(IDataStorageProvider provider) {
            // Unsubscribe from old provider
            if (currentProvider != null) {
                currentProvider.OnSaveComplete -= HandleSaveComplete;
                currentProvider.OnLoadComplete -= HandleLoadComplete;
                currentProvider.OnError -= HandleError;
            }

            base.SetProvider(provider);

            // Subscribe to new provider
            if (currentProvider != null) {
                currentProvider.OnSaveComplete += HandleSaveComplete;
                currentProvider.OnLoadComplete += HandleLoadComplete;
                currentProvider.OnError += HandleError;
            }
        }

        /// <summary>
        /// Saves data to the specified slot.
        /// </summary>
        public async Task<ServiceResult> SaveAsync<T>(string slotId, T data) where T : SaveData {
            if (!IsInitialized) {
                return ServiceResult.Failed("DataStorage service not initialized");
            }

            data.OnBeforeSave();
            return await currentProvider.SaveAsync(slotId, data);
        }

        /// <summary>
        /// Loads data from the specified slot.
        /// </summary>
        public async Task<ServiceResult<T>> LoadAsync<T>(string slotId) where T : SaveData {
            if (!IsInitialized) {
                return ServiceResult<T>.Failed("DataStorage service not initialized");
            }

            var result = await currentProvider.LoadAsync<T>(slotId);
            if (result.Success && result.Data != null) {
                result.Data.OnAfterLoad();
            }
            return result;
        }

        /// <summary>
        /// Deletes a save slot.
        /// </summary>
        public async Task<ServiceResult> DeleteAsync(string slotId) {
            if (!IsInitialized) {
                return ServiceResult.Failed("DataStorage service not initialized");
            }

            return await currentProvider.DeleteAsync(slotId);
        }

        /// <summary>
        /// Checks if a save exists in the specified slot.
        /// </summary>
        public async Task<bool> ExistsAsync(string slotId) {
            if (!IsInitialized) {
                return false;
            }

            return await currentProvider.ExistsAsync(slotId);
        }

        /// <summary>
        /// Gets metadata for all available saves.
        /// </summary>
        public async Task<ServiceResult<List<SaveMetadata>>> GetAllSavesMetadataAsync() {
            if (!IsInitialized) {
                return ServiceResult<List<SaveMetadata>>.Failed("DataStorage service not initialized");
            }

            return await currentProvider.GetAllSavesMetadataAsync();
        }

        /// <summary>
        /// Syncs local data with cloud storage.
        /// </summary>
        public async Task<ServiceResult> SyncWithCloudAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("DataStorage service not initialized");
            }

            return await currentProvider.SyncWithCloudAsync();
        }

        void HandleSaveComplete(string slotId) {
            Debug.Log($"[DataStorageService] Save complete: {slotId}");
            OnSaveComplete?.Invoke(slotId);
        }

        void HandleLoadComplete(string slotId) {
            Debug.Log($"[DataStorageService] Load complete: {slotId}");
            OnLoadComplete?.Invoke(slotId);
        }

        void HandleError(string error) {
            Debug.LogError($"[DataStorageService] Error: {error}");
            OnError?.Invoke(error);
        }

        public override void Shutdown() {
            if (currentProvider != null) {
                currentProvider.OnSaveComplete -= HandleSaveComplete;
                currentProvider.OnLoadComplete -= HandleLoadComplete;
                currentProvider.OnError -= HandleError;
            }
            base.Shutdown();
        }
    }
}
