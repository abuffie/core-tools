using System;

namespace Aarware.Services.Account {
    /// <summary>
    /// Represents user account information.
    /// </summary>
    [Serializable]
    public class AccountData {
        public string userId;
        public string username;
        public string email;
        public string displayName;
        public string avatarUrl;
        public DateTime createdAt;
        public DateTime lastLoginAt;
        public bool isGuest;

        public AccountData() {
            userId = string.Empty;
            username = string.Empty;
            email = string.Empty;
            displayName = string.Empty;
            avatarUrl = string.Empty;
            createdAt = DateTime.Now;
            lastLoginAt = DateTime.Now;
            isGuest = false;
        }

        public AccountData(string userId, string username, bool isGuest = false) {
            this.userId = userId;
            this.username = username;
            this.displayName = username;
            this.email = string.Empty;
            this.avatarUrl = string.Empty;
            this.createdAt = DateTime.Now;
            this.lastLoginAt = DateTime.Now;
            this.isGuest = isGuest;
        }
    }
}
