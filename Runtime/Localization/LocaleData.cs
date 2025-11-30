using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aarware.Localization {
    /// <summary>
    /// ScriptableObject that holds localization data for a specific language.
    /// </summary>
    [CreateAssetMenu(fileName = "LocaleData", menuName = "Aarware/Localization/Locale Data", order = 1)]
    public class LocaleData : ScriptableObject {
        [SerializeField] SystemLanguage language;
        [SerializeField] string languageCode;
        [SerializeField] List<LocaleEntry> entries = new List<LocaleEntry>();

        Dictionary<string, string> translationDictionary;

        public SystemLanguage Language => language;
        public string LanguageCode => languageCode;

        /// <summary>
        /// Initializes the translation dictionary for fast lookups.
        /// </summary>
        public void Initialize() {
            translationDictionary = new Dictionary<string, string>();
            foreach (var entry in entries) {
                if (!string.IsNullOrEmpty(entry.key)) {
                    translationDictionary[entry.key] = entry.value;
                }
            }
        }

        /// <summary>
        /// Gets a translated string by key.
        /// </summary>
        public string GetTranslation(string key) {
            if (translationDictionary == null) {
                Initialize();
            }

            if (translationDictionary.TryGetValue(key, out string value)) {
                return value;
            }

            Debug.LogWarning($"[LocaleData] Translation key '{key}' not found for language {language}");
            return key;
        }

        /// <summary>
        /// Adds or updates a translation entry.
        /// </summary>
        public void SetTranslation(string key, string value) {
            if (translationDictionary == null) {
                Initialize();
            }

            translationDictionary[key] = value;

            // Update or add to entries list
            bool found = false;
            for (int i = 0; i < entries.Count; i++) {
                if (entries[i].key == key) {
                    entries[i] = new LocaleEntry { key = key, value = value };
                    found = true;
                    break;
                }
            }

            if (!found) {
                entries.Add(new LocaleEntry { key = key, value = value });
            }
        }

        /// <summary>
        /// Gets all translation entries (for editor use).
        /// </summary>
        public List<LocaleEntry> GetAllEntries() {
            return entries;
        }
    }

    [Serializable]
    public struct LocaleEntry {
        public string key;
        [TextArea(2, 5)]
        public string value;
    }
}
