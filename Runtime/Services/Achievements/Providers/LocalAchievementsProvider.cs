using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Aarware.Utilities;

namespace Aarware.Services.Achievements {
    /// <summary>
    /// Local achievements provider that stores achievement data in JSON files.
    /// Desktop/Mobile: Stores in Application.persistentDataPath
    /// WebGL: Falls back to PlayerPrefs (browser IndexedDB)
    /// </summary>
    public class LocalAchievementsProvider : IAchievementsProvider {
        const string ACHIEVEMENTS_DATA_KEY = "Aarware_Achievements";

        PlayerAchievements playerAchievements;
        Dictionary<string, Achievement> achievementDefinitions;

        public BackendPlatform Platform => BackendPlatform.Local;
        public bool IsInitialized { get; private set; }

        public event Action<Achievement> OnAchievementUnlocked;
        public event Action<string, float> OnAchievementProgress;
        public event Action OnAchievementsSynced;

        public async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                return true;
            }

            achievementDefinitions = new Dictionary<string, Achievement>();
            playerAchievements = new PlayerAchievements();

            // Load saved achievements if they exist
            if (LocalStorageHelper.HasData(ACHIEVEMENTS_DATA_KEY)) {
                try {
                    string json = LocalStorageHelper.LoadData(ACHIEVEMENTS_DATA_KEY);
                    if (!string.IsNullOrEmpty(json)) {
                        playerAchievements = JsonUtility.FromJson<PlayerAchievements>(json);
                    }
                } catch (Exception ex) {
                    Debug.LogError($"[LocalAchievementsProvider] Failed to load achievements: {ex.Message}");
                }
            }

            IsInitialized = true;
            await Task.CompletedTask;
            return true;
        }

        public void Shutdown() {
            SaveAchievements();
            IsInitialized = false;
        }

        public void DefineAchievements(List<Achievement> achievementDefs) {
            foreach (var achievementDef in achievementDefs) {
                achievementDefinitions[achievementDef.achievementId] = achievementDef;

                // Initialize achievement if it doesn't exist
                if (!playerAchievements.HasAchievement(achievementDef.achievementId)) {
                    playerAchievements.AddAchievement(new Achievement(
                        achievementDef.achievementId,
                        achievementDef.displayName,
                        achievementDef.description,
                        achievementDef.maxProgress,
                        achievementDef.isHidden
                    ));
                }
            }

            SaveAchievements();
        }

        public async Task<ServiceResult> UnlockAchievementAsync(string achievementId) {
            try {
                Achievement achievement = GetOrCreateAchievement(achievementId);

                if (achievement.isUnlocked) {
                    await Task.CompletedTask;
                    return ServiceResult.Successful();
                }

                achievement.isUnlocked = true;
                achievement.progress = achievement.maxProgress;
                achievement.unlockedAt = DateTime.Now;
                playerAchievements.lastUpdated = DateTime.Now;

                SaveAchievements();
                OnAchievementUnlocked?.Invoke(achievement);

                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult.Failed($"Failed to unlock achievement: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SetAchievementProgressAsync(string achievementId, float progress) {
            try {
                Achievement achievement = GetOrCreateAchievement(achievementId);

                if (achievement.isUnlocked) {
                    await Task.CompletedTask;
                    return ServiceResult.Successful();
                }

                achievement.progress = Mathf.Clamp(progress, 0f, achievement.maxProgress);
                playerAchievements.lastUpdated = DateTime.Now;

                OnAchievementProgress?.Invoke(achievementId, achievement.progress);

                // Check if achievement should be unlocked
                if (achievement.progress >= achievement.maxProgress) {
                    await UnlockAchievementAsync(achievementId);
                } else {
                    SaveAchievements();
                }

                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult.Failed($"Failed to set achievement progress: {ex.Message}");
            }
        }

        public async Task<ServiceResult> IncrementAchievementProgressAsync(string achievementId, float incrementBy = 1f) {
            Achievement achievement = GetOrCreateAchievement(achievementId);
            float newProgress = achievement.progress + incrementBy;
            return await SetAchievementProgressAsync(achievementId, newProgress);
        }

        public async Task<ServiceResult<Achievement>> GetAchievementAsync(string achievementId) {
            Achievement achievement = playerAchievements.GetAchievement(achievementId);
            if (achievement != null) {
                await Task.CompletedTask;
                return ServiceResult<Achievement>.Successful(achievement);
            }

            await Task.CompletedTask;
            return ServiceResult<Achievement>.Failed($"Achievement '{achievementId}' not found");
        }

        public async Task<ServiceResult<PlayerAchievements>> GetAllAchievementsAsync() {
            await Task.CompletedTask;
            return ServiceResult<PlayerAchievements>.Successful(playerAchievements);
        }

        public async Task<bool> IsAchievementUnlockedAsync(string achievementId) {
            Achievement achievement = playerAchievements.GetAchievement(achievementId);
            await Task.CompletedTask;
            return achievement != null && achievement.isUnlocked;
        }

        public async Task<ServiceResult> ResetAllAchievementsAsync() {
            foreach (var achievement in playerAchievements.achievements.Values) {
                achievement.isUnlocked = false;
                achievement.progress = 0f;
                achievement.unlockedAt = DateTime.MinValue;
            }

            SaveAchievements();
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        public async Task<ServiceResult> SyncAchievementsAsync() {
            // Local provider doesn't sync with cloud
            OnAchievementsSynced?.Invoke();
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        Achievement GetOrCreateAchievement(string achievementId) {
            Achievement achievement = playerAchievements.GetAchievement(achievementId);

            if (achievement == null) {
                // Check if we have a definition for this achievement
                if (achievementDefinitions.TryGetValue(achievementId, out Achievement definition)) {
                    achievement = new Achievement(
                        definition.achievementId,
                        definition.displayName,
                        definition.description,
                        definition.maxProgress,
                        definition.isHidden
                    );
                } else {
                    // Create a default achievement
                    achievement = new Achievement(achievementId, achievementId, "No description", 1f, false);
                }

                playerAchievements.AddAchievement(achievement);
            }

            return achievement;
        }

        void SaveAchievements() {
            try {
                string json = JsonUtility.ToJson(playerAchievements);
                LocalStorageHelper.SaveData(ACHIEVEMENTS_DATA_KEY, json);
            } catch (Exception ex) {
                Debug.LogError($"[LocalAchievementsProvider] Failed to save achievements: {ex.Message}");
            }
        }
    }
}
