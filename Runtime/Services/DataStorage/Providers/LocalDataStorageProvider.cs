using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Aarware.Utilities;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Local data storage provider for GAME SAVE DATA ONLY (player progress, inventory, etc.).
    /// For device settings and session tokens, use SettingsStorage instead.
    ///
    /// Storage Strategy:
    /// - Desktop/Mobile: JSON files in Application.persistentDataPath/Saves/
    /// - WebGL: PlayerPrefs fallback (browser IndexedDB)
    ///
    /// This provider is always used as the local layer, even when cloud sync is enabled.
    /// </summary>
    public class LocalDataStorageProvider : IDataStorageProvider {
        const string SAVE_FOLDER = "Saves";
        const string FILE_EXTENSION = ".json";
        const string SAVE_INDEX_KEY = "Aarware_SaveIndex";

        string savePath;

        public BackendPlatform Platform => BackendPlatform.Local;
        public bool IsInitialized { get; private set; }

        public event Action<string> OnSaveComplete;
        public event Action<string> OnLoadComplete;
        public event Action<string> OnError;

        public async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                return true;
            }

            try {
                #if !UNITY_WEBGL || UNITY_EDITOR
                savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
                if (!Directory.Exists(savePath)) {
                    Directory.CreateDirectory(savePath);
                }
                Debug.Log($"[LocalDataStorageProvider] Initialized. Save path: {savePath}");
                #else
                Debug.Log($"[LocalDataStorageProvider] Initialized (WebGL mode - using PlayerPrefs)");
                #endif

                IsInitialized = true;
                await Task.CompletedTask;
                return true;
            } catch (Exception ex) {
                Debug.LogError($"[LocalDataStorageProvider] Failed to initialize: {ex.Message}");
                await Task.CompletedTask;
                return false;
            }
        }

        public void Shutdown() {
            IsInitialized = false;
        }

        public async Task<ServiceResult> SaveAsync<T>(string slotId, T data) where T : SaveData {
            try {
                string key = GetSaveKey(slotId);
                string json = JsonUtility.ToJson(data, true);

                LocalStorageHelper.SaveData(key, json);
                AddToSaveIndex(slotId);

                OnSaveComplete?.Invoke(slotId);
                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                string error = $"Failed to save data to slot {slotId}: {ex.Message}";
                OnError?.Invoke(error);
                await Task.CompletedTask;
                return ServiceResult.Failed(error);
            }
        }

        public async Task<ServiceResult<T>> LoadAsync<T>(string slotId) where T : SaveData {
            try {
                string key = GetSaveKey(slotId);

                if (!LocalStorageHelper.HasData(key)) {
                    string error = $"Save file not found: {slotId}";
                    OnError?.Invoke(error);
                    await Task.CompletedTask;
                    return ServiceResult<T>.Failed(error);
                }

                string json = LocalStorageHelper.LoadData(key);
                if (string.IsNullOrEmpty(json)) {
                    string error = $"Save file is empty: {slotId}";
                    OnError?.Invoke(error);
                    await Task.CompletedTask;
                    return ServiceResult<T>.Failed(error);
                }

                T data = JsonUtility.FromJson<T>(json);

                OnLoadComplete?.Invoke(slotId);
                await Task.CompletedTask;
                return ServiceResult<T>.Successful(data);
            } catch (Exception ex) {
                string error = $"Failed to load data from slot {slotId}: {ex.Message}";
                OnError?.Invoke(error);
                await Task.CompletedTask;
                return ServiceResult<T>.Failed(error);
            }
        }

        public async Task<ServiceResult> DeleteAsync(string slotId) {
            try {
                string key = GetSaveKey(slotId);

                if (LocalStorageHelper.HasData(key)) {
                    LocalStorageHelper.DeleteData(key);
                    RemoveFromSaveIndex(slotId);
                }

                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                string error = $"Failed to delete save slot {slotId}: {ex.Message}";
                OnError?.Invoke(error);
                await Task.CompletedTask;
                return ServiceResult.Failed(error);
            }
        }

        public async Task<bool> ExistsAsync(string slotId) {
            string key = GetSaveKey(slotId);
            await Task.CompletedTask;
            return LocalStorageHelper.HasData(key);
        }

        public async Task<ServiceResult<List<SaveMetadata>>> GetAllSavesMetadataAsync() {
            try {
                List<SaveMetadata> metadataList = new List<SaveMetadata>();
                List<string> slotIds = GetSaveIndex();

                foreach (string slotId in slotIds) {
                    try {
                        string key = GetSaveKey(slotId);
                        if (!LocalStorageHelper.HasData(key)) {
                            continue;
                        }

                        string json = LocalStorageHelper.LoadData(key);
                        if (string.IsNullOrEmpty(json)) {
                            continue;
                        }

                        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                        long fileSize = json.Length; // Approximate size
                        SaveMetadata metadata = new SaveMetadata(saveData, fileSize);
                        metadataList.Add(metadata);
                    } catch (Exception ex) {
                        Debug.LogWarning($"[LocalDataStorageProvider] Failed to read metadata from slot {slotId}: {ex.Message}");
                    }
                }

                await Task.CompletedTask;
                return ServiceResult<List<SaveMetadata>>.Successful(metadataList);
            } catch (Exception ex) {
                string error = $"Failed to get saves metadata: {ex.Message}";
                OnError?.Invoke(error);
                await Task.CompletedTask;
                return ServiceResult<List<SaveMetadata>>.Failed(error);
            }
        }

        public async Task<ServiceResult> SyncWithCloudAsync() {
            // Local provider doesn't support cloud sync
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        string GetSaveKey(string slotId) {
            return $"{SAVE_FOLDER}_{slotId}";
        }

        /// <summary>
        /// Gets the full save directory path (for debugging/editor tools).
        /// Returns null on WebGL.
        /// </summary>
        public string GetSavePath() {
            #if !UNITY_WEBGL || UNITY_EDITOR
            return savePath;
            #else
            return null;
            #endif
        }

        List<string> GetSaveIndex() {
            string indexJson = LocalStorageHelper.LoadData(SAVE_INDEX_KEY);
            if (string.IsNullOrEmpty(indexJson)) {
                return new List<string>();
            }

            try {
                SaveIndex index = JsonUtility.FromJson<SaveIndex>(indexJson);
                return index.slotIds;
            } catch {
                return new List<string>();
            }
        }

        void AddToSaveIndex(string slotId) {
            List<string> slotIds = GetSaveIndex();
            if (!slotIds.Contains(slotId)) {
                slotIds.Add(slotId);
                SaveIndex index = new SaveIndex { slotIds = slotIds };
                string json = JsonUtility.ToJson(index);
                LocalStorageHelper.SaveData(SAVE_INDEX_KEY, json);
            }
        }

        void RemoveFromSaveIndex(string slotId) {
            List<string> slotIds = GetSaveIndex();
            if (slotIds.Remove(slotId)) {
                SaveIndex index = new SaveIndex { slotIds = slotIds };
                string json = JsonUtility.ToJson(index);
                LocalStorageHelper.SaveData(SAVE_INDEX_KEY, json);
            }
        }

        [Serializable]
        class SaveIndex {
            public List<string> slotIds = new List<string>();
        }
    }
}
