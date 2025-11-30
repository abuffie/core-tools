using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.Account {
    /// <summary>
    /// Local account provider that stores account data using PlayerPrefs.
    /// Suitable for offline games or as a fallback.
    /// </summary>
    public class LocalAccountProvider : IAccountProvider {
        const string ACCOUNT_DATA_KEY = "Aarware_LocalAccount";
        const string IS_LOGGED_IN_KEY = "Aarware_IsLoggedIn";

        AccountData currentAccount;

        public BackendPlatform Platform => BackendPlatform.Local;
        public bool IsInitialized { get; private set; }

        public event Action<AccountData> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogout;

        public async Task<bool> InitializeAsync() {
            if (IsInitialized) {
                return true;
            }

            // Try to load existing account
            if (PlayerPrefs.HasKey(ACCOUNT_DATA_KEY) && PlayerPrefs.GetInt(IS_LOGGED_IN_KEY, 0) == 1) {
                string json = PlayerPrefs.GetString(ACCOUNT_DATA_KEY);
                currentAccount = JsonUtility.FromJson<AccountData>(json);
                Debug.Log($"[LocalAccountProvider] Loaded existing account: {currentAccount.username}");
            }

            IsInitialized = true;
            await Task.CompletedTask;
            return true;
        }

        public void Shutdown() {
            IsInitialized = false;
        }

        public async Task<ServiceResult<AccountData>> LoginAsync(string username, string password) {
            // For local accounts, we just check if an account with this username exists
            if (PlayerPrefs.HasKey(ACCOUNT_DATA_KEY)) {
                string json = PlayerPrefs.GetString(ACCOUNT_DATA_KEY);
                AccountData savedAccount = JsonUtility.FromJson<AccountData>(json);

                if (savedAccount.username == username) {
                    currentAccount = savedAccount;
                    currentAccount.lastLoginAt = DateTime.Now;
                    SaveAccount();

                    PlayerPrefs.SetInt(IS_LOGGED_IN_KEY, 1);
                    PlayerPrefs.Save();

                    OnLoginSuccess?.Invoke(currentAccount);
                    await Task.CompletedTask;
                    return ServiceResult<AccountData>.Successful(currentAccount);
                }
            }

            string error = "Account not found. Please create an account first.";
            OnLoginFailed?.Invoke(error);
            await Task.CompletedTask;
            return ServiceResult<AccountData>.Failed(error);
        }

        public async Task<ServiceResult<AccountData>> LoginAsGuestAsync() {
            string guestId = Guid.NewGuid().ToString();
            currentAccount = new AccountData($"guest_{guestId}", $"Guest_{guestId.Substring(0, 8)}", true);

            PlayerPrefs.SetInt(IS_LOGGED_IN_KEY, 1);
            PlayerPrefs.Save();

            OnLoginSuccess?.Invoke(currentAccount);
            await Task.CompletedTask;
            return ServiceResult<AccountData>.Successful(currentAccount);
        }

        public async Task<ServiceResult> LogoutAsync() {
            currentAccount = null;
            PlayerPrefs.SetInt(IS_LOGGED_IN_KEY, 0);
            PlayerPrefs.Save();

            OnLogout?.Invoke();
            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        public async Task<ServiceResult<AccountData>> CreateAccountAsync(string username, string password, string email) {
            if (string.IsNullOrEmpty(username)) {
                await Task.CompletedTask;
                return ServiceResult<AccountData>.Failed("Username cannot be empty");
            }

            string userId = Guid.NewGuid().ToString();
            currentAccount = new AccountData(userId, username, false) {
                email = email
            };

            SaveAccount();

            PlayerPrefs.SetInt(IS_LOGGED_IN_KEY, 1);
            PlayerPrefs.Save();

            OnLoginSuccess?.Invoke(currentAccount);
            await Task.CompletedTask;
            return ServiceResult<AccountData>.Successful(currentAccount);
        }

        public async Task<ServiceResult> UpdateAccountAsync(AccountData accountData) {
            if (currentAccount == null) {
                await Task.CompletedTask;
                return ServiceResult.Failed("No account logged in");
            }

            currentAccount = accountData;
            SaveAccount();

            await Task.CompletedTask;
            return ServiceResult.Successful();
        }

        public AccountData GetCurrentAccount() {
            return currentAccount;
        }

        public bool IsLoggedIn() {
            return currentAccount != null && PlayerPrefs.GetInt(IS_LOGGED_IN_KEY, 0) == 1;
        }

        void SaveAccount() {
            string json = JsonUtility.ToJson(currentAccount);
            PlayerPrefs.SetString(ACCOUNT_DATA_KEY, json);
            PlayerPrefs.Save();
        }
    }
}
