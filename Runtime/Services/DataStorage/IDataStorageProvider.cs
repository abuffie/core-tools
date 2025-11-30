using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Interface for platform-specific data storage providers.
    /// </summary>
    public interface IDataStorageProvider : IServiceProvider {
        /// <summary>
        /// The backend platform this provider supports.
        /// </summary>
        BackendPlatform Platform { get; }

        /// <summary>
        /// Event fired when data is successfully saved.
        /// </summary>
        event Action<string> OnSaveComplete;

        /// <summary>
        /// Event fired when data is successfully loaded.
        /// </summary>
        event Action<string> OnLoadComplete;

        /// <summary>
        /// Event fired when save or load fails.
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Saves data to storage.
        /// </summary>
        Task<ServiceResult> SaveAsync<T>(string slotId, T data) where T : SaveData;

        /// <summary>
        /// Loads data from storage.
        /// </summary>
        Task<ServiceResult<T>> LoadAsync<T>(string slotId) where T : SaveData;

        /// <summary>
        /// Deletes a save slot.
        /// </summary>
        Task<ServiceResult> DeleteAsync(string slotId);

        /// <summary>
        /// Checks if a save exists in the specified slot.
        /// </summary>
        Task<bool> ExistsAsync(string slotId);

        /// <summary>
        /// Gets metadata for all available saves.
        /// </summary>
        Task<ServiceResult<List<SaveMetadata>>> GetAllSavesMetadataAsync();

        /// <summary>
        /// Syncs local data with cloud storage (if supported).
        /// </summary>
        Task<ServiceResult> SyncWithCloudAsync();
    }
}
