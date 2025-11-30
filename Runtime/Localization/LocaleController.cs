using System;
using System.Collections.Generic;
using UnityEngine;
using Aarware.Utilities;

namespace Aarware.Localization {
    /// <summary>
    /// Manages localization/language settings and provides translation services.
    /// Singleton pattern ensures only one instance exists.
    /// </summary>
    public class LocaleController : MonoSingleton<LocaleController> {
        [SerializeField] List<LocaleData> availableLocales = new List<LocaleData>();
        [SerializeField] SystemLanguage fallbackLanguage = SystemLanguage.English;

        LocaleData currentLocale;
        const string LANGUAGE_PREF_KEY = "Aarware_SelectedLanguage";

        /// <summary>
        /// Event fired when the language changes. Provides the new language code.
        /// </summary>
        public event Action<SystemLanguage> OnLanguageChanged;

        /// <summary>
        /// Gets the currently selected language.
        /// </summary>
        public SystemLanguage CurrentLanguage => currentLocale?.Language ?? fallbackLanguage;

        private void Awake() {
            base.Awake(persist: true, destroyComponentOnly: true);
        }

        protected override void OnInitialize() {
            LoadLanguage();
        }

        /// <summary>
        /// Loads the appropriate language on startup.
        /// Uses saved preference if available, otherwise uses system language.
        /// </summary>
        void LoadLanguage() {
            SystemLanguage targetLanguage;

            if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY)) {
                // User has previously selected a language
                int savedLanguage = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);
                targetLanguage = (SystemLanguage)savedLanguage;
            } else {
                // No saved preference, use system language
                targetLanguage = Application.systemLanguage;
                Debug.Log($"[LocaleController] No saved language preference. Using system language: {targetLanguage}");
            }

            SetLanguage(targetLanguage, false);
        }

        /// <summary>
        /// Changes the current language and notifies all listeners.
        /// </summary>
        public void SetLanguage(SystemLanguage language, bool savePreference = true) {
            LocaleData newLocale = FindLocaleData(language);

            if (newLocale == null) {
                Debug.LogWarning($"[LocaleController] Language {language} not found. Falling back to {fallbackLanguage}");
                newLocale = FindLocaleData(fallbackLanguage);
            }

            if (newLocale == null) {
                Debug.LogError($"[LocaleController] Fallback language {fallbackLanguage} not found! Please add locale data.");
                return;
            }

            currentLocale = newLocale;
            currentLocale.Initialize();

            if (savePreference) {
                PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)language);
                PlayerPrefs.Save();
            }

            Debug.Log($"[LocaleController] Language set to: {currentLocale.Language}");
            OnLanguageChanged?.Invoke(currentLocale.Language);
        }

        /// <summary>
        /// Gets a translated string for the given key.
        /// </summary>
        public string GetTranslation(string key) {
            if (currentLocale == null) {
                Debug.LogWarning($"[LocaleController] No locale loaded. Cannot translate '{key}'");
                return key;
            }

            return currentLocale.GetTranslation(key);
        }

        /// <summary>
        /// Gets all available languages.
        /// </summary>
        public List<SystemLanguage> GetAvailableLanguages() {
            List<SystemLanguage> languages = new List<SystemLanguage>();
            foreach (var locale in availableLocales) {
                if (locale != null) {
                    languages.Add(locale.Language);
                }
            }
            return languages;
        }

        /// <summary>
        /// Clears the saved language preference.
        /// </summary>
        public void ClearLanguagePreference() {
            PlayerPrefs.DeleteKey(LANGUAGE_PREF_KEY);
            PlayerPrefs.Save();
        }

        LocaleData FindLocaleData(SystemLanguage language) {
            foreach (var locale in availableLocales) {
                if (locale != null && locale.Language == language) {
                    return locale;
                }
            }
            return null;
        }
    }
}
