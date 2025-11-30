using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.Achievements {
    /// <summary>
    /// Service for managing achievements across different platforms.
    /// </summary>
    public class AchievementsService : ServiceBase<IAchievementsProvider> {
        /// <summary>
        /// Event fired when an achievement is unlocked.
        /// </summary>
        public event Action<Achievement> OnAchievementUnlocked;

        /// <summary>
        /// Event fired when achievement progress is updated.
        /// </summary>
        public event Action<string, float> OnAchievementProgress;

        /// <summary>
        /// Event fired when achievements are synced.
        /// </summary>
        public event Action OnAchievementsSynced;

        public override void SetProvider(IAchievementsProvider provider) {
            // Unsubscribe from old provider
            if (currentProvider != null) {
                currentProvider.OnAchievementUnlocked -= HandleAchievementUnlocked;
                currentProvider.OnAchievementProgress -= HandleAchievementProgress;
                currentProvider.OnAchievementsSynced -= HandleAchievementsSynced;
            }

            base.SetProvider(provider);

            // Subscribe to new provider
            if (currentProvider != null) {
                currentProvider.OnAchievementUnlocked += HandleAchievementUnlocked;
                currentProvider.OnAchievementProgress += HandleAchievementProgress;
                currentProvider.OnAchievementsSynced += HandleAchievementsSynced;
            }
        }

        /// <summary>
        /// Defines the achievements that can be unlocked.
        /// </summary>
        public void DefineAchievements(List<Achievement> achievementDefinitions) {
            if (!IsInitialized) {
                Debug.LogWarning("[AchievementsService] Service not initialized.");
                return;
            }

            currentProvider.DefineAchievements(achievementDefinitions);
        }

        /// <summary>
        /// Unlocks an achievement.
        /// </summary>
        public async Task<ServiceResult> UnlockAchievementAsync(string achievementId) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Achievements service not initialized");
            }

            return await currentProvider.UnlockAchievementAsync(achievementId);
        }

        /// <summary>
        /// Sets progress for a progressive achievement.
        /// </summary>
        public async Task<ServiceResult> SetAchievementProgressAsync(string achievementId, float progress) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Achievements service not initialized");
            }

            return await currentProvider.SetAchievementProgressAsync(achievementId, progress);
        }

        /// <summary>
        /// Increments progress for a progressive achievement.
        /// </summary>
        public async Task<ServiceResult> IncrementAchievementProgressAsync(string achievementId, float incrementBy = 1f) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Achievements service not initialized");
            }

            return await currentProvider.IncrementAchievementProgressAsync(achievementId, incrementBy);
        }

        /// <summary>
        /// Gets a specific achievement.
        /// </summary>
        public async Task<ServiceResult<Achievement>> GetAchievementAsync(string achievementId) {
            if (!IsInitialized) {
                return ServiceResult<Achievement>.Failed("Achievements service not initialized");
            }

            return await currentProvider.GetAchievementAsync(achievementId);
        }

        /// <summary>
        /// Gets all achievements.
        /// </summary>
        public async Task<ServiceResult<PlayerAchievements>> GetAllAchievementsAsync() {
            if (!IsInitialized) {
                return ServiceResult<PlayerAchievements>.Failed("Achievements service not initialized");
            }

            return await currentProvider.GetAllAchievementsAsync();
        }

        /// <summary>
        /// Checks if an achievement is unlocked.
        /// </summary>
        public async Task<bool> IsAchievementUnlockedAsync(string achievementId) {
            if (!IsInitialized) {
                return false;
            }

            return await currentProvider.IsAchievementUnlockedAsync(achievementId);
        }

        /// <summary>
        /// Resets all achievements.
        /// </summary>
        public async Task<ServiceResult> ResetAllAchievementsAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Achievements service not initialized");
            }

            return await currentProvider.ResetAllAchievementsAsync();
        }

        /// <summary>
        /// Syncs achievements with cloud/server.
        /// </summary>
        public async Task<ServiceResult> SyncAchievementsAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Achievements service not initialized");
            }

            return await currentProvider.SyncAchievementsAsync();
        }

        void HandleAchievementUnlocked(Achievement achievement) {
            Debug.Log($"[AchievementsService] Achievement unlocked: {achievement.displayName}");
            OnAchievementUnlocked?.Invoke(achievement);
        }

        void HandleAchievementProgress(string achievementId, float progress) {
            Debug.Log($"[AchievementsService] Achievement progress: {achievementId} = {progress}");
            OnAchievementProgress?.Invoke(achievementId, progress);
        }

        void HandleAchievementsSynced() {
            Debug.Log("[AchievementsService] Achievements synced");
            OnAchievementsSynced?.Invoke();
        }

        public override void Shutdown() {
            if (currentProvider != null) {
                currentProvider.OnAchievementUnlocked -= HandleAchievementUnlocked;
                currentProvider.OnAchievementProgress -= HandleAchievementProgress;
                currentProvider.OnAchievementsSynced -= HandleAchievementsSynced;
            }
            base.Shutdown();
        }
    }
}
