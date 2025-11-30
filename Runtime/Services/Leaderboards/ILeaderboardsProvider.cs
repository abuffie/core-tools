using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aarware.Services.Leaderboards {
    /// <summary>
    /// Interface for platform-specific leaderboard providers.
    /// </summary>
    public interface ILeaderboardsProvider : IServiceProvider {
        /// <summary>
        /// The backend platform this provider supports.
        /// </summary>
        BackendPlatform Platform { get; }

        /// <summary>
        /// Event fired when a score is successfully submitted.
        /// </summary>
        event Action<string, long> OnScoreSubmitted;

        /// <summary>
        /// Event fired when leaderboard data is loaded.
        /// </summary>
        event Action<string> OnLeaderboardLoaded;

        /// <summary>
        /// Submits a score to a leaderboard.
        /// </summary>
        Task<ServiceResult> SubmitScoreAsync(string leaderboardId, long score, string extraData = "");

        /// <summary>
        /// Gets leaderboard entries.
        /// </summary>
        Task<ServiceResult<Leaderboard>> GetLeaderboardAsync(
            string leaderboardId,
            LeaderboardScope scope = LeaderboardScope.Global,
            LeaderboardTimeRange timeRange = LeaderboardTimeRange.AllTime,
            int maxEntries = 10
        );

        /// <summary>
        /// Gets the player's rank on a leaderboard.
        /// </summary>
        Task<ServiceResult<int>> GetPlayerRankAsync(string leaderboardId);

        /// <summary>
        /// Gets the player's score on a leaderboard.
        /// </summary>
        Task<ServiceResult<long>> GetPlayerScoreAsync(string leaderboardId);

        /// <summary>
        /// Defines available leaderboards (should be called during initialization).
        /// </summary>
        void DefineLeaderboards(List<Leaderboard> leaderboardDefinitions);

        /// <summary>
        /// Resets all leaderboards (for testing).
        /// </summary>
        Task<ServiceResult> ResetAllLeaderboardsAsync();

        /// <summary>
        /// Syncs leaderboards with cloud/server.
        /// </summary>
        Task<ServiceResult> SyncLeaderboardsAsync();
    }
}
