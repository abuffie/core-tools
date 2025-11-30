using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Aarware.Localization {
    /// <summary>
    /// Component that automatically translates UI text based on a localization key.
    /// Supports both Unity UI Text and TextMeshPro.
    /// </summary>
    public class LocaleComponent : MonoBehaviour {
        [SerializeField] string localizationKey;
        [SerializeField] bool useTextMeshPro = false;

        Text uiText;
        TextMeshProUGUI tmpText;

        void Awake() {
            if (useTextMeshPro) {
                tmpText = GetComponent<TextMeshProUGUI>();
                if (tmpText == null) {
                    Debug.LogError($"[LocaleComponent] TextMeshProUGUI component not found on {gameObject.name}");
                }
            } else {
                uiText = GetComponent<Text>();
                if (uiText == null) {
                    Debug.LogError($"[LocaleComponent] Text component not found on {gameObject.name}");
                }
            }
        }

        void OnEnable() {
            if (LocaleController.HasInstance) {
                LocaleController.Instance.OnLanguageChanged += OnLanguageChanged;
                UpdateText();
            }
        }

        void OnDisable() {
            if (LocaleController.HasInstance) {
                LocaleController.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        void Start() {
            // Ensure text is updated on start in case OnEnable happened before LocaleController initialized
            if (LocaleController.HasInstance) {
                UpdateText();
            }
        }

        void OnLanguageChanged(SystemLanguage newLanguage) {
            UpdateText();
        }

        /// <summary>
        /// Updates the text with the translated value.
        /// </summary>
        void UpdateText() {
            if (string.IsNullOrEmpty(localizationKey)) {
                return;
            }

            if (!LocaleController.HasInstance) {
                Debug.LogWarning($"[LocaleComponent] LocaleController instance not found. Cannot translate '{localizationKey}'");
                return;
            }

            string translatedText = LocaleController.Instance.GetTranslation(localizationKey);

            if (useTextMeshPro && tmpText != null) {
                tmpText.text = translatedText;
            } else if (uiText != null) {
                uiText.text = translatedText;
            }
        }

        /// <summary>
        /// Sets the localization key and updates the text.
        /// </summary>
        public void SetLocalizationKey(string key) {
            localizationKey = key;
            UpdateText();
        }

        /// <summary>
        /// Gets the current localization key.
        /// </summary>
        public string GetLocalizationKey() {
            return localizationKey;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to capture current text as the key.
        /// </summary>
        [ContextMenu("Use Current Text as Key")]
        void UseCurrentTextAsKey() {
            if (useTextMeshPro) {
                tmpText = GetComponent<TextMeshProUGUI>();
                if (tmpText != null) {
                    localizationKey = tmpText.text;
                }
            } else {
                uiText = GetComponent<Text>();
                if (uiText != null) {
                    localizationKey = uiText.text;
                }
            }
            Debug.Log($"[LocaleComponent] Set localization key to: {localizationKey}");
        }
#endif
    }
}
