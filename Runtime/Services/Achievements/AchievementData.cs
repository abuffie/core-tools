using System;
using System.Collections.Generic;

namespace Aarware.Services.Achievements {
    /// <summary>
    /// Represents a single achievement.
    /// </summary>
    [Serializable]
    public class Achievement {
        public string achievementId;
        public string displayName;
        public string description;
        public string iconUrl;
        public bool isUnlocked;
        public bool isHidden;
        public float progress;
        public float maxProgress;
        public DateTime unlockedAt;

        public Achievement(string achievementId, string displayName, string description, float maxProgress = 1f, bool isHidden = false) {
            this.achievementId = achievementId;
            this.displayName = displayName;
            this.description = description;
            this.iconUrl = string.Empty;
            this.isUnlocked = false;
            this.isHidden = isHidden;
            this.progress = 0f;
            this.maxProgress = maxProgress;
            this.unlockedAt = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the completion percentage (0-100).
        /// </summary>
        public float GetProgressPercentage() {
            if (maxProgress <= 0) {
                return isUnlocked ? 100f : 0f;
            }
            return (progress / maxProgress) * 100f;
        }

        /// <summary>
        /// Checks if this is a progressive achievement (requires multiple steps).
        /// </summary>
        public bool IsProgressive() {
            return maxProgress > 1f;
        }
    }

    /// <summary>
    /// Container for all player achievements.
    /// </summary>
    [Serializable]
    public class PlayerAchievements {
        public Dictionary<string, Achievement> achievements;
        public DateTime lastUpdated;

        public PlayerAchievements() {
            achievements = new Dictionary<string, Achievement>();
            lastUpdated = DateTime.Now;
        }

        public void AddAchievement(Achievement achievement) {
            if (!achievements.ContainsKey(achievement.achievementId)) {
                achievements[achievement.achievementId] = achievement;
            }
        }

        public Achievement GetAchievement(string achievementId) {
            if (achievements.TryGetValue(achievementId, out Achievement achievement)) {
                return achievement;
            }
            return null;
        }

        public bool HasAchievement(string achievementId) {
            return achievements.ContainsKey(achievementId);
        }

        /// <summary>
        /// Gets all unlocked achievements.
        /// </summary>
        public List<Achievement> GetUnlockedAchievements() {
            List<Achievement> unlocked = new List<Achievement>();
            foreach (var achievement in achievements.Values) {
                if (achievement.isUnlocked) {
                    unlocked.Add(achievement);
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Gets completion percentage across all achievements.
        /// </summary>
        public float GetOverallCompletionPercentage() {
            if (achievements.Count == 0) {
                return 0f;
            }

            int unlockedCount = 0;
            foreach (var achievement in achievements.Values) {
                if (achievement.isUnlocked) {
                    unlockedCount++;
                }
            }

            return ((float)unlockedCount / achievements.Count) * 100f;
        }
    }
}
