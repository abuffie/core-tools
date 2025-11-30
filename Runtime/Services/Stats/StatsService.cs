using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.Stats {
    /// <summary>
    /// Service for managing player statistics across different platforms.
    /// </summary>
    public class StatsService : ServiceBase<IStatsProvider> {
        /// <summary>
        /// Event fired when a stat is updated.
        /// </summary>
        public event Action<string, float> OnStatUpdated;

        /// <summary>
        /// Event fired when stats are synced.
        /// </summary>
        public event Action OnStatsSynced;

        public override void SetProvider(IStatsProvider provider) {
            // Unsubscribe from old provider
            if (currentProvider != null) {
                currentProvider.OnStatUpdated -= HandleStatUpdated;
                currentProvider.OnStatsSynced -= HandleStatsSynced;
            }

            base.SetProvider(provider);

            // Subscribe to new provider
            if (currentProvider != null) {
                currentProvider.OnStatUpdated += HandleStatUpdated;
                currentProvider.OnStatsSynced += HandleStatsSynced;
            }
        }

        /// <summary>
        /// Defines the stats that will be tracked.
        /// </summary>
        public void DefineStats(List<Stat> statDefinitions) {
            if (!IsInitialized) {
                Debug.LogWarning("[StatsService] Service not initialized.");
                return;
            }

            currentProvider.DefineStats(statDefinitions);
        }

        /// <summary>
        /// Sets a stat value (overwrites existing).
        /// </summary>
        public async Task<ServiceResult> SetStatAsync(string statId, float value) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Stats service not initialized");
            }

            return await currentProvider.SetStatAsync(statId, value);
        }

        /// <summary>
        /// Increments a stat by the specified amount.
        /// </summary>
        public async Task<ServiceResult> IncrementStatAsync(string statId, float incrementBy = 1f) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Stats service not initialized");
            }

            return await currentProvider.IncrementStatAsync(statId, incrementBy);
        }

        /// <summary>
        /// Gets the current value of a stat.
        /// </summary>
        public async Task<ServiceResult<float>> GetStatAsync(string statId) {
            if (!IsInitialized) {
                return ServiceResult<float>.Failed("Stats service not initialized");
            }

            return await currentProvider.GetStatAsync(statId);
        }

        /// <summary>
        /// Gets all stats.
        /// </summary>
        public async Task<ServiceResult<PlayerStats>> GetAllStatsAsync() {
            if (!IsInitialized) {
                return ServiceResult<PlayerStats>.Failed("Stats service not initialized");
            }

            return await currentProvider.GetAllStatsAsync();
        }

        /// <summary>
        /// Resets a specific stat.
        /// </summary>
        public async Task<ServiceResult> ResetStatAsync(string statId) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Stats service not initialized");
            }

            return await currentProvider.ResetStatAsync(statId);
        }

        /// <summary>
        /// Resets all stats.
        /// </summary>
        public async Task<ServiceResult> ResetAllStatsAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Stats service not initialized");
            }

            return await currentProvider.ResetAllStatsAsync();
        }

        /// <summary>
        /// Syncs stats with cloud/server.
        /// </summary>
        public async Task<ServiceResult> SyncStatsAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Stats service not initialized");
            }

            return await currentProvider.SyncStatsAsync();
        }

        void HandleStatUpdated(string statId, float newValue) {
            Debug.Log($"[StatsService] Stat updated: {statId} = {newValue}");
            OnStatUpdated?.Invoke(statId, newValue);
        }

        void HandleStatsSynced() {
            Debug.Log("[StatsService] Stats synced");
            OnStatsSynced?.Invoke();
        }

        public override void Shutdown() {
            if (currentProvider != null) {
                currentProvider.OnStatUpdated -= HandleStatUpdated;
                currentProvider.OnStatsSynced -= HandleStatsSynced;
            }
            base.Shutdown();
        }
    }
}
