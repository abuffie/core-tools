namespace Aarware.Services {
    /// <summary>
    /// Defines how a service stores its data.
    /// Supports offline-only, cloud-only, or hybrid local+cloud sync.
    /// </summary>
    public enum ServiceStorageMode {
        /// <summary>
        /// Store data locally only. Works completely offline.
        /// </summary>
        LocalOnly,

        /// <summary>
        /// Store data locally with cloud sync when available.
        /// Best user experience - works offline and syncs when online.
        /// Follows spec: "always use local, if using a service keep local synced"
        /// </summary>
        LocalWithCloudSync,

        /// <summary>
        /// Store data only in the cloud. Requires connection to platform.
        /// No local fallback - if cloud is unavailable, service won't work.
        /// </summary>
        CloudOnly
    }
}
