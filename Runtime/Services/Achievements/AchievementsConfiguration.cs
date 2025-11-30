using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aarware.Services.Achievements {
    /// <summary>
    /// Configuration for game achievements.
    /// Defines all achievements available in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementsConfiguration", menuName = "Aarware/Achievements Configuration", order = 3)]
    public class AchievementsConfiguration : ScriptableObject {
        [Header("Platform Configuration")]
        [Tooltip("How achievements are stored: Local Only, Local+Cloud Sync, or Cloud Only")]
        public ServiceStorageMode storageMode = ServiceStorageMode.LocalWithCloudSync;

        [Tooltip("Cloud platform for syncing (when not using Local Only)")]
        public BackendPlatform cloudPlatform = BackendPlatform.Steam;

        [Header("Achievement Definitions")]
        [Tooltip("All achievements defined for this game")]
        public List<AchievementDefinition> achievements = new List<AchievementDefinition>();

        /// <summary>
        /// Gets an achievement definition by ID.
        /// </summary>
        public AchievementDefinition GetAchievement(string achievementId) {
            foreach (var achievement in achievements) {
                if (achievement.achievementId == achievementId) {
                    return achievement;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if an achievement exists.
        /// </summary>
        public bool HasAchievement(string achievementId) {
            return GetAchievement(achievementId) != null;
        }

        /// <summary>
        /// Adds a new achievement definition.
        /// </summary>
        public void AddAchievement(AchievementDefinition achievement) {
            if (!HasAchievement(achievement.achievementId)) {
                achievements.Add(achievement);
            }
        }

        /// <summary>
        /// Removes an achievement definition.
        /// </summary>
        public bool RemoveAchievement(string achievementId) {
            for (int i = 0; i < achievements.Count; i++) {
                if (achievements[i].achievementId == achievementId) {
                    achievements.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static AchievementsConfiguration CreateDefault() {
            AchievementsConfiguration config = CreateInstance<AchievementsConfiguration>();
            config.storageMode = ServiceStorageMode.LocalWithCloudSync;
            config.cloudPlatform = BackendPlatform.Steam;
            config.achievements = new List<AchievementDefinition>();
            return config;
        }
    }

    /// <summary>
    /// Defines a single achievement (design-time configuration).
    /// </summary>
    [Serializable]
    public class AchievementDefinition {
        [Tooltip("Unique identifier for this achievement")]
        public string achievementId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Description of how to unlock")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("URL or resource path for achievement icon")]
        public string iconPath;

        [Tooltip("Whether this achievement is hidden until unlocked")]
        public bool isHidden;

        [Tooltip("Whether this is a progressive achievement")]
        public bool isProgressive;

        [Tooltip("Max progress needed to unlock (for progressive achievements)")]
        public float maxProgress = 1f;

        [Tooltip("Point value for this achievement")]
        public int pointValue;

        public AchievementDefinition() {
            achievementId = "";
            displayName = "";
            description = "";
            iconPath = "";
            isHidden = false;
            isProgressive = false;
            maxProgress = 1f;
            pointValue = 10;
        }

        public AchievementDefinition(string id, string name, string desc, float maxProg = 1f, bool hidden = false) {
            achievementId = id;
            displayName = name;
            description = desc;
            iconPath = "";
            isHidden = hidden;
            isProgressive = maxProg > 1f;
            maxProgress = maxProg;
            pointValue = 10;
        }
    }
}
