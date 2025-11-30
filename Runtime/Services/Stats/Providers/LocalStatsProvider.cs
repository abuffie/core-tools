using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Aarware.Utilities;

namespace Aarware.Services.Stats {
    /// <summary>
    /// Local stats provider that stores statistics in JSON files.
    /// Desktop/Mobile: Stores in Application.persistentDataPath
    /// WebGL: Falls back to PlayerPrefs (browser IndexedDB)
    /// </summary>
    public class LocalStatsProvider : IStatsProvider {
        const string STATS_DATA_KEY = "Aarware_Stats";

        PlayerStats playerStats;
        Dictionary<string, Stat> statDefinitions;

        public BackendPlatform Platform => BackendPlatform.Local;
        public bool IsInitialized { get; private set; }

        public event Action<string, float> OnStatUpdated;
        public event Action OnStatsSynced;

        public async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                return true;
            }

            statDefinitions = new Dictionary<string, Stat>();
            playerStats = new PlayerStats();

            // Load saved stats if they exist
            if (LocalStorageHelper.HasData(STATS_DATA_KEY)) {
                try {
                    string json = LocalStorageHelper.LoadData(STATS_DATA_KEY);
                    if (!string.IsNullOrEmpty(json)) {
                        playerStats = JsonUtility.FromJson<PlayerStats>(json);
                    }
                } catch (Exception ex) {
                    Debug.LogError($"[LocalStatsProvider] Failed to load stats: {ex.Message}");
                }
            }

            IsInitialized = true;
            await Task.CompletedTask;
            return true;
        }

        public void Shutdown() {
            SaveStats();
            IsInitialized = false;
        }

        public void DefineStats(List<Stat> statDefs) {
            foreach (var statDef in statDefs) {
                statDefinitions[statDef.statId] = statDef;

                // Initialize stat if it doesn't exist
                if (!playerStats.HasStat(statDef.statId)) {
                    playerStats.AddStat(new Stat(statDef.statId, statDef.displayName, statDef.type, statDef.defaultValue));
                }
            }

            SaveStats();
        }

        public async Task<ServiceResult> SetStatAsync(string statId, float value) {
            try {
                Stat stat = GetOrCreateStat(statId);
                stat.value = value;
                playerStats.lastUpdated = DateTime.Now;

                SaveStats();
                OnStatUpdated?.Invoke(statId, value);

                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult.Failed($"Failed to set stat: {ex.Message}");
            }
        }

        public async Task<ServiceResult> IncrementStatAsync(string statId, float incrementBy = 1f) {
            try {
                Stat stat = GetOrCreateStat(statId);

                switch (stat.type) {
                    case StatType.Int:
                        stat.value += Mathf.RoundToInt(incrementBy);
                        break;
                    case StatType.Float:
                        stat.value += incrementBy;
                        break;
                    case StatType.Max:
                        if (incrementBy > stat.value) {
                            stat.value = incrementBy;
                        }
                        break;
                    case StatType.Min:
                        if (stat.value == 0 || incrementBy < stat.value) {
                            stat.value = incrementBy;
                        }
                        break;
                    default:
                        stat.value += incrementBy;
                        break;
                }

                playerStats.lastUpdated = DateTime.Now;

                SaveStats();
                OnStatUpdated?.Invoke(statId, stat.value);

                await Task.CompletedTask;
                return ServiceResult.Successful();
            } catch (Exception ex) {
                await Task.CompletedTask;
                return ServiceResult.Failed($"Failed to increment stat: {ex.Message}");
            }
        }

        public async Task<ServiceResult<float>> GetStatAsync(string statId) {
            Stat stat = playerStats.GetStat(statId);
            if (stat != null) {
                await Task.CompletedTask;
                return ServiceResult<float>.Successful(stat.value);
            }

            await Task.CompletedTask;
            return ServiceResult<float>.Failed($"Stat '{statId}' not found");
        }

        public async Task<ServiceResult<PlayerStats>> GetAllStatsAsync() {
            await Task.CompletedTask;
            return ServiceResult<PlayerStats>.Successful(playerStats);
        }

        public async Task<ServiceResult> ResetStatAsync(string statId) {
            Stat stat = playerStats.GetStat(statId);
            if (stat != null) {
                stat.value = stat.defaultValue;
                SaveStats();
                OnStatUpdated?.Invoke(statId, stat.value);

                await Task.CompletedTask;
                return ServiceResult.Successful();
            }

            await Task.CompletedTask;
            return ServiceResult.Failed($"Stat '{statId}' not found");
        }

        public async Task<ServiceResult> ResetAllStatsAsync() {
            foreach (var stat in playerStats.stats.Values) {
                stat.value = stat.defaultValue;
            }

            SaveStats();
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        public async Task<ServiceResult> SyncStatsAsync() {
            // Local provider doesn't sync with cloud
            OnStatsSynced?.Invoke();
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        Stat GetOrCreateStat(string statId) {
            Stat stat = playerStats.GetStat(statId);

            if (stat == null) {
                // Check if we have a definition for this stat
                if (statDefinitions.TryGetValue(statId, out Stat definition)) {
                    stat = new Stat(definition.statId, definition.displayName, definition.type, definition.defaultValue);
                } else {
                    // Create a default stat
                    stat = new Stat(statId, statId, StatType.Float, 0f);
                }

                playerStats.AddStat(stat);
            }

            return stat;
        }

        void SaveStats() {
            try {
                string json = JsonUtility.ToJson(playerStats);
                LocalStorageHelper.SaveData(STATS_DATA_KEY, json);
            } catch (Exception ex) {
                Debug.LogError($"[LocalStatsProvider] Failed to save stats: {ex.Message}");
            }
        }
    }
}
