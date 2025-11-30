using System.Collections.Generic;
using Aarware.Services;

namespace Aarware.Editor {
    /// <summary>
    /// Validates platform compatibility based on build target.
    /// Ensures that backend and networking platforms are supported by the selected build target.
    /// Editor-only validation for configuration UI.
    /// </summary>
    public static class PlatformValidator {
        /// <summary>
        /// Checks if a backend platform is valid for the given build target.
        /// </summary>
        public static bool IsBackendPlatformValid(DistributionPlatform buildTarget, BackendPlatform platform) {
            switch (buildTarget) {
                case DistributionPlatform.StandaloneWindows:
                case DistributionPlatform.StandaloneMac:
                case DistributionPlatform.StandaloneLinux:
                case DistributionPlatform.WebGL:
                case DistributionPlatform.Android:
                case DistributionPlatform.iOS:
                case DistributionPlatform.Amazon:
                    // Standalone builds: Only Local and Custom
                    return platform == BackendPlatform.Local || platform == BackendPlatform.Custom;

                case DistributionPlatform.SteamWindows:
                case DistributionPlatform.SteamMac:
                case DistributionPlatform.SteamLinux:
                    // Steam builds: Local, Custom, and Steam
                    return platform == BackendPlatform.Local ||
                           platform == BackendPlatform.Custom ||
                           platform == BackendPlatform.Steam;

                case DistributionPlatform.GooglePlay:
                    // Google Play: Local, Custom, and GooglePlay
                    return platform == BackendPlatform.Local ||
                           platform == BackendPlatform.Custom ||
                           platform == BackendPlatform.GooglePlay;

                case DistributionPlatform.WindowsStore:
                    // Windows Store: Local, Custom, and UniversalWindows
                    return platform == BackendPlatform.Local ||
                           platform == BackendPlatform.Custom ||
                           platform == BackendPlatform.UniversalWindows;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if a networking platform is valid for the given build target.
        /// </summary>
        public static bool IsNetworkingPlatformValid(DistributionPlatform buildTarget, NetworkingPlatform platform) {
            // Photon and Custom work on all platforms
            if (platform == NetworkingPlatform.Photon || platform == NetworkingPlatform.Custom || platform == NetworkingPlatform.Local) {
                return true;
            }

            // Steam networking only works on Steam builds
            if (platform == NetworkingPlatform.Steam) {
                return buildTarget == DistributionPlatform.SteamWindows ||
                       buildTarget == DistributionPlatform.SteamMac ||
                       buildTarget == DistributionPlatform.SteamLinux;
            }

            return false;
        }

        /// <summary>
        /// Gets all valid backend platforms for the given build target.
        /// </summary>
        public static List<BackendPlatform> GetValidBackendPlatforms(DistributionPlatform buildTarget) {
            List<BackendPlatform> validPlatforms = new List<BackendPlatform>();

            // All platforms support Local and Custom
            validPlatforms.Add(BackendPlatform.Local);
            validPlatforms.Add(BackendPlatform.Custom);

            // Add platform-specific options
            switch (buildTarget) {
                case DistributionPlatform.SteamWindows:
                case DistributionPlatform.SteamMac:
                case DistributionPlatform.SteamLinux:
                    validPlatforms.Add(BackendPlatform.Steam);
                    break;

                case DistributionPlatform.GooglePlay:
                    validPlatforms.Add(BackendPlatform.GooglePlay);
                    break;

                case DistributionPlatform.WindowsStore:
                    validPlatforms.Add(BackendPlatform.UniversalWindows);
                    break;
            }

            return validPlatforms;
        }

        /// <summary>
        /// Gets all valid networking platforms for the given build target.
        /// </summary>
        public static List<NetworkingPlatform> GetValidNetworkingPlatforms(DistributionPlatform buildTarget) {
            List<NetworkingPlatform> validPlatforms = new List<NetworkingPlatform> {
                NetworkingPlatform.Local,
                NetworkingPlatform.Custom,
                NetworkingPlatform.Photon
            };

            // Add Steam networking for Steam builds
            if (buildTarget == DistributionPlatform.SteamWindows ||
                buildTarget == DistributionPlatform.SteamMac ||
                buildTarget == DistributionPlatform.SteamLinux) {
                validPlatforms.Add(NetworkingPlatform.Steam);
            }

            return validPlatforms;
        }

        /// <summary>
        /// Gets a validation error message for an invalid backend platform.
        /// </summary>
        public static string GetValidationError(DistributionPlatform buildTarget, BackendPlatform platform) {
            if (IsBackendPlatformValid(buildTarget, platform)) {
                return string.Empty;
            }

            switch (platform) {
                case BackendPlatform.Steam:
                    return $"Steam backend requires a Steam build target (SteamWindows, SteamMac, or SteamLinux). Current target: {buildTarget}";

                case BackendPlatform.GooglePlay:
                    return $"Google Play backend requires GooglePlay build target. Current target: {buildTarget}";

                case BackendPlatform.UniversalWindows:
                    return $"Universal Windows backend requires WindowsStore build target. Current target: {buildTarget}";

                default:
                    return $"{platform} is not supported for build target {buildTarget}. Use Local or Custom instead.";
            }
        }

        /// <summary>
        /// Gets a validation error message for an invalid networking platform.
        /// </summary>
        public static string GetValidationError(DistributionPlatform buildTarget, NetworkingPlatform platform) {
            if (IsNetworkingPlatformValid(buildTarget, platform)) {
                return string.Empty;
            }

            if (platform == NetworkingPlatform.Steam) {
                return $"Steam networking requires a Steam build target (SteamWindows, SteamMac, or SteamLinux). Current target: {buildTarget}";
            }

            return $"{platform} is not supported for build target {buildTarget}.";
        }

        /// <summary>
        /// Gets a user-friendly description of which platforms are available for the build target.
        /// </summary>
        public static string GetBuildTargetInfo(DistributionPlatform buildTarget) {
            switch (buildTarget) {
                case DistributionPlatform.StandaloneWindows:
                    return "Standalone Windows build. Backend: Local, Custom. Networking: Local, Custom, Photon.";

                case DistributionPlatform.StandaloneMac:
                    return "Standalone Mac build. Backend: Local, Custom. Networking: Local, Custom, Photon.";

                case DistributionPlatform.StandaloneLinux:
                    return "Standalone Linux build. Backend: Local, Custom. Networking: Local, Custom, Photon.";

                case DistributionPlatform.WebGL:
                    return "WebGL browser build. Backend: Local, Custom. Networking: Local, Custom, Photon. Limited file access.";

                case DistributionPlatform.Android:
                    return "Standard Android build. Backend: Local, Custom. Networking: Local, Custom, Photon.";

                case DistributionPlatform.iOS:
                    return "iOS build. Backend: Local, Custom. Networking: Local, Custom, Photon.";

                case DistributionPlatform.SteamWindows:
                    return "Steam on Windows. Backend: Local, Custom, Steam. Networking: Local, Custom, Photon, Steam. Requires Steamworks.NET.";

                case DistributionPlatform.SteamMac:
                    return "Steam on Mac. Backend: Local, Custom, Steam. Networking: Local, Custom, Photon, Steam. Requires Steamworks.NET.";

                case DistributionPlatform.SteamLinux:
                    return "Steam on Linux. Backend: Local, Custom, Steam. Networking: Local, Custom, Photon, Steam. Requires Steamworks.NET.";

                case DistributionPlatform.GooglePlay:
                    return "Google Play on Android. Backend: Local, Custom, GooglePlay. Networking: Local, Custom, Photon. Requires Google Play Services SDK.";

                case DistributionPlatform.WindowsStore:
                    return "Windows Store (UWP). Backend: Local, Custom, UniversalWindows. Networking: Local, Custom, Photon. Requires Microsoft GDK.";

                case DistributionPlatform.Amazon:
                    return "Amazon Appstore on Android. Backend: Local, Custom. Networking: Local, Custom, Photon.";

                default:
                    return "Unknown build target.";
            }
        }
    }
}
