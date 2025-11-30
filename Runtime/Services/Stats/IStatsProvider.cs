using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aarware.Services.Stats {
    /// <summary>
    /// Interface for platform-specific stats providers.
    /// </summary>
    public interface IStatsProvider : IServiceProvider {
        /// <summary>
        /// The backend platform this provider supports.
        /// </summary>
        BackendPlatform Platform { get; }

        /// <summary>
        /// Event fired when a stat is updated.
        /// </summary>
        event Action<string, float> OnStatUpdated;

        /// <summary>
        /// Event fired when stats are synced with server/cloud.
        /// </summary>
        event Action OnStatsSynced;

        /// <summary>
        /// Sets a stat value (overwrites existing value).
        /// </summary>
        Task<ServiceResult> SetStatAsync(string statId, float value);

        /// <summary>
        /// Increments a stat value by the specified amount.
        /// </summary>
        Task<ServiceResult> IncrementStatAsync(string statId, float incrementBy = 1f);

        /// <summary>
        /// Gets the current value of a stat.
        /// </summary>
        Task<ServiceResult<float>> GetStatAsync(string statId);

        /// <summary>
        /// Gets all stats for the current player.
        /// </summary>
        Task<ServiceResult<PlayerStats>> GetAllStatsAsync();

        /// <summary>
        /// Resets a stat to its default value.
        /// </summary>
        Task<ServiceResult> ResetStatAsync(string statId);

        /// <summary>
        /// Resets all stats to their default values.
        /// </summary>
        Task<ServiceResult> ResetAllStatsAsync();

        /// <summary>
        /// Defines available stats (should be called during initialization).
        /// </summary>
        void DefineStats(List<Stat> statDefinitions);

        /// <summary>
        /// Syncs local stats with cloud/server.
        /// </summary>
        Task<ServiceResult> SyncStatsAsync();
    }
}
