using System.IO;
using UnityEngine;

namespace Aarware.Utilities {
    /// <summary>
    /// Handles local data storage with automatic WebGL PlayerPrefs fallback.
    /// Use this instead of PlayerPrefs for game data (stats, achievements, saves).
    /// PlayerPrefs should only be used for user settings (volume, language, etc.).
    /// </summary>
    public static class LocalStorageHelper {
        static readonly string STORAGE_PATH = Application.persistentDataPath;

        /// <summary>
        /// Saves data with the given key.
        /// Desktop/Mobile: Saves to JSON file in persistentDataPath
        /// WebGL: Saves to PlayerPrefs (browser IndexedDB)
        /// </summary>
        public static void SaveData(string key, string jsonData) {
            #if UNITY_WEBGL && !UNITY_EDITOR
                try {
                    PlayerPrefs.SetString(key, jsonData);
                    PlayerPrefs.Save();
                } catch (System.Exception e) {
                    Debug.LogError($"[LocalStorageHelper] Failed to save data for key '{key}' to PlayerPrefs: {e.Message}");
                }
            #else
                try {
                    string path = Path.Combine(STORAGE_PATH, $"{key}.json");
                    File.WriteAllText(path, jsonData);
                } catch (System.Exception e) {
                    Debug.LogError($"[LocalStorageHelper] Failed to save data for key '{key}' to file: {e.Message}");
                }
            #endif
        }

        /// <summary>
        /// Loads data for the given key.
        /// Returns empty string if data doesn't exist or fails to load.
        /// </summary>
        public static string LoadData(string key) {
            #if UNITY_WEBGL && !UNITY_EDITOR
                try {
                    return PlayerPrefs.GetString(key, "");
                } catch (System.Exception e) {
                    Debug.LogWarning($"[LocalStorageHelper] Failed to load data for key '{key}' from PlayerPrefs: {e.Message}");
                    return "";
                }
            #else
                try {
                    string path = Path.Combine(STORAGE_PATH, $"{key}.json");
                    if (File.Exists(path)) {
                        return File.ReadAllText(path);
                    }
                    return "";
                } catch (System.Exception e) {
                    Debug.LogWarning($"[LocalStorageHelper] Failed to load data for key '{key}' from file: {e.Message}");
                    return "";
                }
            #endif
        }

        /// <summary>
        /// Checks if data exists for the given key.
        /// </summary>
        public static bool HasData(string key) {
            #if UNITY_WEBGL && !UNITY_EDITOR
                return PlayerPrefs.HasKey(key);
            #else
                string path = Path.Combine(STORAGE_PATH, $"{key}.json");
                return File.Exists(path);
            #endif
        }

        /// <summary>
        /// Deletes data for the given key.
        /// </summary>
        public static void DeleteData(string key) {
            #if UNITY_WEBGL && !UNITY_EDITOR
                try {
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                } catch (System.Exception e) {
                    Debug.LogError($"[LocalStorageHelper] Failed to delete data for key '{key}' from PlayerPrefs: {e.Message}");
                }
            #else
                try {
                    string path = Path.Combine(STORAGE_PATH, $"{key}.json");
                    if (File.Exists(path)) {
                        File.Delete(path);
                    }
                } catch (System.Exception e) {
                    Debug.LogError($"[LocalStorageHelper] Failed to delete data for key '{key}' from file: {e.Message}");
                }
            #endif
        }

        /// <summary>
        /// Gets the full storage path for a given key (file-based platforms only).
        /// Returns null for WebGL.
        /// </summary>
        public static string GetStoragePath(string key) {
            #if UNITY_WEBGL && !UNITY_EDITOR
                return null;
            #else
                return Path.Combine(STORAGE_PATH, $"{key}.json");
            #endif
        }

        /// <summary>
        /// Gets the directory where local storage files are saved.
        /// Returns null for WebGL.
        /// </summary>
        public static string GetStorageDirectory() {
            #if UNITY_WEBGL && !UNITY_EDITOR
                return null;
            #else
                return STORAGE_PATH;
            #endif
        }

        /// <summary>
        /// Checks if the current platform uses file-based storage.
        /// </summary>
        public static bool IsFileBased() {
            #if UNITY_WEBGL && !UNITY_EDITOR
                return false;
            #else
                return true;
            #endif
        }
    }
}
