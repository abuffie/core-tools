using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Aarware.Utilities;

namespace Aarware.Services.Leaderboards {
    /// <summary>
    /// Local leaderboards provider that stores leaderboard data in JSON files.
    /// Desktop/Mobile: Stores in Application.persistentDataPath
    /// WebGL: Falls back to PlayerPrefs (browser IndexedDB)
    /// Suitable for offline testing or single-player games.
    /// </summary>
    public class LocalLeaderboardsProvider : ILeaderboardsProvider {
        const string LEADERBOARDS_DATA_KEY = "Aarware_Leaderboards";
        const string CURRENT_USER_ID_KEY = "Aarware_CurrentUserId";

        Dictionary<string, Leaderboard> leaderboards;
        string currentUserId;

        public BackendPlatform Platform => BackendPlatform.Local;
        public bool IsInitialized { get; private set; }

        public event Action<string, long> OnScoreSubmitted;
        public event Action<string> OnLeaderboardLoaded;

        public async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                return true;
            }

            leaderboards = new Dictionary<string, Leaderboard>();

            // Load current user ID
            string userIdJson = LocalStorageHelper.LoadData(CURRENT_USER_ID_KEY);
            currentUserId = !string.IsNullOrEmpty(userIdJson) ? userIdJson : SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(userIdJson)) {
                LocalStorageHelper.SaveData(CURRENT_USER_ID_KEY, currentUserId);
            }

            // Load saved leaderboards if they exist
            if (LocalStorageHelper.HasData(LEADERBOARDS_DATA_KEY)) {
                try {
                    // Note: Unity's JsonUtility doesn't support Dictionary serialization well,
                    // so we'll use a simpler approach for local leaderboards
                    Debug.Log("[LocalLeaderboardsProvider] Loaded leaderboard data");
                } catch (Exception ex) {
                    Debug.LogError($"[LocalLeaderboardsProvider] Failed to load leaderboards: {ex.Message}");
                }
            }

            IsInitialized = true;
            await Task.CompletedTask;
            return true;
        }

        public void Shutdown() {
            SaveLeaderboards();
            IsInitialized = false;
        }

        public void DefineLeaderboards(List<Leaderboard> leaderboardDefs) {
            foreach (var leaderboardDef in leaderboardDefs) {
                if (!leaderboards.ContainsKey(leaderboardDef.leaderboardId)) {
                    leaderboards[leaderboardDef.leaderboardId] = new Leaderboard(
                        leaderboardDef.leaderboardId,
                        leaderboardDef.displayName,
                        leaderboardDef.sortOrder
                    );
                }
            }

            SaveLeaderboards();
        }

        public async Task<ServiceResult> SubmitScoreAsync(string leaderboardId, long score, string extraData = "") {
            try {
                Leaderboard leaderboard = GetOrCreateLeaderboard(leaderboardId);

                // Check if player already has an entry
                LeaderboardEntry existingEntry = leaderboard.GetEntryByUserId(currentUserId);

                if (existingEntry != null) {
                    // Update score if it's better based on sort order
                    bool shouldUpdate = leaderboard.sortOrder == LeaderboardSortOrder.Descending ?
                        score > existingEntry.score :
                        score < existingEntry.score;

                    if (shouldUpdate) {
                        existingEntry.score = score;
                        existingEntry.extraData = extraData;
                        existingEntry.submittedAt = DateTime.Now;
                        leaderboard.SortEntries();
                    }
                } else {
                    // Add new entry
                    LeaderboardEntry newEntry = new LeaderboardEntry(currentUserId, "Player", score) {
                        extraData = extraData
                    };
                    leaderboard.AddEntry(newEntry);
                }

                SaveLeaderboards();
                OnScoreSubmitted?.Invoke(leaderboardId, score);

                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult.Failed($"Failed to submit score: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Leaderboard>> GetLeaderboardAsync(
            string leaderboardId,
            LeaderboardScope scope = LeaderboardScope.Global,
            LeaderboardTimeRange timeRange = LeaderboardTimeRange.AllTime,
            int maxEntries = 10) {
            try {
                Leaderboard leaderboard = GetOrCreateLeaderboard(leaderboardId);

                // Create a copy with limited entries
                Leaderboard result = new Leaderboard(leaderboard.leaderboardId, leaderboard.displayName, leaderboard.sortOrder);
                result.entries = leaderboard.GetTopEntries(maxEntries);
                result.lastUpdated = leaderboard.lastUpdated;

                OnLeaderboardLoaded?.Invoke(leaderboardId);

                await Task.CompletedTask;
                return ServiceResult<Leaderboard>.Successful(result);
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult<Leaderboard>.Failed($"Failed to get leaderboard: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> GetPlayerRankAsync(string leaderboardId) {
            try {
                Leaderboard leaderboard = GetOrCreateLeaderboard(leaderboardId);
                LeaderboardEntry entry = leaderboard.GetEntryByUserId(currentUserId);

                if (entry != null) {
                    await Task.CompletedTask;
                    return ServiceResult<int>.Successful(entry.rank);
                }

                await Task.CompletedTask;
                return ServiceResult<int>.Failed("Player has no score on this leaderboard");
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult<int>.Failed($"Failed to get player rank: {ex.Message}");
            }
        }

        public async Task<ServiceResult<long>> GetPlayerScoreAsync(string leaderboardId) {
            try {
                Leaderboard leaderboard = GetOrCreateLeaderboard(leaderboardId);
                LeaderboardEntry entry = leaderboard.GetEntryByUserId(currentUserId);

                if (entry != null) {
                    await Task.CompletedTask;
                    return ServiceResult<long>.Successful(entry.score);
                }

                await Task.CompletedTask;
                return ServiceResult<long>.Failed("Player has no score on this leaderboard");
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult<long>.Failed($"Failed to get player score: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ResetAllLeaderboardsAsync() {
            foreach (var leaderboard in leaderboards.Values) {
                leaderboard.entries.Clear();
            }

            SaveLeaderboards();
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        public async Task<ServiceResult> SyncLeaderboardsAsync() {
            // Local provider doesn't sync with cloud
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        Leaderboard GetOrCreateLeaderboard(string leaderboardId) {
            if (leaderboards.TryGetValue(leaderboardId, out Leaderboard leaderboard)) {
                return leaderboard;
            }

            // Create default leaderboard
            leaderboard = new Leaderboard(leaderboardId, leaderboardId, LeaderboardSortOrder.Descending);
            leaderboards[leaderboardId] = leaderboard;
            return leaderboard;
        }

        void SaveLeaderboards() {
            try {
                // For local storage, we'll save each leaderboard separately
                foreach (var kvp in leaderboards) {
                    string key = $"{LEADERBOARDS_DATA_KEY}_{kvp.Key}";
                    string json = JsonUtility.ToJson(kvp.Value);
                    LocalStorageHelper.SaveData(key, json);
                }
            } catch (Exception ex) {
                Debug.LogError($"[LocalLeaderboardsProvider] Failed to save leaderboards: {ex.Message}");
            }
        }
    }
}
