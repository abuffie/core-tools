using System;
using UnityEngine;

namespace Aarware.Utilities {
    /// <summary>
    /// Utility for storing device settings and session tokens using PlayerPrefs.
    /// Use this for key-value pairs like user preferences, device settings, and session data.
    /// For game save data, use DataStorageService instead.
    ///
    /// Note: Keys are used directly without prefixes. If you want namespacing, include it in your key
    /// (e.g., "Audio_MasterVolume", "Graphics_Quality", etc.)
    /// </summary>
    public static class SettingsStorage {
        static SettingsStorageConfiguration config;

        /// <summary>
        /// Sets the configuration for default values.
        /// </summary>
        public static void SetConfiguration(SettingsStorageConfiguration configuration) {
            config = configuration;
        }

        #region String Methods

        /// <summary>
        /// Saves a string value.
        /// </summary>
        public static void SetString(string key, string value) {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets a string value, returns defaultValue if not found.
        /// </summary>
        public static string GetString(string key, string defaultValue = "") {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        #endregion

        #region Int Methods

        /// <summary>
        /// Saves an int value.
        /// </summary>
        public static void SetInt(string key, int value) {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets an int value, returns defaultValue if not found.
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0) {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        #endregion

        #region Float Methods

        /// <summary>
        /// Saves a float value.
        /// </summary>
        public static void SetFloat(string key, float value) {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets a float value, returns defaultValue if not found.
        /// </summary>
        public static float GetFloat(string key, float defaultValue = 0f) {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        #endregion

        #region Bool Methods

        /// <summary>
        /// Saves a bool value (stored as int: 0 = false, 1 = true).
        /// </summary>
        public static void SetBool(string key, bool value) {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets a bool value, returns defaultValue if not found.
        /// </summary>
        public static bool GetBool(string key, bool defaultValue = false) {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        #endregion

        #region Object Methods

        /// <summary>
        /// Saves an object as JSON.
        /// </summary>
        public static void SetObject<T>(string key, T obj) {
            string json = JsonUtility.ToJson(obj);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets an object from JSON, returns default(T) if not found or invalid.
        /// </summary>
        public static T GetObject<T>(string key, T defaultValue = default) {
            string json = PlayerPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(json)) {
                return defaultValue;
            }

            try {
                return JsonUtility.FromJson<T>(json);
            } catch (Exception ex) {
                Debug.LogWarning($"[SettingsStorage] Failed to deserialize object for key '{key}': {ex.Message}");
                return defaultValue;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if a key exists.
        /// </summary>
        public static bool HasKey(string key) {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// Deletes a specific key.
        /// </summary>
        public static void Delete(string key) {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Deletes all PlayerPrefs (WARNING: This affects ALL PlayerPrefs, not just Aarware settings).
        /// </summary>
        public static void DeleteAll() {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Forces PlayerPrefs to save immediately.
        /// </summary>
        public static void Save() {
            PlayerPrefs.Save();
        }

        #endregion

        #region Common Settings Helpers

        /// <summary>
        /// Gets the selected language code (e.g., "en", "es", "fr").
        /// </summary>
        public static string GetLanguage() {
            string defaultValue = config != null ? config.defaultLanguage : "en";
            return GetString("Language", defaultValue);
        }

        /// <summary>
        /// Sets the selected language code.
        /// </summary>
        public static void SetLanguage(string languageCode) {
            SetString("Language", languageCode);
        }

        /// <summary>
        /// Gets the master volume setting (0.0 to 1.0).
        /// </summary>
        public static float GetMasterVolume() {
            float defaultValue = config != null ? config.defaultMasterVolume : 1f;
            return GetFloat("MasterVolume", defaultValue);
        }

        /// <summary>
        /// Sets the master volume setting.
        /// </summary>
        public static void SetMasterVolume(float volume) {
            SetFloat("MasterVolume", Mathf.Clamp01(volume));
        }

        /// <summary>
        /// Gets the music volume setting (0.0 to 1.0).
        /// </summary>
        public static float GetMusicVolume() {
            float defaultValue = config != null ? config.defaultMusicVolume : 1f;
            return GetFloat("MusicVolume", defaultValue);
        }

        /// <summary>
        /// Sets the music volume setting.
        /// </summary>
        public static void SetMusicVolume(float volume) {
            SetFloat("MusicVolume", Mathf.Clamp01(volume));
        }

        /// <summary>
        /// Gets the SFX volume setting (0.0 to 1.0).
        /// </summary>
        public static float GetSFXVolume() {
            float defaultValue = config != null ? config.defaultSFXVolume : 1f;
            return GetFloat("SFXVolume", defaultValue);
        }

        /// <summary>
        /// Sets the SFX volume setting.
        /// </summary>
        public static void SetSFXVolume(float volume) {
            SetFloat("SFXVolume", Mathf.Clamp01(volume));
        }

        /// <summary>
        /// Gets the fullscreen setting.
        /// </summary>
        public static bool GetFullscreen() {
            bool defaultValue = config != null ? config.defaultFullscreen : true;
            return GetBool("Fullscreen", defaultValue);
        }

        /// <summary>
        /// Sets the fullscreen setting.
        /// </summary>
        public static void SetFullscreen(bool fullscreen) {
            SetBool("Fullscreen", fullscreen);
        }

        #endregion

        #region Session Token Methods

        /// <summary>
        /// Gets the current session token.
        /// </summary>
        public static string GetSessionToken() {
            return GetString("SessionToken", null);
        }

        /// <summary>
        /// Sets the current session token.
        /// </summary>
        public static void SetSessionToken(string token) {
            SetString("SessionToken", token);
        }

        /// <summary>
        /// Clears the current session token.
        /// </summary>
        public static void ClearSessionToken() {
            Delete("SessionToken");
        }

        /// <summary>
        /// Gets the current refresh token.
        /// </summary>
        public static string GetRefreshToken() {
            return GetString("RefreshToken", null);
        }

        /// <summary>
        /// Sets the current refresh token.
        /// </summary>
        public static void SetRefreshToken(string token) {
            SetString("RefreshToken", token);
        }

        /// <summary>
        /// Clears the current refresh token.
        /// </summary>
        public static void ClearRefreshToken() {
            Delete("RefreshToken");
        }

        /// <summary>
        /// Clears all session-related data.
        /// </summary>
        public static void ClearSession() {
            ClearSessionToken();
            ClearRefreshToken();
        }

        #endregion
    }
}
