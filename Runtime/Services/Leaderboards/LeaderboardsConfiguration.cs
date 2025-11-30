using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aarware.Services.Leaderboards {
    /// <summary>
    /// Configuration for game leaderboards.
    /// Defines all leaderboards available in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "LeaderboardsConfiguration", menuName = "Aarware/Leaderboards Configuration", order = 4)]
    public class LeaderboardsConfiguration : ScriptableObject {
        [Header("Platform Configuration")]
        [Tooltip("How leaderboards are stored: Local Only, Local+Cloud Sync, or Cloud Only")]
        public ServiceStorageMode storageMode = ServiceStorageMode.LocalWithCloudSync;

        [Tooltip("Cloud platform for syncing (when not using Local Only)")]
        public BackendPlatform cloudPlatform = BackendPlatform.Steam;

        [Header("Leaderboard Definitions")]
        [Tooltip("All leaderboards defined for this game")]
        public List<LeaderboardDefinition> leaderboards = new List<LeaderboardDefinition>();

        /// <summary>
        /// Gets a leaderboard definition by ID.
        /// </summary>
        public LeaderboardDefinition GetLeaderboard(string leaderboardId) {
            foreach (var leaderboard in leaderboards) {
                if (leaderboard.leaderboardId == leaderboardId) {
                    return leaderboard;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if a leaderboard exists.
        /// </summary>
        public bool HasLeaderboard(string leaderboardId) {
            return GetLeaderboard(leaderboardId) != null;
        }

        /// <summary>
        /// Adds a new leaderboard definition.
        /// </summary>
        public void AddLeaderboard(LeaderboardDefinition leaderboard) {
            if (!HasLeaderboard(leaderboard.leaderboardId)) {
                leaderboards.Add(leaderboard);
            }
        }

        /// <summary>
        /// Removes a leaderboard definition.
        /// </summary>
        public bool RemoveLeaderboard(string leaderboardId) {
            for (int i = 0; i < leaderboards.Count; i++) {
                if (leaderboards[i].leaderboardId == leaderboardId) {
                    leaderboards.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static LeaderboardsConfiguration CreateDefault() {
            LeaderboardsConfiguration config = CreateInstance<LeaderboardsConfiguration>();
            config.storageMode = ServiceStorageMode.LocalWithCloudSync;
            config.cloudPlatform = BackendPlatform.Steam;
            config.leaderboards = new List<LeaderboardDefinition>();
            return config;
        }
    }

    /// <summary>
    /// Defines a single leaderboard (design-time configuration).
    /// </summary>
    [Serializable]
    public class LeaderboardDefinition {
        [Tooltip("Unique identifier for this leaderboard")]
        public string leaderboardId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Sort order for scores")]
        public LeaderboardSortOrder sortOrder = LeaderboardSortOrder.Descending;

        [Tooltip("Maximum entries to display")]
        public int maxEntries = 100;

        [Tooltip("Score format (e.g., time, points, distance)")]
        public LeaderboardScoreFormat scoreFormat = LeaderboardScoreFormat.Numeric;

        [Tooltip("Description of this leaderboard")]
        [TextArea(2, 3)]
        public string description;

        public LeaderboardDefinition() {
            leaderboardId = "";
            displayName = "";
            sortOrder = LeaderboardSortOrder.Descending;
            maxEntries = 100;
            scoreFormat = LeaderboardScoreFormat.Numeric;
            description = "";
        }

        public LeaderboardDefinition(string id, string name, LeaderboardSortOrder order) {
            leaderboardId = id;
            displayName = name;
            sortOrder = order;
            maxEntries = 100;
            scoreFormat = LeaderboardScoreFormat.Numeric;
            description = "";
        }
    }

    /// <summary>
    /// How scores are displayed/formatted.
    /// </summary>
    public enum LeaderboardScoreFormat {
        Numeric,        // Regular number: 1000
        Time,           // Time format: 01:23:45
        TimeMilliseconds, // Time with ms: 01:23.456
        Distance,       // Distance: 1000m
        Currency        // Currency: $1000
    }
}
