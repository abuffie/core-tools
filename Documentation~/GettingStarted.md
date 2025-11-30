# Getting Started with Aarware Core Tools

## Installation

The package is automatically detected when placed in the `Packages/` folder during development.

## Quick Start

### 1. Configure Services

Open the Service Configurator window:
- `Aarware > Service Configurator`
- Create a new `ServiceConfiguration` asset
- Select platform providers for each service
- Save the configuration

### 2. Initialize Services

```csharp
using Aarware.Services;
using Aarware.Services.Account;
using Aarware.Services.DataStorage;

// Create and register services
var accountService = new AccountService();
accountService.SetProvider(new LocalAccountProvider());
await accountService.InitializeAsync();
ServiceManager.RegisterService(accountService);

// Access services from anywhere
var account = ServiceManager.GetService<AccountService>();
var result = await account.LoginAsGuestAsync();
```

### 3. Use Localization

1. Create a `LocaleData` asset: `Create > Aarware > Localization > Locale Data`
2. Add translations to the asset
3. Add `LocaleController` to a GameObject in your scene
4. Assign locale data assets to the controller
5. Add `LocaleComponent` to any Text/TextMeshPro objects

### 4. Manage Session Data

Use the editor tools to manage local data:
- `Aarware > Session Manager` - View/clear account, stats, achievements
- `Aarware > Cache Manager` - Manage cached images
- `Aarware > Locale Editor` - Manage translations
- `Aarware > Scene Manager` - Generate scene enums

## Example: Complete Service Setup

```csharp
using System.Threading.Tasks;
using UnityEngine;
using Aarware.Services;
using Aarware.Services.Account;
using Aarware.Services.Stats;
using Aarware.Services.Achievements;

public class GameBootstrap : MonoBehaviour {
    async void Start() {
        await InitializeServices();
    }

    async Task InitializeServices() {
        // Account Service
        var accountService = new AccountService();
        accountService.SetProvider(new LocalAccountProvider());
        accountService.OnLoginSuccess += OnPlayerLoggedIn;
        await accountService.InitializeAsync();
        ServiceManager.RegisterService(accountService);

        // Stats Service
        var statsService = new StatsService();
        statsService.SetProvider(new LocalStatsProvider());
        await statsService.InitializeAsync();
        ServiceManager.RegisterService(statsService);

        // Define stats
        statsService.DefineStats(new List<Stat> {
            new Stat("kills", "Kills", StatType.Int),
            new Stat("deaths", "Deaths", StatType.Int),
            new Stat("highScore", "High Score", StatType.Max)
        });

        // Achievements Service
        var achievementsService = new AchievementsService();
        achievementsService.SetProvider(new LocalAchievementsProvider());
        achievementsService.OnAchievementUnlocked += OnAchievementUnlocked;
        await achievementsService.InitializeAsync();
        ServiceManager.RegisterService(achievementsService);

        Debug.Log("All services initialized!");
    }

    void OnPlayerLoggedIn(AccountData account) {
        Debug.Log($"Player logged in: {account.username}");
    }

    void OnAchievementUnlocked(Achievement achievement) {
        Debug.Log($"Achievement unlocked: {achievement.displayName}");
    }

    void OnDestroy() {
        ServiceManager.ShutdownAll();
    }
}
```

## Scene Enum Generation

1. Open `Aarware > Scene Manager`
2. Enable scenes you want in the build
3. Click "Generate Scene Enum"
4. Use the generated enum:

```csharp
using UnityEngine.SceneManagement;

// Type-safe scene loading
SceneManager.LoadScene(SceneNames.MainMenu.ToString());
```

## Platform Provider Integration

The package includes stubs for platform-specific providers:
- **Steam**: Integrate Steamworks.NET SDK
- **Android**: Integrate Google Play Games SDK
- **Windows**: Integrate Microsoft GDK/Xbox Live SDK
- **Photon**: Integrate Photon PUN2 SDK

Each provider has TODO comments indicating where to integrate the platform SDKs.

## Architecture Overview

- **Services**: High-level APIs (AccountService, StatsService, etc.)
- **Providers**: Platform-specific implementations (LocalAccountProvider, SteamAccountProvider, etc.)
- **ServiceManager**: Central registry for accessing services
- **Events**: All services use C# events for callbacks (no coroutines)

## Best Practices

1. Always initialize services before using them
2. Register services with ServiceManager for global access
3. Use async/await for service operations
4. Subscribe to service events for state changes
5. Shutdown services when no longer needed
6. Use ScriptableObjects for configurations
7. Never use GameObject.Find - assign references in inspector

## Support

For issues and feature requests, refer to the project repository.
