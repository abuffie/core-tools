using System;
using System.Collections.Generic;

namespace Aarware.Services.Leaderboards {
    /// <summary>
    /// Represents a single leaderboard entry.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry {
        public int rank;
        public string userId;
        public string username;
        public long score;
        public string extraData;
        public DateTime submittedAt;

        public LeaderboardEntry(string userId, string username, long score) {
            this.rank = 0;
            this.userId = userId;
            this.username = username;
            this.score = score;
            this.extraData = string.Empty;
            this.submittedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a leaderboard with multiple entries.
    /// </summary>
    [Serializable]
    public class Leaderboard {
        public string leaderboardId;
        public string displayName;
        public LeaderboardSortOrder sortOrder;
        public List<LeaderboardEntry> entries;
        public DateTime lastUpdated;

        public Leaderboard(string leaderboardId, string displayName, LeaderboardSortOrder sortOrder = LeaderboardSortOrder.Descending) {
            this.leaderboardId = leaderboardId;
            this.displayName = displayName;
            this.sortOrder = sortOrder;
            this.entries = new List<LeaderboardEntry>();
            this.lastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Adds an entry to the leaderboard and sorts it.
        /// </summary>
        public void AddEntry(LeaderboardEntry entry) {
            entries.Add(entry);
            SortEntries();
        }

        /// <summary>
        /// Sorts entries based on sort order.
        /// </summary>
        public void SortEntries() {
            if (sortOrder == LeaderboardSortOrder.Ascending) {
                entries.Sort((a, b) => a.score.CompareTo(b.score));
            } else {
                entries.Sort((a, b) => b.score.CompareTo(a.score));
            }

            // Update ranks
            for (int i = 0; i < entries.Count; i++) {
                entries[i].rank = i + 1;
            }

            lastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Gets an entry by user ID.
        /// </summary>
        public LeaderboardEntry GetEntryByUserId(string userId) {
            return entries.Find(e => e.userId == userId);
        }

        /// <summary>
        /// Gets the top N entries.
        /// </summary>
        public List<LeaderboardEntry> GetTopEntries(int count) {
            int actualCount = Math.Min(count, entries.Count);
            return entries.GetRange(0, actualCount);
        }
    }

    /// <summary>
    /// Sort order for leaderboards.
    /// </summary>
    public enum LeaderboardSortOrder {
        /// <summary>Lowest score first (e.g., fastest time)</summary>
        Ascending,
        /// <summary>Highest score first (e.g., highest points)</summary>
        Descending
    }

    /// <summary>
    /// Time range for leaderboard queries.
    /// </summary>
    public enum LeaderboardTimeRange {
        AllTime,
        Weekly,
        Monthly,
        Daily
    }

    /// <summary>
    /// Scope for leaderboard queries.
    /// </summary>
    public enum LeaderboardScope {
        Global,
        Friends,
        AroundPlayer
    }
}
