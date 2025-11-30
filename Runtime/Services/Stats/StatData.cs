using System;
using System.Collections.Generic;

namespace Aarware.Services.Stats {
    /// <summary>
    /// Represents a single statistic.
    /// </summary>
    [Serializable]
    public class Stat {
        public string statId;
        public string displayName;
        public StatType type;
        public float value;
        public float defaultValue;

        public Stat(string statId, string displayName, StatType type, float defaultValue = 0f) {
            this.statId = statId;
            this.displayName = displayName;
            this.type = type;
            this.value = defaultValue;
            this.defaultValue = defaultValue;
        }
    }

    /// <summary>
    /// Types of statistics.
    /// </summary>
    public enum StatType {
        /// <summary>Integer stat (kills, deaths, games played, etc.)</summary>
        Int,
        /// <summary>Float stat (accuracy, distance traveled, etc.)</summary>
        Float,
        /// <summary>Average stat (calculated from sum and count)</summary>
        Average,
        /// <summary>Max value stat (highest score, longest streak, etc.)</summary>
        Max,
        /// <summary>Min value stat (lowest time, etc.)</summary>
        Min
    }

    /// <summary>
    /// Container for all player statistics.
    /// </summary>
    [Serializable]
    public class PlayerStats {
        public Dictionary<string, Stat> stats;
        public DateTime lastUpdated;

        public PlayerStats() {
            stats = new Dictionary<string, Stat>();
            lastUpdated = DateTime.Now;
        }

        public void AddStat(Stat stat) {
            if (!stats.ContainsKey(stat.statId)) {
                stats[stat.statId] = stat;
            }
        }

        public Stat GetStat(string statId) {
            if (stats.TryGetValue(statId, out Stat stat)) {
                return stat;
            }
            return null;
        }

        public bool HasStat(string statId) {
            return stats.ContainsKey(statId);
        }
    }
}
