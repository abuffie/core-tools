using System;
using System.Threading.Tasks;

namespace Aarware.Services.Account {
    /// <summary>
    /// Interface for platform-specific account providers.
    /// </summary>
    public interface IAccountProvider : IServiceProvider {
        /// <summary>
        /// The backend platform this provider supports.
        /// </summary>
        BackendPlatform Platform { get; }

        /// <summary>
        /// Event fired when user successfully logs in.
        /// </summary>
        event Action<AccountData> OnLoginSuccess;

        /// <summary>
        /// Event fired when login fails.
        /// </summary>
        event Action<string> OnLoginFailed;

        /// <summary>
        /// Event fired when user logs out.
        /// </summary>
        event Action OnLogout;

        /// <summary>
        /// Attempts to log in with credentials.
        /// </summary>
        Task<ServiceResult<AccountData>> LoginAsync(string username, string password);

        /// <summary>
        /// Attempts to log in as guest.
        /// </summary>
        Task<ServiceResult<AccountData>> LoginAsGuestAsync();

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        Task<ServiceResult> LogoutAsync();

        /// <summary>
        /// Gets the currently logged in account, if any.
        /// </summary>
        AccountData GetCurrentAccount();

        /// <summary>
        /// Checks if a user is currently logged in.
        /// </summary>
        bool IsLoggedIn();

        /// <summary>
        /// Creates a new account.
        /// </summary>
        Task<ServiceResult<AccountData>> CreateAccountAsync(string username, string password, string email);

        /// <summary>
        /// Updates account information.
        /// </summary>
        Task<ServiceResult> UpdateAccountAsync(AccountData accountData);
    }
}
