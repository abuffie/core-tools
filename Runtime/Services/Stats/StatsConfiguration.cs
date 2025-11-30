using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aarware.Services.Stats {
    /// <summary>
    /// Configuration for game statistics.
    /// Defines all stats available in the game, modeled after Steam's stat system.
    /// </summary>
    [CreateAssetMenu(fileName = "StatsConfiguration", menuName = "Aarware/Stats Configuration", order = 2)]
    public class StatsConfiguration : ScriptableObject {
        [Header("Platform Configuration")]
        [Tooltip("How stats are stored: Local Only, Local+Cloud Sync, or Cloud Only")]
        public ServiceStorageMode storageMode = ServiceStorageMode.LocalWithCloudSync;

        [Tooltip("Cloud platform for syncing (when not using Local Only)")]
        public BackendPlatform cloudPlatform = BackendPlatform.Steam;

        [Header("Stat Definitions")]
        [Tooltip("All stats defined for this game")]
        public List<StatDefinition> stats = new List<StatDefinition>();

        /// <summary>
        /// Gets a stat definition by ID.
        /// </summary>
        public StatDefinition GetStat(string statId) {
            foreach (var stat in stats) {
                if (stat.statId == statId) {
                    return stat;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if a stat exists.
        /// </summary>
        public bool HasStat(string statId) {
            return GetStat(statId) != null;
        }

        /// <summary>
        /// Adds a new stat definition.
        /// </summary>
        public void AddStat(StatDefinition stat) {
            if (!HasStat(stat.statId)) {
                stats.Add(stat);
            }
        }

        /// <summary>
        /// Removes a stat definition.
        /// </summary>
        public bool RemoveStat(string statId) {
            for (int i = 0; i < stats.Count; i++) {
                if (stats[i].statId == statId) {
                    stats.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static StatsConfiguration CreateDefault() {
            StatsConfiguration config = CreateInstance<StatsConfiguration>();
            config.storageMode = ServiceStorageMode.LocalWithCloudSync;
            config.cloudPlatform = BackendPlatform.Steam;
            config.stats = new List<StatDefinition>();
            return config;
        }
    }

    /// <summary>
    /// Defines a single stat (design-time configuration).
    /// </summary>
    [Serializable]
    public class StatDefinition {
        [Tooltip("Unique identifier for this stat")]
        public string statId;

        [Tooltip("Display name shown to players")]
        public string displayName;

        [Tooltip("Type of stat (Int, Float, etc.)")]
        public StatType type;

        [Tooltip("Default/starting value")]
        public float defaultValue;

        [Tooltip("Minimum allowed value (optional)")]
        public float minValue;

        [Tooltip("Maximum allowed value (optional)")]
        public float maxValue;

        [Tooltip("Whether this stat has min/max constraints")]
        public bool useMinMax;

        [Tooltip("Whether to show this stat to players")]
        public bool isVisible = true;

        [Tooltip("Description of what this stat tracks")]
        [TextArea(2, 4)]
        public string description;

        public StatDefinition() {
            statId = "";
            displayName = "";
            type = StatType.Int;
            defaultValue = 0f;
            minValue = 0f;
            maxValue = 100f;
            useMinMax = false;
            isVisible = true;
            description = "";
        }

        public StatDefinition(string id, string name, StatType statType, float defaultVal = 0f) {
            statId = id;
            displayName = name;
            type = statType;
            defaultValue = defaultVal;
            minValue = 0f;
            maxValue = 100f;
            useMinMax = false;
            isVisible = true;
            description = "";
        }
    }
}
