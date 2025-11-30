using UnityEngine;

namespace Aarware.Utilities {
    /// <summary>
    /// Configuration for SettingsStorage default values.
    /// Used for device settings, session tokens, and other key-value pairs stored in PlayerPrefs.
    /// </summary>
    [CreateAssetMenu(fileName = "SettingsStorageConfiguration", menuName = "Aarware/Settings Storage Configuration", order = 0)]
    public class SettingsStorageConfiguration : ScriptableObject {
        [Header("Default Values")]
        [Tooltip("Default language code (e.g., 'en', 'es', 'fr')")]
        public string defaultLanguage = "en";

        [Tooltip("Default master volume (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float defaultMasterVolume = 1f;

        [Tooltip("Default music volume (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float defaultMusicVolume = 1f;

        [Tooltip("Default SFX volume (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float defaultSFXVolume = 1f;

        [Tooltip("Default fullscreen setting")]
        public bool defaultFullscreen = true;

        [Header("Security")]
        [Tooltip("Encrypt session tokens in PlayerPrefs")]
        public bool encryptSessionTokens = false;

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static SettingsStorageConfiguration CreateDefault() {
            SettingsStorageConfiguration config = CreateInstance<SettingsStorageConfiguration>();
            config.defaultLanguage = "en";
            config.defaultMasterVolume = 1f;
            config.defaultMusicVolume = 1f;
            config.defaultSFXVolume = 1f;
            config.defaultFullscreen = true;
            config.encryptSessionTokens = false;
            return config;
        }
    }
}
