using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Aarware.Utilities;

namespace Aarware.RemoteImage {
    /// <summary>
    /// Service for loading and caching images from remote URLs.
    /// Singleton pattern ensures centralized cache management.
    /// </summary>
    public class RemoteImageLoader : MonoSingleton<RemoteImageLoader> {
        [SerializeField] int maxMemoryCacheSize = 50;
        [SerializeField] bool enableDiskCache = true;
        [SerializeField] int diskCacheDurationDays = 7;

        Dictionary<string, Sprite> memoryCache = new Dictionary<string, Sprite>();
        Queue<string> cacheQueue = new Queue<string>();
        string diskCachePath;

        /// <summary>
        /// Event fired when an image finishes loading (successfully or with error).
        /// Parameters: url, sprite (null if failed), errorMessage
        /// </summary>
        public event Action<string, Sprite, string> OnImageLoaded;

        private void Awake() {
            base.Awake(persist: true, destroyComponentOnly: true);
        }

        protected override void OnInitialize() {
            diskCachePath = Path.Combine(Application.persistentDataPath, "ImageCache");
            if (enableDiskCache && !Directory.Exists(diskCachePath)) {
                Directory.CreateDirectory(diskCachePath);
            }
            CleanupOldCachedImages();
        }

        /// <summary>
        /// Loads an image from a URL. Returns cached version if available.
        /// </summary>
        public async Task<Sprite> LoadImageAsync(string url) {
            if (string.IsNullOrEmpty(url)) {
                Debug.LogError("[RemoteImageLoader] URL is null or empty.");
                OnImageLoaded?.Invoke(url, null, "Invalid URL");
                return null;
            }

            // Check memory cache first
            if (memoryCache.TryGetValue(url, out Sprite cachedSprite)) {
                OnImageLoaded?.Invoke(url, cachedSprite, null);
                return cachedSprite;
            }

            // Check disk cache
            if (enableDiskCache) {
                Sprite diskCachedSprite = LoadFromDiskCache(url);
                if (diskCachedSprite != null) {
                    AddToMemoryCache(url, diskCachedSprite);
                    OnImageLoaded?.Invoke(url, diskCachedSprite, null);
                    return diskCachedSprite;
                }
            }

            // Download from network
            return await DownloadImageAsync(url);
        }

        /// <summary>
        /// Loads an image and invokes a callback when complete.
        /// </summary>
        public void LoadImage(string url, Action<Sprite, string> callback) {
            LoadImageAsync(url).ContinueWith(task => {
                if (task.IsFaulted) {
                    callback?.Invoke(null, task.Exception?.Message);
                } else {
                    callback?.Invoke(task.Result, null);
                }
            });
        }

        async Task<Sprite> DownloadImageAsync(string url) {
            try {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url)) {
                    var operation = request.SendWebRequest();

                    while (!operation.isDone) {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success) {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        Sprite sprite = ConvertToSprite(texture);

                        AddToMemoryCache(url, sprite);

                        if (enableDiskCache) {
                            SaveToDiskCache(url, texture);
                        }

                        OnImageLoaded?.Invoke(url, sprite, null);
                        return sprite;
                    } else {
                        string error = $"Failed to download image: {request.error}";
                        Debug.LogError($"[RemoteImageLoader] {error}");
                        OnImageLoaded?.Invoke(url, null, error);
                        return null;
                    }
                }
            } catch (Exception ex) {
                Debug.LogError($"[RemoteImageLoader] Exception: {ex.Message}");
                OnImageLoaded?.Invoke(url, null, ex.Message);
                return null;
            }
        }

        Sprite ConvertToSprite(Texture2D texture) {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        void AddToMemoryCache(string url, Sprite sprite) {
            if (memoryCache.ContainsKey(url)) {
                return;
            }

            if (memoryCache.Count >= maxMemoryCacheSize) {
                // Remove oldest entry
                string oldestUrl = cacheQueue.Dequeue();
                if (memoryCache.ContainsKey(oldestUrl)) {
                    Destroy(memoryCache[oldestUrl].texture);
                    memoryCache.Remove(oldestUrl);
                }
            }

            memoryCache[url] = sprite;
            cacheQueue.Enqueue(url);
        }

        string GetCacheFilePath(string url) {
            string hash = GetUrlHash(url);
            return Path.Combine(diskCachePath, $"{hash}.png");
        }

        string GetUrlHash(string url) {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        void SaveToDiskCache(string url, Texture2D texture) {
            try {
                string filePath = GetCacheFilePath(url);
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
            } catch (Exception ex) {
                Debug.LogError($"[RemoteImageLoader] Failed to save to disk cache: {ex.Message}");
            }
        }

        Sprite LoadFromDiskCache(string url) {
            try {
                string filePath = GetCacheFilePath(url);
                if (File.Exists(filePath)) {
                    TimeSpan age = DateTime.Now - File.GetLastWriteTime(filePath);
                    if (age.TotalDays > diskCacheDurationDays) {
                        File.Delete(filePath);
                        return null;
                    }

                    byte[] bytes = File.ReadAllBytes(filePath);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(bytes)) {
                        return ConvertToSprite(texture);
                    }
                }
            } catch (Exception ex) {
                Debug.LogError($"[RemoteImageLoader] Failed to load from disk cache: {ex.Message}");
            }
            return null;
        }

        void CleanupOldCachedImages() {
            if (!enableDiskCache || !Directory.Exists(diskCachePath)) {
                return;
            }

            try {
                string[] files = Directory.GetFiles(diskCachePath);
                foreach (string file in files) {
                    TimeSpan age = DateTime.Now - File.GetLastWriteTime(file);
                    if (age.TotalDays > diskCacheDurationDays) {
                        File.Delete(file);
                    }
                }
            } catch (Exception ex) {
                Debug.LogError($"[RemoteImageLoader] Failed to cleanup cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all cached images from memory.
        /// </summary>
        public void ClearMemoryCache() {
            foreach (var sprite in memoryCache.Values) {
                if (sprite != null && sprite.texture != null) {
                    Destroy(sprite.texture);
                }
            }
            memoryCache.Clear();
            cacheQueue.Clear();
        }

        /// <summary>
        /// Clears all cached images from disk.
        /// </summary>
        public void ClearDiskCache() {
            if (!Directory.Exists(diskCachePath)) {
                return;
            }

            try {
                string[] files = Directory.GetFiles(diskCachePath);
                foreach (string file in files) {
                    File.Delete(file);
                }
            } catch (Exception ex) {
                Debug.LogError($"[RemoteImageLoader] Failed to clear disk cache: {ex.Message}");
            }
        }

        protected override void OnCleanup() {
            ClearMemoryCache();
        }
    }
}
