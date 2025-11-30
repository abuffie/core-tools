using UnityEngine;
using UnityEngine.UI;

namespace Aarware.RemoteImage {
    /// <summary>
    /// Component that automatically loads a remote image into a UI Image component.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class RemoteImageComponent : MonoBehaviour {
        [SerializeField] string imageUrl;
        [SerializeField] Sprite placeholderSprite;
        [SerializeField] bool loadOnStart = true;

        Image targetImage;
        bool isLoading = false;

        void Awake() {
            targetImage = GetComponent<Image>();
            if (placeholderSprite != null) {
                targetImage.sprite = placeholderSprite;
            }
        }

        void Start() {
            if (loadOnStart && !string.IsNullOrEmpty(imageUrl)) {
                LoadImage();
            }
        }

        /// <summary>
        /// Loads the image from the specified URL.
        /// </summary>
        public void LoadImage() {
            if (string.IsNullOrEmpty(imageUrl)) {
                Debug.LogWarning($"[RemoteImageComponent] Image URL is empty on {gameObject.name}");
                return;
            }

            if (isLoading) {
                return;
            }

            if (!RemoteImageLoader.HasInstance) {
                Debug.LogError("[RemoteImageComponent] RemoteImageLoader instance not found.");
                return;
            }

            isLoading = true;
            RemoteImageLoader.Instance.LoadImage(imageUrl, OnImageLoaded);
        }

        /// <summary>
        /// Sets a new image URL and loads it.
        /// </summary>
        public void SetImageUrl(string url, bool loadImmediately = true) {
            imageUrl = url;
            if (loadImmediately) {
                LoadImage();
            }
        }

        void OnImageLoaded(Sprite sprite, string error) {
            isLoading = false;

            if (!string.IsNullOrEmpty(error)) {
                Debug.LogError($"[RemoteImageComponent] Failed to load image on {gameObject.name}: {error}");
                return;
            }

            if (sprite != null && targetImage != null) {
                targetImage.sprite = sprite;
            }
        }

        void OnDestroy() {
            // Image cache is managed by RemoteImageLoader singleton
        }
    }
}
