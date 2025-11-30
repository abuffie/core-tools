using UnityEngine;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Configuration for data storage/save system.
    /// </summary>
    [CreateAssetMenu(fileName = "DataStorageConfiguration", menuName = "Aarware/Data Storage Configuration", order = 6)]
    public class DataStorageConfiguration : ScriptableObject {
        [Header("Platform Configuration")]
        [Tooltip("How save data is stored: Local Only, Local+Cloud Sync, or Cloud Only")]
        public ServiceStorageMode storageMode = ServiceStorageMode.LocalWithCloudSync;

        [Tooltip("Cloud platform for syncing (when not using Local Only)")]
        public BackendPlatform cloudPlatform = BackendPlatform.Steam;

        [Header("Storage Settings")]
        [Tooltip("Enable automatic cloud sync (if platform supports it)")]
        public bool enableCloudSync = true;

        [Tooltip("Auto-save interval in seconds (0 = disabled)")]
        public float autoSaveInterval = 0f;

        [Tooltip("Maximum number of save slots")]
        public int maxSaveSlots = 10;

        [Tooltip("Encrypt save data")]
        public bool encryptSaveData = false;

        [Header("Backup Settings")]
        [Tooltip("Create backups of save files")]
        public bool createBackups = true;

        [Tooltip("Maximum number of backups to keep per save slot")]
        public int maxBackupsPerSlot = 3;

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static DataStorageConfiguration CreateDefault() {
            DataStorageConfiguration config = CreateInstance<DataStorageConfiguration>();
            config.storageMode = ServiceStorageMode.LocalWithCloudSync;
            config.cloudPlatform = BackendPlatform.Steam;
            config.enableCloudSync = true;
            config.autoSaveInterval = 0f;
            config.maxSaveSlots = 10;
            config.encryptSaveData = false;
            config.createBackups = true;
            config.maxBackupsPerSlot = 3;
            return config;
        }
    }
}
