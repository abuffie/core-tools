using UnityEngine;

namespace Aarware.Utilities {
    /// <summary>
    /// Abstract singleton class for MonoBehaviour.
    /// Ensures only one instance exists and provides global access point.
    /// Can be placed in scene manually or will auto-create when first accessed.
    /// If multiple instances exist in scene, duplicates are automatically destroyed.
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        static T instance;
        static readonly object lockObject = new object();
        static bool applicationIsQuitting;

        /// <summary>
        /// Gets the singleton instance. Creates one if it doesn't exist.
        /// </summary>
        public static T Instance {
            get {
                if (applicationIsQuitting) {
                    Debug.LogWarning($"[MonoSingleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock (lockObject) {
                    if (instance == null) {
                        GameObject singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                        instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                        Debug.Log($"[MonoSingleton] Created new instance of {typeof(T).Name}");
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// Checks if an instance exists without creating one.
        /// </summary>
        public static bool HasInstance {
            get { return instance != null; }
        }

        /// <summary>
        /// Singleton initialization. Call this from your Awake() implementation.
        /// </summary>
        /// <param name="persist">If true, calls DontDestroyOnLoad. Default: false (scene-specific)</param>
        /// <param name="destroyComponentOnly">If true, destroys only the component on duplicates. Default: false (destroys GameObject)</param>
        protected void Awake(bool persist = false, bool destroyComponentOnly = false) {
            if (instance == null) {
                instance = this as T;

                if (persist) {
                    DontDestroyOnLoad(gameObject);
                }

                OnInitialize();
            } else if (instance != this) {
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T).Name} detected. Destroying.");

                if (destroyComponentOnly) {
                    Destroy(this);
                } else {
                    Destroy(gameObject);
                }
            }
        }

        protected virtual void OnDestroy() {
            if (instance == this) {
                applicationIsQuitting = true;
                OnCleanup();
            }
        }

        /// <summary>
        /// Called when the singleton is first initialized.
        /// Override to add custom initialization logic.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Called when the singleton is destroyed.
        /// Override to add custom cleanup logic.
        /// </summary>
        protected virtual void OnCleanup() { }
    }
}
