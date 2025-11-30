using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.Leaderboards {
    /// <summary>
    /// Service for managing leaderboards across different platforms.
    /// </summary>
    public class LeaderboardsService : ServiceBase<ILeaderboardsProvider> {
        /// <summary>
        /// Event fired when a score is successfully submitted.
        /// </summary>
        public event Action<string, long> OnScoreSubmitted;

        /// <summary>
        /// Event fired when leaderboard data is loaded.
        /// </summary>
        public event Action<string> OnLeaderboardLoaded;

        public override void SetProvider(ILeaderboardsProvider provider) {
            // Unsubscribe from old provider
            if (currentProvider != null) {
                currentProvider.OnScoreSubmitted -= HandleScoreSubmitted;
                currentProvider.OnLeaderboardLoaded -= HandleLeaderboardLoaded;
            }

            base.SetProvider(provider);

            // Subscribe to new provider
            if (currentProvider != null) {
                currentProvider.OnScoreSubmitted += HandleScoreSubmitted;
                currentProvider.OnLeaderboardLoaded += HandleLeaderboardLoaded;
            }
        }

        /// <summary>
        /// Defines the leaderboards that will be used.
        /// </summary>
        public void DefineLeaderboards(List<Leaderboard> leaderboardDefinitions) {
            if (!IsInitialized) {
                Debug.LogWarning("[LeaderboardsService] Service not initialized.");
                return;
            }

            currentProvider.DefineLeaderboards(leaderboardDefinitions);
        }

        /// <summary>
        /// Submits a score to a leaderboard.
        /// </summary>
        public async Task<ServiceResult> SubmitScoreAsync(string leaderboardId, long score, string extraData = "") {
            if (!IsInitialized) {
                return ServiceResult.Failed("Leaderboards service not initialized");
            }

            return await currentProvider.SubmitScoreAsync(leaderboardId, score, extraData);
        }

        /// <summary>
        /// Gets leaderboard entries.
        /// </summary>
        public async Task<ServiceResult<Leaderboard>> GetLeaderboardAsync(
            string leaderboardId,
            LeaderboardScope scope = LeaderboardScope.Global,
            LeaderboardTimeRange timeRange = LeaderboardTimeRange.AllTime,
            int maxEntries = 10) {
            if (!IsInitialized) {
                return ServiceResult<Leaderboard>.Failed("Leaderboards service not initialized");
            }

            return await currentProvider.GetLeaderboardAsync(leaderboardId, scope, timeRange, maxEntries);
        }

        /// <summary>
        /// Gets the player's rank on a leaderboard.
        /// </summary>
        public async Task<ServiceResult<int>> GetPlayerRankAsync(string leaderboardId) {
            if (!IsInitialized) {
                return ServiceResult<int>.Failed("Leaderboards service not initialized");
            }

            return await currentProvider.GetPlayerRankAsync(leaderboardId);
        }

        /// <summary>
        /// Gets the player's score on a leaderboard.
        /// </summary>
        public async Task<ServiceResult<long>> GetPlayerScoreAsync(string leaderboardId) {
            if (!IsInitialized) {
                return ServiceResult<long>.Failed("Leaderboards service not initialized");
            }

            return await currentProvider.GetPlayerScoreAsync(leaderboardId);
        }

        /// <summary>
        /// Resets all leaderboards.
        /// </summary>
        public async Task<ServiceResult> ResetAllLeaderboardsAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Leaderboards service not initialized");
            }

            return await currentProvider.ResetAllLeaderboardsAsync();
        }

        /// <summary>
        /// Syncs leaderboards with cloud/server.
        /// </summary>
        public async Task<ServiceResult> SyncLeaderboardsAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Leaderboards service not initialized");
            }

            return await currentProvider.SyncLeaderboardsAsync();
        }

        void HandleScoreSubmitted(string leaderboardId, long score) {
            Debug.Log($"[LeaderboardsService] Score submitted to {leaderboardId}: {score}");
            OnScoreSubmitted?.Invoke(leaderboardId, score);
        }

        void HandleLeaderboardLoaded(string leaderboardId) {
            Debug.Log($"[LeaderboardsService] Leaderboard loaded: {leaderboardId}");
            OnLeaderboardLoaded?.Invoke(leaderboardId);
        }

        public override void Shutdown() {
            if (currentProvider != null) {
                currentProvider.OnScoreSubmitted -= HandleScoreSubmitted;
                currentProvider.OnLeaderboardLoaded -= HandleLeaderboardLoaded;
            }
            base.Shutdown();
        }
    }
}
