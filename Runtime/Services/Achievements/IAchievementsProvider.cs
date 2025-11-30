using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aarware.Services.Achievements {
    /// <summary>
    /// Interface for platform-specific achievements providers.
    /// </summary>
    public interface IAchievementsProvider : IServiceProvider {
        /// <summary>
        /// The backend platform this provider supports.
        /// </summary>
        BackendPlatform Platform { get; }

        /// <summary>
        /// Event fired when an achievement is unlocked.
        /// </summary>
        event Action<Achievement> OnAchievementUnlocked;

        /// <summary>
        /// Event fired when achievement progress is updated.
        /// </summary>
        event Action<string, float> OnAchievementProgress;

        /// <summary>
        /// Event fired when achievements are synced with server.
        /// </summary>
        event Action OnAchievementsSynced;

        /// <summary>
        /// Unlocks an achievement.
        /// </summary>
        Task<ServiceResult> UnlockAchievementAsync(string achievementId);

        /// <summary>
        /// Sets progress for a progressive achievement.
        /// </summary>
        Task<ServiceResult> SetAchievementProgressAsync(string achievementId, float progress);

        /// <summary>
        /// Increments progress for a progressive achievement.
        /// </summary>
        Task<ServiceResult> IncrementAchievementProgressAsync(string achievementId, float incrementBy = 1f);

        /// <summary>
        /// Gets a specific achievement.
        /// </summary>
        Task<ServiceResult<Achievement>> GetAchievementAsync(string achievementId);

        /// <summary>
        /// Gets all achievements.
        /// </summary>
        Task<ServiceResult<PlayerAchievements>> GetAllAchievementsAsync();

        /// <summary>
        /// Checks if an achievement is unlocked.
        /// </summary>
        Task<bool> IsAchievementUnlockedAsync(string achievementId);

        /// <summary>
        /// Defines available achievements (should be called during initialization).
        /// </summary>
        void DefineAchievements(List<Achievement> achievementDefinitions);

        /// <summary>
        /// Resets all achievements (for testing).
        /// </summary>
        Task<ServiceResult> ResetAllAchievementsAsync();

        /// <summary>
        /// Syncs achievements with cloud/server.
        /// </summary>
        Task<ServiceResult> SyncAchievementsAsync();
    }
}
