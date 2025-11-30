using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.Account {
    /// <summary>
    /// Service for managing user accounts across different platforms.
    /// </summary>
    public class AccountService : ServiceBase<IAccountProvider> {
        /// <summary>
        /// Event fired when user successfully logs in.
        /// </summary>
        public event Action<AccountData> OnLoginSuccess;

        /// <summary>
        /// Event fired when login fails.
        /// </summary>
        public event Action<string> OnLoginFailed;

        /// <summary>
        /// Event fired when user logs out.
        /// </summary>
        public event Action OnLogout;

        public override void SetProvider(IAccountProvider provider) {
            // Unsubscribe from old provider
            if (currentProvider != null) {
                currentProvider.OnLoginSuccess -= HandleLoginSuccess;
                currentProvider.OnLoginFailed -= HandleLoginFailed;
                currentProvider.OnLogout -= HandleLogout;
            }

            base.SetProvider(provider);

            // Subscribe to new provider
            if (currentProvider != null) {
                currentProvider.OnLoginSuccess += HandleLoginSuccess;
                currentProvider.OnLoginFailed += HandleLoginFailed;
                currentProvider.OnLogout += HandleLogout;
            }
        }

        /// <summary>
        /// Logs in with username and password.
        /// </summary>
        public async Task<ServiceResult<AccountData>> LoginAsync(string username, string password) {
            if (!IsInitialized) {
                return ServiceResult<AccountData>.Failed("Account service not initialized");
            }

            return await currentProvider.LoginAsync(username, password);
        }

        /// <summary>
        /// Logs in as a guest user.
        /// </summary>
        public async Task<ServiceResult<AccountData>> LoginAsGuestAsync() {
            if (!IsInitialized) {
                return ServiceResult<AccountData>.Failed("Account service not initialized");
            }

            return await currentProvider.LoginAsGuestAsync();
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        public async Task<ServiceResult> LogoutAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Account service not initialized");
            }

            return await currentProvider.LogoutAsync();
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        public async Task<ServiceResult<AccountData>> CreateAccountAsync(string username, string password, string email = "") {
            if (!IsInitialized) {
                return ServiceResult<AccountData>.Failed("Account service not initialized");
            }

            return await currentProvider.CreateAccountAsync(username, password, email);
        }

        /// <summary>
        /// Updates the current account information.
        /// </summary>
        public async Task<ServiceResult> UpdateAccountAsync(AccountData accountData) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Account service not initialized");
            }

            return await currentProvider.UpdateAccountAsync(accountData);
        }

        /// <summary>
        /// Gets the currently logged in account.
        /// </summary>
        public AccountData GetCurrentAccount() {
            if (!IsInitialized) {
                return null;
            }

            return currentProvider.GetCurrentAccount();
        }

        /// <summary>
        /// Checks if a user is currently logged in.
        /// </summary>
        public bool IsLoggedIn() {
            if (!IsInitialized) {
                return false;
            }

            return currentProvider.IsLoggedIn();
        }

        void HandleLoginSuccess(AccountData account) {
            Debug.Log($"[AccountService] Login successful: {account.username}");
            OnLoginSuccess?.Invoke(account);
        }

        void HandleLoginFailed(string error) {
            Debug.LogWarning($"[AccountService] Login failed: {error}");
            OnLoginFailed?.Invoke(error);
        }

        void HandleLogout() {
            Debug.Log("[AccountService] User logged out");
            OnLogout?.Invoke();
        }

        public override void Shutdown() {
            if (currentProvider != null) {
                currentProvider.OnLoginSuccess -= HandleLoginSuccess;
                currentProvider.OnLoginFailed -= HandleLoginFailed;
                currentProvider.OnLogout -= HandleLogout;
            }
            base.Shutdown();
        }
    }
}
