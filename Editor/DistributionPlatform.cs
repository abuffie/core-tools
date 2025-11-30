namespace Aarware.Editor {
    /// <summary>
    /// Distribution/deployment platforms (Editor-only).
    /// Used for validating platform configurations in the editor.
    /// Determines which platform APIs and SDKs are available in the build.
    /// </summary>
    public enum DistributionPlatform {
        /// <summary>
        /// Standalone Windows build (no platform services).
        /// </summary>
        StandaloneWindows,

        /// <summary>
        /// Standalone Mac build (no platform services).
        /// </summary>
        StandaloneMac,

        /// <summary>
        /// Standalone Linux build (no platform services).
        /// </summary>
        StandaloneLinux,

        /// <summary>
        /// WebGL build (browser-based, limited APIs).
        /// </summary>
        WebGL,

        /// <summary>
        /// Standard Android build (no store services).
        /// </summary>
        Android,

        /// <summary>
        /// iOS build (no store services).
        /// </summary>
        iOS,

        /// <summary>
        /// Steam distribution on Windows (includes Steamworks SDK).
        /// </summary>
        SteamWindows,

        /// <summary>
        /// Steam distribution on Mac (includes Steamworks SDK).
        /// </summary>
        SteamMac,

        /// <summary>
        /// Steam distribution on Linux (includes Steamworks SDK).
        /// </summary>
        SteamLinux,

        /// <summary>
        /// Google Play distribution on Android (includes Google Play Services).
        /// </summary>
        GooglePlay,

        /// <summary>
        /// Windows Store/UWP distribution (includes Universal Windows APIs).
        /// </summary>
        WindowsStore,

        /// <summary>
        /// Amazon Appstore distribution on Android.
        /// </summary>
        Amazon,

        // Future platforms (not yet implemented):
        // PlayStation,
        // Xbox,
        // Switch
    }
}
