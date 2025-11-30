# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0] - 2025-10-25

### Added
- Initial release of Aarware Core Tools
- Base service architecture with swappable providers
- Account service with multi-platform support (Local, Steam, Android, Windows)
- Data Storage service with local file-based saves
- Stats service with various stat types (Int, Float, Average, Max, Min)
- Achievements service with progressive achievement support
- Leaderboards service with ranking and score submission
- Networking service with Photon-like event system
- Localization system with LocaleController and LocaleComponent
- Remote image loading with memory and disk caching
- Avatar generator with deterministic generation from strings/seeds
- MonoSingleton utility for clean singleton pattern
- **Unified Core Tools Window** with tabbed interface:
  - Session Manager tab (Account, Stats, Achievements, Save Data)
  - Cache Manager tab (Image cache and bulk operations)
  - Locale Editor tab (Scene tools and locale data management)
  - Leaderboard Manager tab (View and manage leaderboards)
  - Service Configurator tab (Platform provider selection)
  - Scene Manager tab (Build settings and enum generation)

### Features
- C# event-driven architecture (no coroutines)
- ScriptableObject-based configurations
- Multi-platform support: Web, Android, PC
- Local-first with cloud-sync ready
- Type-safe service locator pattern
- All-in-one tabbed editor window for streamlined workflow
- TextMeshPro integration with proper assembly dependencies
