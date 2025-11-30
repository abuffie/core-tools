# Package Structure

## Directory Layout

```
Packages/com.aarware.coretools/
├── package.json                    # Package manifest
├── README.md                       # Package overview
├── CHANGELOG.md                    # Version history
├── LICENSE.md                      # MIT License
├── Documentation~/                 # Documentation (ignored by Unity)
│   ├── GettingStarted.md
│   └── PackageStructure.md
├── Runtime/                        # Runtime scripts
│   ├── Aarware.CoreTools.asmdef   # Runtime assembly definition
│   ├── Avatar/
│   │   └── AvatarGenerator.cs
│   ├── Localization/
│   │   ├── LocaleController.cs
│   │   ├── LocaleComponent.cs
│   │   └── LocaleData.cs
│   ├── RemoteImage/
│   │   ├── RemoteImageLoader.cs
│   │   └── RemoteImageComponent.cs
│   ├── Services/
│   │   ├── Base/
│   │   │   ├── IService.cs
│   │   │   ├── IServiceProvider.cs
│   │   │   ├── ServiceBase.cs
│   │   │   ├── ServiceManager.cs
│   │   │   └── ServiceResult.cs
│   │   ├── Account/
│   │   │   ├── AccountData.cs
│   │   │   ├── AccountService.cs
│   │   │   ├── IAccountProvider.cs
│   │   │   └── Providers/
│   │   │       ├── LocalAccountProvider.cs
│   │   │       ├── SteamAccountProvider.cs
│   │   │       ├── AndroidAccountProvider.cs
│   │   │       └── WindowsAccountProvider.cs
│   │   ├── DataStorage/
│   │   │   ├── SaveData.cs
│   │   │   ├── DataStorageService.cs
│   │   │   ├── IDataStorageProvider.cs
│   │   │   └── Providers/
│   │   │       └── LocalDataStorageProvider.cs
│   │   ├── Stats/
│   │   │   ├── StatData.cs
│   │   │   ├── StatsService.cs
│   │   │   ├── IStatsProvider.cs
│   │   │   └── Providers/
│   │   │       └── LocalStatsProvider.cs
│   │   ├── Achievements/
│   │   │   ├── AchievementData.cs
│   │   │   ├── AchievementsService.cs
│   │   │   ├── IAchievementsProvider.cs
│   │   │   └── Providers/
│   │   │       └── LocalAchievementsProvider.cs
│   │   ├── Leaderboards/
│   │   │   ├── LeaderboardData.cs
│   │   │   ├── LeaderboardsService.cs
│   │   │   ├── ILeaderboardsProvider.cs
│   │   │   └── Providers/
│   │   │       └── LocalLeaderboardsProvider.cs
│   │   ├── Networking/
│   │   │   ├── NetworkData.cs
│   │   │   ├── NetworkingService.cs
│   │   │   ├── INetworkingProvider.cs
│   │   │   └── Providers/
│   │   │       └── PhotonNetworkingProvider.cs
│   │   └── ServiceConfiguration.cs
│   ├── Utilities/
│   │   └── MonoSingleton.cs
│   └── Resources/                  # For runtime resources
└── Editor/                         # Editor-only scripts
    ├── Aarware.CoreTools.Editor.asmdef
    ├── SessionManagerWindow.cs
    ├── CacheManagerWindow.cs
    ├── LocaleEditorWindow.cs
    ├── ServiceConfiguratorWindow.cs
    └── SceneManagerWindow.cs
```

## Verification Steps

### 1. Check Package Manager
1. Open Unity
2. Go to `Window > Package Manager`
3. Switch to "Packages: In Project" or "Packages: Custom"
4. You should see "Aarware Core Tools" listed

### 2. Verify Editor Menu
Check that the following menu items appear:
- `Aarware > Session Manager`
- `Aarware > Cache Manager`
- `Aarware > Locale Editor`
- `Aarware > Service Configurator`
- `Aarware > Scene Manager`

### 3. Test ScriptableObject Creation
Right-click in Project window, verify these appear:
- `Create > Aarware > Localization > Locale Data`
- `Create > Aarware > Service Configuration`

### 4. Test Script References
Create a test script in Assets:

```csharp
using Aarware.Services;
using Aarware.Localization;
using Aarware.Avatar;

// If no compile errors, package is properly imported
```

## Assembly Definitions

The package uses two assembly definitions:

1. **Aarware.CoreTools** (Runtime)
   - Contains all runtime code
   - Platform: All platforms

2. **Aarware.CoreTools.Editor** (Editor)
   - Contains all editor windows
   - Platform: Editor only
   - References: Aarware.CoreTools

## Meta Files

Unity automatically generates `.meta` files for all assets. These are:
- Automatically created when Unity imports the package
- Required for proper asset tracking
- Should not be deleted

## Development Workflow

### Testing Changes
1. Edit scripts in `Packages/com.aarware.coretools/`
2. Unity will automatically recompile
3. Test in your project

### Version Updates
1. Update version in `package.json`
2. Add entry to `CHANGELOG.md`
3. Commit changes

### Distribution
When ready to distribute:
1. Copy the entire `com.aarware.coretools` folder
2. Share via git repository, or
3. Create a .unitypackage, or
4. Publish to npm/OpenUPM

## Notes

- The `Documentation~` folder (with tilde) is ignored during package export
- The `Resources` folder can be used for runtime resources that need to be loaded via `Resources.Load()`
- Editor scripts are only included in Unity Editor builds, not runtime builds
