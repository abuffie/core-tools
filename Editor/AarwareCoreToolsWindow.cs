using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Aarware.Localization;
using Aarware.Services;
using Aarware.Services.Account;
using Aarware.Services.Stats;
using Aarware.Services.Achievements;
using Aarware.Services.Leaderboards;
using Aarware.Services.Networking;
using Aarware.Services.DataStorage;
using Aarware.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aarware.Editor {
    /// <summary>
    /// Unified editor window for all Aarware Core Tools.
    /// </summary>
    public class AarwareCoreToolsWindow : EditorWindow {
        const string BUILD_TARGET_KEY = "Aarware_BuildTarget";

        Vector2 scrollPosition;
        int selectedTab = 0;
        string[] tabNames = { "Session", "Cache", "Locale", "Stats", "Achievements", "Leaderboards", "Networking", "Data Storage", "Scenes" };

        // Build Target (global setting)
        DistributionPlatform currentBuildTarget;

        // Locale Editor
        List<GameObject> foundTextObjects = new List<GameObject>();
        LocaleData selectedLocaleData;
        int localeSubTab = 0;

        // Cache Manager
        string imageCachePath;
        long totalCacheSize = 0;

        // Stats Editor
        StatsConfiguration statsConfig;
        string newStatId = "";
        string newStatDisplayName = "";
        StatType newStatType = StatType.Int;
        float newStatDefaultValue = 0f;
        Vector2 statsScrollPosition;

        // Achievements Editor
        AchievementsConfiguration achievementsConfig;
        string newAchievementId = "";
        string newAchievementDisplayName = "";
        string newAchievementDescription = "";
        bool newAchievementIsProgressive = false;
        float newAchievementMaxProgress = 100f;
        bool newAchievementIsHidden = false;
        int newAchievementPointValue = 10;
        Vector2 achievementsScrollPosition;

        // Leaderboards Editor
        LeaderboardsConfiguration leaderboardsConfig;
        string newLeaderboardId = "";
        string newLeaderboardDisplayName = "";
        LeaderboardSortOrder newLeaderboardSortOrder = LeaderboardSortOrder.Descending;
        Vector2 leaderboardsScrollPosition;

        // Networking Editor
        NetworkingConfiguration networkingConfig;

        // Data Storage Editor
        DataStorageConfiguration dataStorageConfig;

        // Scene Manager
        List<SceneInfo> mainScenes = new List<SceneInfo>();
        List<SceneInfo> additiveScenes = new List<SceneInfo>();
        List<SceneInfo> testingScenes = new List<SceneInfo>();
        List<SceneInfo> otherScenes = new List<SceneInfo>();

        [MenuItem("Aarware/Core Tools Window")]
        static void ShowWindow() {
            AarwareCoreToolsWindow window = GetWindow<AarwareCoreToolsWindow>("Aarware Core Tools");
            window.minSize = new Vector2(700, 500);
        }

        void OnEnable() {
            imageCachePath = Path.Combine(Application.persistentDataPath, "ImageCache");
            CalculateCacheSize();
            ScanScenes();

            // Load build target from EditorPrefs
            currentBuildTarget = (DistributionPlatform)EditorPrefs.GetInt(BUILD_TARGET_KEY, (int)DistributionPlatform.StandaloneWindows);
        }

        void OnFocus() {
            ScanScenes();
        }

        void OnGUI() {
            // Header
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Aarware Core Tools", EditorStyles.boldLabel);
            GUILayout.Label("Unified editor for managing all aspects of your game", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Global Build Target Selector
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Build Target:", EditorStyles.boldLabel, GUILayout.Width(90));
            DistributionPlatform newBuildTarget = (DistributionPlatform)EditorGUILayout.EnumPopup(currentBuildTarget, GUILayout.Width(200));
            if (newBuildTarget != currentBuildTarget) {
                currentBuildTarget = newBuildTarget;
                EditorPrefs.SetInt(BUILD_TARGET_KEY, (int)currentBuildTarget);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("?", GUILayout.Width(25))) {
                EditorUtility.DisplayDialog("Build Target Info", PlatformValidator.GetBuildTargetInfo(currentBuildTarget), "OK");
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Tab selector
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(25));

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab) {
                case 0:
                    DrawSessionTab();
                    break;
                case 1:
                    DrawCacheTab();
                    break;
                case 2:
                    DrawLocaleTab();
                    break;
                case 3:
                    DrawStatsTab();
                    break;
                case 4:
                    DrawAchievementsTab();
                    break;
                case 5:
                    DrawLeaderboardsTab();
                    break;
                case 6:
                    DrawNetworkingTab();
                    break;
                case 7:
                    DrawDataStorageTab();
                    break;
                case 8:
                    DrawScenesTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        #region Session Manager Tab
        void DrawSessionTab() {
            GUILayout.Label("Session Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("View and manage all local player session data", MessageType.Info);

            EditorGUILayout.Space();

            string[] sessionTabs = { "Settings", "Account", "Stats", "Achievements", "Save Data", "Actions" };
            int sessionTab = GUILayout.SelectionGrid(-1, sessionTabs, 6);

            EditorGUILayout.Space();

            if (sessionTab == 0) DrawSettingsSection();
            else if (sessionTab == 1) DrawAccountSection();
            else if (sessionTab == 2) DrawStatsSection();
            else if (sessionTab == 3) DrawAchievementsSection();
            else if (sessionTab == 4) DrawSaveDataSection();
            else if (sessionTab == 5) DrawActionsSection();
            else DrawSettingsSection(); // default
        }

        void DrawSettingsSection() {
            GUILayout.Label("Settings Storage (PlayerPrefs)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Settings Storage uses PlayerPrefs for device settings, volumes, language, and session tokens.\n" +
                "This is separate from game save data (which uses JSON files).",
                MessageType.Info);

            EditorGUILayout.Space();

            // Common Settings
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Common Settings", EditorStyles.boldLabel);

            if (PlayerPrefs.HasKey("Language")) {
                EditorGUILayout.LabelField("Language:", PlayerPrefs.GetString("Language"));
            }
            if (PlayerPrefs.HasKey("MasterVolume")) {
                EditorGUILayout.LabelField("Master Volume:", PlayerPrefs.GetFloat("MasterVolume").ToString("F2"));
            }
            if (PlayerPrefs.HasKey("MusicVolume")) {
                EditorGUILayout.LabelField("Music Volume:", PlayerPrefs.GetFloat("MusicVolume").ToString("F2"));
            }
            if (PlayerPrefs.HasKey("SFXVolume")) {
                EditorGUILayout.LabelField("SFX Volume:", PlayerPrefs.GetFloat("SFXVolume").ToString("F2"));
            }
            if (PlayerPrefs.HasKey("Fullscreen")) {
                EditorGUILayout.LabelField("Fullscreen:", PlayerPrefs.GetInt("Fullscreen") == 1 ? "Yes" : "No");
            }

            if (!PlayerPrefs.HasKey("Language") && !PlayerPrefs.HasKey("MasterVolume")) {
                EditorGUILayout.HelpBox("No common settings found.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Session Tokens
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Session Tokens", EditorStyles.boldLabel);

            if (PlayerPrefs.HasKey("SessionToken")) {
                string token = PlayerPrefs.GetString("SessionToken");
                EditorGUILayout.LabelField("Session Token:", token.Substring(0, Mathf.Min(20, token.Length)) + "...");
            } else {
                EditorGUILayout.HelpBox("No session token found.", MessageType.Info);
            }

            if (PlayerPrefs.HasKey("RefreshToken")) {
                string token = PlayerPrefs.GetString("RefreshToken");
                EditorGUILayout.LabelField("Refresh Token:", token.Substring(0, Mathf.Min(20, token.Length)) + "...");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Actions
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Clear Session Tokens")) {
                if (EditorUtility.DisplayDialog("Clear Tokens", "Clear all session tokens?", "Yes", "No")) {
                    PlayerPrefs.DeleteKey("SessionToken");
                    PlayerPrefs.DeleteKey("RefreshToken");
                    PlayerPrefs.Save();
                    Debug.Log("Session tokens cleared");
                }
            }

            if (GUILayout.Button("Clear All Settings (PlayerPrefs)", GUILayout.Height(30))) {
                if (EditorUtility.DisplayDialog("Clear All", "Delete ALL PlayerPrefs? This cannot be undone!", "Yes", "Cancel")) {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    Debug.Log("All PlayerPrefs cleared");
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawAccountSection() {
            GUILayout.Label("Account Data", EditorStyles.boldLabel);

            if (PlayerPrefs.HasKey("Aarware_LocalAccount")) {
                string json = PlayerPrefs.GetString("Aarware_LocalAccount");
                AccountData account = JsonUtility.FromJson<AccountData>(json);

                EditorGUILayout.LabelField("User ID:", account.userId);
                EditorGUILayout.LabelField("Username:", account.username);
                EditorGUILayout.LabelField("Display Name:", account.displayName);
                EditorGUILayout.LabelField("Email:", account.email);
                EditorGUILayout.LabelField("Is Guest:", account.isGuest.ToString());
                EditorGUILayout.LabelField("Created:", account.createdAt.ToString());
                EditorGUILayout.LabelField("Last Login:", account.lastLoginAt.ToString());

                EditorGUILayout.Space();

                if (GUILayout.Button("Clear Account Data")) {
                    if (EditorUtility.DisplayDialog("Clear Account", "Are you sure?", "Yes", "No")) {
                        PlayerPrefs.DeleteKey("Aarware_LocalAccount");
                        PlayerPrefs.DeleteKey("Aarware_IsLoggedIn");
                        PlayerPrefs.Save();
                        Debug.Log("Account data cleared");
                    }
                }
            } else {
                EditorGUILayout.HelpBox("No local account data found.", MessageType.Info);
            }
        }

        void DrawStatsSection() {
            GUILayout.Label("Stats", EditorStyles.boldLabel);

            if (LocalStorageHelper.HasData("Aarware_Stats")) {
                string json = LocalStorageHelper.LoadData("Aarware_Stats");
                try {
                    PlayerStats stats = JsonUtility.FromJson<PlayerStats>(json);

                    EditorGUILayout.LabelField("Last Updated:", stats.lastUpdated.ToString());
                    EditorGUILayout.Space();

                    if (stats.stats != null && stats.stats.Count > 0) {
                        foreach (var stat in stats.stats.Values) {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(stat.displayName, GUILayout.Width(200));
                            EditorGUILayout.LabelField($"{stat.value} ({stat.type})", GUILayout.Width(150));
                            EditorGUILayout.EndHorizontal();
                        }
                    } else {
                        EditorGUILayout.HelpBox("No stats defined.", MessageType.Info);
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Clear All Stats")) {
                        if (EditorUtility.DisplayDialog("Clear Stats", "Reset all stats?", "Yes", "No")) {
                            LocalStorageHelper.DeleteData("Aarware_Stats");
                            Debug.Log("Stats cleared");
                        }
                    }
                } catch {
                    EditorGUILayout.HelpBox("Failed to parse stats data.", MessageType.Error);
                }
            } else {
                EditorGUILayout.HelpBox("No stats data found.", MessageType.Info);
            }
        }

        void DrawAchievementsSection() {
            GUILayout.Label("Achievements", EditorStyles.boldLabel);

            if (PlayerPrefs.HasKey("Aarware_PlayerAchievements")) {
                string json = PlayerPrefs.GetString("Aarware_PlayerAchievements");
                try {
                    PlayerAchievements achievements = JsonUtility.FromJson<PlayerAchievements>(json);

                    int unlocked = achievements.GetUnlockedAchievements().Count;
                    int total = achievements.achievements.Count;
                    float completion = achievements.GetOverallCompletionPercentage();

                    EditorGUILayout.LabelField($"Unlocked: {unlocked} / {total} ({completion:F1}%)");
                    EditorGUILayout.Space();

                    if (achievements.achievements != null && achievements.achievements.Count > 0) {
                        foreach (var achievement in achievements.achievements.Values) {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField(achievement.displayName, EditorStyles.boldLabel);
                            EditorGUILayout.LabelField("Description:", achievement.description);
                            EditorGUILayout.LabelField("Status:", achievement.isUnlocked ? "✓ Unlocked" : "Locked");
                            if (achievement.IsProgressive()) {
                                EditorGUILayout.LabelField($"Progress: {achievement.progress} / {achievement.maxProgress} ({achievement.GetProgressPercentage():F1}%)");
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                        }
                    } else {
                        EditorGUILayout.HelpBox("No achievements defined.", MessageType.Info);
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Clear All Achievements")) {
                        if (EditorUtility.DisplayDialog("Clear Achievements", "Reset all achievements?", "Yes", "No")) {
                            PlayerPrefs.DeleteKey("Aarware_PlayerAchievements");
                            PlayerPrefs.Save();
                            Debug.Log("Achievements cleared");
                        }
                    }
                } catch {
                    EditorGUILayout.HelpBox("Failed to parse achievements data.", MessageType.Error);
                }
            } else {
                EditorGUILayout.HelpBox("No achievements data found.", MessageType.Info);
            }
        }

        void DrawSaveDataSection() {
            GUILayout.Label("Save Data", EditorStyles.boldLabel);

            string savePath = Path.Combine(Application.persistentDataPath, "Saves");

            if (Directory.Exists(savePath)) {
                string[] files = Directory.GetFiles(savePath, "*.json");

                if (files.Length > 0) {
                    EditorGUILayout.LabelField($"Save Location: {savePath}");
                    EditorGUILayout.Space();

                    foreach (string file in files) {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(Path.GetFileName(file));
                        FileInfo info = new FileInfo(file);
                        EditorGUILayout.LabelField($"{info.Length} bytes", GUILayout.Width(100));
                        if (GUILayout.Button("Delete", GUILayout.Width(60))) {
                            if (EditorUtility.DisplayDialog("Delete Save", $"Delete {Path.GetFileName(file)}?", "Yes", "No")) {
                                File.Delete(file);
                                Debug.Log($"Deleted {file}");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                } else {
                    EditorGUILayout.HelpBox("No save files found.", MessageType.Info);
                }
            } else {
                EditorGUILayout.HelpBox($"Save directory does not exist: {savePath}", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Save Folder")) {
                if (Directory.Exists(savePath)) {
                    EditorUtility.RevealInFinder(savePath);
                } else {
                    Debug.LogWarning("Save folder does not exist yet.");
                }
            }
        }

        void DrawActionsSection() {
            GUILayout.Label("Bulk Actions", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Clear all local session data. This cannot be undone!", MessageType.Warning);

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear All Session Data", GUILayout.Height(40))) {
                if (EditorUtility.DisplayDialog("Clear All Data", "This will delete ALL local data. Are you sure?", "Yes", "Cancel")) {
                    PlayerPrefs.DeleteKey("Aarware_LocalAccount");
                    PlayerPrefs.DeleteKey("Aarware_IsLoggedIn");
                    PlayerPrefs.DeleteKey("Aarware_PlayerStats");
                    PlayerPrefs.DeleteKey("Aarware_PlayerAchievements");
                    PlayerPrefs.Save();

                    string savePath = Path.Combine(Application.persistentDataPath, "Saves");
                    if (Directory.Exists(savePath)) {
                        Directory.Delete(savePath, true);
                    }

                    Debug.Log("All session data cleared!");
                    EditorUtility.DisplayDialog("Complete", "All session data cleared.", "OK");
                }
            }
        }
        #endregion

        #region Cache Manager Tab
        void DrawCacheTab() {
            GUILayout.Label("Cache Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("View and manage all locally cached assets", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Cache Summary", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Size:", FormatBytes(totalCacheSize));
            EditorGUILayout.LabelField("Location:", Application.persistentDataPath);

            if (GUILayout.Button("Refresh")) {
                CalculateCacheSize();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            DrawImageCacheSection();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Bulk Actions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Clear all caches to free up disk space.", MessageType.Warning);

            if (GUILayout.Button("Clear All Caches", GUILayout.Height(30))) {
                if (EditorUtility.DisplayDialog("Clear All", "Delete all cached assets?", "Yes", "Cancel")) {
                    ClearAllCaches();
                }
            }
            EditorGUILayout.EndVertical();
        }

        void DrawImageCacheSection() {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Image Cache", EditorStyles.boldLabel);

            if (Directory.Exists(imageCachePath)) {
                string[] files = Directory.GetFiles(imageCachePath);
                long imageCacheSize = 0;

                foreach (string file in files) {
                    FileInfo info = new FileInfo(file);
                    imageCacheSize += info.Length;
                }

                EditorGUILayout.LabelField("Images:", files.Length.ToString());
                EditorGUILayout.LabelField("Size:", FormatBytes(imageCacheSize));

                if (files.Length > 0) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Recent Files:", EditorStyles.boldLabel);

                    int displayCount = Mathf.Min(5, files.Length);
                    for (int i = 0; i < displayCount; i++) {
                        FileInfo info = new FileInfo(files[i]);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(Path.GetFileName(files[i]), GUILayout.MinWidth(150));
                        EditorGUILayout.LabelField(FormatBytes(info.Length), GUILayout.Width(80));
                        EditorGUILayout.EndHorizontal();
                    }

                    if (files.Length > displayCount) {
                        EditorGUILayout.LabelField($"... and {files.Length - displayCount} more");
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Image Cache")) {
                    if (EditorUtility.DisplayDialog("Clear", $"Delete {files.Length} images?", "Yes", "No")) {
                        ClearImageCache();
                    }
                }
                if (GUILayout.Button("Open Folder")) {
                    EditorUtility.RevealInFinder(imageCachePath);
                }
                EditorGUILayout.EndHorizontal();
            } else {
                EditorGUILayout.HelpBox("Image cache doesn't exist yet.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        void CalculateCacheSize() {
            totalCacheSize = 0;
            if (Directory.Exists(imageCachePath)) {
                string[] files = Directory.GetFiles(imageCachePath);
                foreach (string file in files) {
                    FileInfo info = new FileInfo(file);
                    totalCacheSize += info.Length;
                }
            }
            Repaint();
        }

        void ClearImageCache() {
            if (Directory.Exists(imageCachePath)) {
                Directory.Delete(imageCachePath, true);
                Debug.Log("Image cache cleared");
                CalculateCacheSize();
            }
        }

        void ClearAllCaches() {
            ClearImageCache();
            Caching.ClearCache();
            Debug.Log("All caches cleared!");
            EditorUtility.DisplayDialog("Complete", "All caches cleared.", "OK");
            CalculateCacheSize();
        }

        string FormatBytes(long bytes) {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
        #endregion

        #region Locale Tab
        void DrawLocaleTab() {
            GUILayout.Label("Locale Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Manage localization and translations", MessageType.Info);

            EditorGUILayout.Space();

            string[] localeTabs = { "Scene Tools", "Locale Data" };
            localeSubTab = GUILayout.Toolbar(localeSubTab, localeTabs);

            EditorGUILayout.Space();

            if (localeSubTab == 0) {
                DrawSceneToolsSection();
            } else {
                DrawLocaleDataSection();
            }
        }

        void DrawSceneToolsSection() {
            GUILayout.Label("Scene Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Find All Text Objects", GUILayout.Height(25))) {
                FindAllTextObjects();
            }

            EditorGUILayout.Space();

            if (foundTextObjects.Count > 0) {
                EditorGUILayout.LabelField($"Found {foundTextObjects.Count} text objects:");

                foreach (GameObject obj in foundTextObjects) {
                    if (obj == null) continue;

                    EditorGUILayout.BeginHorizontal("box");

                    string path = GetGameObjectPath(obj);
                    EditorGUILayout.LabelField(path, GUILayout.MinWidth(200));

                    LocaleComponent localeComp = obj.GetComponent<LocaleComponent>();
                    bool hasComponent = localeComp != null;

                    if (hasComponent) {
                        EditorGUILayout.LabelField("✓ Has Component", GUILayout.Width(120));
                    } else {
                        if (GUILayout.Button("Add Component", GUILayout.Width(120))) {
                            Undo.AddComponent<LocaleComponent>(obj);
                            EditorUtility.SetDirty(obj);
                        }
                    }

                    if (GUILayout.Button("Select", GUILayout.Width(60))) {
                        Selection.activeGameObject = obj;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Component to All")) {
                    if (EditorUtility.DisplayDialog("Add Components", $"Add to all {foundTextObjects.Count} objects?", "Yes", "No")) {
                        AddLocaleComponentToAll();
                    }
                }
            } else {
                EditorGUILayout.HelpBox("No text objects found.", MessageType.Info);
            }
        }

        void DrawLocaleDataSection() {
            GUILayout.Label("Locale Data", EditorStyles.boldLabel);

            LocaleData newSelection = EditorGUILayout.ObjectField("Locale Asset:", selectedLocaleData, typeof(LocaleData), false) as LocaleData;
            if (newSelection != selectedLocaleData) {
                selectedLocaleData = newSelection;
            }

            EditorGUILayout.Space();

            if (selectedLocaleData != null) {
                EditorGUILayout.LabelField("Language:", selectedLocaleData.Language.ToString());
                EditorGUILayout.LabelField("Code:", selectedLocaleData.LanguageCode);

                var entries = selectedLocaleData.GetAllEntries();
                EditorGUILayout.LabelField($"Translations: {entries.Count}");

                EditorGUILayout.Space();

                if (entries.Count > 0) {
                    foreach (var entry in entries) {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("Key:", entry.key, EditorStyles.boldLabel);
                        EditorGUILayout.TextArea(entry.value, GUILayout.MinHeight(30));
                        EditorGUILayout.EndVertical();
                    }
                }
            } else {
                EditorGUILayout.HelpBox("Select a LocaleData asset to view translations.", MessageType.Info);

                if (GUILayout.Button("Create New LocaleData")) {
                    CreateNewLocaleData();
                }
            }
        }

        void FindAllTextObjects() {
            foundTextObjects.Clear();

            Text[] textComponents = FindObjectsOfType<Text>(true);
            foreach (Text text in textComponents) {
                if (!foundTextObjects.Contains(text.gameObject)) {
                    foundTextObjects.Add(text.gameObject);
                }
            }

            TextMeshProUGUI[] tmpComponents = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI tmp in tmpComponents) {
                if (!foundTextObjects.Contains(tmp.gameObject)) {
                    foundTextObjects.Add(tmp.gameObject);
                }
            }

            Debug.Log($"Found {foundTextObjects.Count} text objects");
        }

        void AddLocaleComponentToAll() {
            int added = 0;
            foreach (GameObject obj in foundTextObjects) {
                if (obj == null) continue;
                if (obj.GetComponent<LocaleComponent>() == null) {
                    Undo.AddComponent<LocaleComponent>(obj);
                    EditorUtility.SetDirty(obj);
                    added++;
                }
            }
            Debug.Log($"Added LocaleComponent to {added} objects");
            EditorUtility.DisplayDialog("Complete", $"Added to {added} objects", "OK");
        }

        string GetGameObjectPath(GameObject obj) {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null) {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        void CreateNewLocaleData() {
            string path = EditorUtility.SaveFilePanelInProject("Create Locale Data", "NewLocaleData", "asset", "Create locale asset");
            if (!string.IsNullOrEmpty(path)) {
                LocaleData newData = CreateInstance<LocaleData>();
                AssetDatabase.CreateAsset(newData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                selectedLocaleData = newData;
                Debug.Log($"Created {path}");
            }
        }
        #endregion

        #region Leaderboards Tab
        void DrawLeaderboardsTab() {
            GUILayout.Label("Leaderboard Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("View and manage local leaderboard data", MessageType.Info);

            EditorGUILayout.Space();

            // Get all leaderboard keys from PlayerPrefs
            string leaderboardPrefix = "Aarware_Leaderboards_";
            List<string> leaderboardKeys = new List<string>();

            // Since PlayerPrefs doesn't have a GetAllKeys method, we'll need to check for known leaderboards
            // or display a message about how they work
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Local Leaderboards", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Leaderboards are stored locally with prefix 'Aarware_Leaderboards_'.\n\nLeaderboards will appear here after being created through code.", MessageType.Info);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Storage Location Info
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Local Data Storage", EditorStyles.boldLabel);

            string leaderboardsStoragePath = LocalStorageHelper.GetStoragePath("Aarware_Leaderboards");
            if (leaderboardsStoragePath != null) {
                EditorGUILayout.LabelField("Storage Type:", "File-based (Desktop/Mobile)");
                EditorGUILayout.LabelField("File Path:", $"{leaderboardsStoragePath} (+ leaderboard ID suffix)");

                if (GUILayout.Button("Open Storage Folder")) {
                    string directory = LocalStorageHelper.GetStorageDirectory();
                    if (directory != null && Directory.Exists(directory)) {
                        EditorUtility.RevealInFinder(directory);
                    } else if (directory != null) {
                        Directory.CreateDirectory(directory);
                        EditorUtility.RevealInFinder(directory);
                    }
                }
            } else {
                EditorGUILayout.LabelField("Storage Type:", "PlayerPrefs (WebGL build target)");
                EditorGUILayout.HelpBox("WebGL builds store data in browser IndexedDB via PlayerPrefs. File access is not available on WebGL.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Show instructions for testing
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Testing Leaderboards", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("To test leaderboards:\n\n1. Create leaderboards in code using LeaderboardsService\n2. Submit test scores\n3. Return here to view and manage entries", MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear All Leaderboards", GUILayout.Height(30))) {
                if (EditorUtility.DisplayDialog("Clear Leaderboards", "Delete all local leaderboard data?", "Yes", "Cancel")) {
                    // Clear all leaderboard PlayerPrefs keys
                    // This is a bit of a hack since we can't enumerate all keys
                    // In a production version, you'd want to maintain a list of leaderboard IDs
                    PlayerPrefs.DeleteAll(); // WARNING: This deletes ALL PlayerPrefs
                    PlayerPrefs.Save();
                    Debug.Log("All leaderboards cleared");
                    EditorUtility.DisplayDialog("Complete", "All leaderboards cleared.", "OK");
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Example code section
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Example Code", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(@"// Setup leaderboards service
var leaderboardsService = new LeaderboardsService();
leaderboardsService.SetProvider(new LocalLeaderboardsProvider());
await leaderboardsService.InitializeAsync();

// Define leaderboards
leaderboardsService.DefineLeaderboards(new List<Leaderboard> {
    new Leaderboard(""highscores"", ""High Scores"", LeaderboardSortOrder.Descending),
    new Leaderboard(""fastestTime"", ""Fastest Time"", LeaderboardSortOrder.Ascending)
});

// Submit score
await leaderboardsService.SubmitScoreAsync(""highscores"", 1000);", MessageType.None);
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Stats Tab
        void DrawStatsTab() {
            GUILayout.Label("Stats Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Define and manage game statistics. Modeled after Steam's stat system.", MessageType.Info);

            EditorGUILayout.Space();

            // Configuration Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            StatsConfiguration newConfig = EditorGUILayout.ObjectField("Stats Config:", statsConfig, typeof(StatsConfiguration), false) as StatsConfiguration;
            if (newConfig != statsConfig) {
                statsConfig = newConfig;
            }

            if (statsConfig == null) {
                EditorGUILayout.HelpBox("No configuration selected. Create or select one.", MessageType.Warning);

                if (GUILayout.Button("Create New Stats Configuration", GUILayout.Height(30))) {
                    CreateNewStatsConfiguration();
                }

                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Platform Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Platform Configuration", EditorStyles.boldLabel);

            ServiceStorageMode newMode = (ServiceStorageMode)EditorGUILayout.EnumPopup("Storage Mode:", statsConfig.storageMode);
            if (newMode != statsConfig.storageMode) {
                statsConfig.storageMode = newMode;
                EditorUtility.SetDirty(statsConfig);
            }

            // Show cloud platform selector if not LocalOnly
            if (statsConfig.storageMode != ServiceStorageMode.LocalOnly) {
                EditorGUI.indentLevel++;
                BackendPlatform newPlatform = (BackendPlatform)EditorGUILayout.EnumPopup("Cloud Platform:", statsConfig.cloudPlatform);
                if (newPlatform != statsConfig.cloudPlatform) {
                    statsConfig.cloudPlatform = newPlatform;
                    EditorUtility.SetDirty(statsConfig);
                }

                string platformInfo = GetPlatformInfo(statsConfig.cloudPlatform);
                if (!string.IsNullOrEmpty(platformInfo)) {
                    EditorGUILayout.HelpBox(platformInfo, MessageType.Info);
                }

                // Validation warning
                if (!PlatformValidator.IsBackendPlatformValid(currentBuildTarget, statsConfig.cloudPlatform)) {
                    string error = PlatformValidator.GetValidationError(currentBuildTarget, statsConfig.cloudPlatform);
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }

            // Show mode explanation
            EditorGUILayout.Space();
            string modeInfo = GetStorageModeInfo(statsConfig.storageMode);
            EditorGUILayout.HelpBox(modeInfo, MessageType.Info);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Add New Stat
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Add New Stat", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stat ID:", GUILayout.Width(100));
            newStatId = EditorGUILayout.TextField(newStatId);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Display Name:", GUILayout.Width(100));
            newStatDisplayName = EditorGUILayout.TextField(newStatDisplayName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type:", GUILayout.Width(100));
            newStatType = (StatType)EditorGUILayout.EnumPopup(newStatType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Value:", GUILayout.Width(100));
            newStatDefaultValue = EditorGUILayout.FloatField(newStatDefaultValue);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add Stat", GUILayout.Height(25))) {
                if (string.IsNullOrEmpty(newStatId)) {
                    EditorUtility.DisplayDialog("Error", "Stat ID cannot be empty", "OK");
                } else if (statsConfig.HasStat(newStatId)) {
                    EditorUtility.DisplayDialog("Error", $"Stat with ID '{newStatId}' already exists", "OK");
                } else {
                    StatDefinition newStat = new StatDefinition(newStatId, newStatDisplayName, newStatType, newStatDefaultValue);
                    statsConfig.AddStat(newStat);
                    EditorUtility.SetDirty(statsConfig);
                    newStatId = "";
                    newStatDisplayName = "";
                    newStatDefaultValue = 0f;
                    Debug.Log($"Added stat: {newStat.displayName}");
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Defined Stats List
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Defined Stats ({statsConfig.stats.Count})", EditorStyles.boldLabel);

            if (statsConfig.stats.Count == 0) {
                EditorGUILayout.HelpBox("No stats defined yet. Add stats above.", MessageType.Info);
            } else {
                statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition, GUILayout.MaxHeight(400));

                List<int> toDelete = new List<int>();

                for (int i = 0; i < statsConfig.stats.Count; i++) {
                    StatDefinition stat = statsConfig.stats[i];

                    Color oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
                    EditorGUILayout.BeginVertical("box");
                    GUI.backgroundColor = oldBg;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(stat.displayName, EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"[{stat.statId}]", EditorStyles.miniLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"{stat.type}", GUILayout.Width(80));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete", GUILayout.Width(60))) {
                        if (EditorUtility.DisplayDialog("Delete Stat", $"Delete stat '{stat.displayName}'?", "Yes", "No")) {
                            toDelete.Add(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;

                    // Editable fields
                    stat.displayName = EditorGUILayout.TextField("Display Name:", stat.displayName);
                    stat.type = (StatType)EditorGUILayout.EnumPopup("Type:", stat.type);
                    stat.defaultValue = EditorGUILayout.FloatField("Default Value:", stat.defaultValue);

                    stat.useMinMax = EditorGUILayout.Toggle("Use Min/Max:", stat.useMinMax);
                    if (stat.useMinMax) {
                        EditorGUI.indentLevel++;
                        stat.minValue = EditorGUILayout.FloatField("Min Value:", stat.minValue);
                        stat.maxValue = EditorGUILayout.FloatField("Max Value:", stat.maxValue);
                        EditorGUI.indentLevel--;
                    }

                    stat.isVisible = EditorGUILayout.Toggle("Visible to Players:", stat.isVisible);
                    stat.description = EditorGUILayout.TextField("Description:", stat.description);

                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.EndScrollView();

                // Delete marked stats
                for (int i = toDelete.Count - 1; i >= 0; i--) {
                    statsConfig.stats.RemoveAt(toDelete[i]);
                    EditorUtility.SetDirty(statsConfig);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Storage Location Info
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Local Data Storage", EditorStyles.boldLabel);

            string statsStoragePath = LocalStorageHelper.GetStoragePath("Aarware_Stats");
            if (statsStoragePath != null) {
                EditorGUILayout.LabelField("Storage Type:", "File-based (Desktop/Mobile)");
                EditorGUILayout.LabelField("File Path:", statsStoragePath);

                if (GUILayout.Button("Open Storage Folder")) {
                    string directory = LocalStorageHelper.GetStorageDirectory();
                    if (directory != null && Directory.Exists(directory)) {
                        EditorUtility.RevealInFinder(directory);
                    } else if (directory != null) {
                        Directory.CreateDirectory(directory);
                        EditorUtility.RevealInFinder(directory);
                    }
                }
            } else {
                EditorGUILayout.LabelField("Storage Type:", "PlayerPrefs (WebGL build target)");
                EditorGUILayout.HelpBox("WebGL builds store data in browser IndexedDB via PlayerPrefs. File access is not available on WebGL.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Save Button
            if (GUILayout.Button("Save Configuration", GUILayout.Height(30))) {
                EditorUtility.SetDirty(statsConfig);
                AssetDatabase.SaveAssets();
                Debug.Log("Stats configuration saved");
                EditorUtility.DisplayDialog("Saved", "Stats configuration saved.", "OK");
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(statsConfig);
            }
        }

        void CreateNewStatsConfiguration() {
            string path = EditorUtility.SaveFilePanelInProject("Create Stats Configuration", "StatsConfiguration", "asset", "Create stats configuration");
            if (!string.IsNullOrEmpty(path)) {
                StatsConfiguration newConfig = StatsConfiguration.CreateDefault();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                statsConfig = newConfig;
                Debug.Log($"Created {path}");
            }
        }

        void CreateNewAchievementsConfiguration() {
            string path = EditorUtility.SaveFilePanelInProject("Create Achievements Configuration", "AchievementsConfiguration", "asset", "Create achievements configuration");
            if (!string.IsNullOrEmpty(path)) {
                AchievementsConfiguration newConfig = AchievementsConfiguration.CreateDefault();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                achievementsConfig = newConfig;
                Debug.Log($"Created {path}");
            }
        }

        void CreateNewLeaderboardsConfiguration() {
            string path = EditorUtility.SaveFilePanelInProject("Create Leaderboards Configuration", "LeaderboardsConfiguration", "asset", "Create leaderboards configuration");
            if (!string.IsNullOrEmpty(path)) {
                LeaderboardsConfiguration newConfig = LeaderboardsConfiguration.CreateDefault();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                leaderboardsConfig = newConfig;
                Debug.Log($"Created {path}");
            }
        }

        void CreateNewNetworkingConfiguration() {
            string path = EditorUtility.SaveFilePanelInProject("Create Networking Configuration", "NetworkingConfiguration", "asset", "Create networking configuration");
            if (!string.IsNullOrEmpty(path)) {
                NetworkingConfiguration newConfig = NetworkingConfiguration.CreateDefault();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                networkingConfig = newConfig;
                Debug.Log($"Created {path}");
            }
        }

        void CreateNewDataStorageConfiguration() {
            string path = EditorUtility.SaveFilePanelInProject("Create Data Storage Configuration", "DataStorageConfiguration", "asset", "Create data storage configuration");
            if (!string.IsNullOrEmpty(path)) {
                DataStorageConfiguration newConfig = DataStorageConfiguration.CreateDefault();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                dataStorageConfig = newConfig;
                Debug.Log($"Created {path}");
            }
        }
        #endregion

        #region Achievements Tab
        void DrawAchievementsTab() {
            GUILayout.Label("Achievements Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Define and manage game achievements. Modeled after Steam's achievement system.", MessageType.Info);

            EditorGUILayout.Space();

            // Configuration Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            AchievementsConfiguration newConfig = EditorGUILayout.ObjectField("Achievements Config:", achievementsConfig, typeof(AchievementsConfiguration), false) as AchievementsConfiguration;
            if (newConfig != achievementsConfig) {
                achievementsConfig = newConfig;
            }

            if (achievementsConfig == null) {
                EditorGUILayout.HelpBox("No configuration selected. Create or select one.", MessageType.Warning);

                if (GUILayout.Button("Create New Achievements Configuration", GUILayout.Height(30))) {
                    CreateNewAchievementsConfiguration();
                }

                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Platform Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Platform Configuration", EditorStyles.boldLabel);

            ServiceStorageMode newMode = (ServiceStorageMode)EditorGUILayout.EnumPopup("Storage Mode:", achievementsConfig.storageMode);
            if (newMode != achievementsConfig.storageMode) {
                achievementsConfig.storageMode = newMode;
                EditorUtility.SetDirty(achievementsConfig);
            }

            // Show cloud platform selector if not LocalOnly
            if (achievementsConfig.storageMode != ServiceStorageMode.LocalOnly) {
                EditorGUI.indentLevel++;
                BackendPlatform newPlatform = (BackendPlatform)EditorGUILayout.EnumPopup("Cloud Platform:", achievementsConfig.cloudPlatform);
                if (newPlatform != achievementsConfig.cloudPlatform) {
                    achievementsConfig.cloudPlatform = newPlatform;
                    EditorUtility.SetDirty(achievementsConfig);
                }

                string platformInfo = GetPlatformInfo(achievementsConfig.cloudPlatform);
                if (!string.IsNullOrEmpty(platformInfo)) {
                    EditorGUILayout.HelpBox(platformInfo, MessageType.Info);
                }

                // Validation warning
                if (!PlatformValidator.IsBackendPlatformValid(currentBuildTarget, achievementsConfig.cloudPlatform)) {
                    string error = PlatformValidator.GetValidationError(currentBuildTarget, achievementsConfig.cloudPlatform);
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }

            // Show mode explanation
            EditorGUILayout.Space();
            string modeInfo = GetStorageModeInfo(achievementsConfig.storageMode);
            EditorGUILayout.HelpBox(modeInfo, MessageType.Info);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Add New Achievement
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Add New Achievement", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Achievement ID:", GUILayout.Width(120));
            newAchievementId = EditorGUILayout.TextField(newAchievementId);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Display Name:", GUILayout.Width(120));
            newAchievementDisplayName = EditorGUILayout.TextField(newAchievementDisplayName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description:", GUILayout.Width(120));
            newAchievementDescription = EditorGUILayout.TextField(newAchievementDescription);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Progressive:", GUILayout.Width(120));
            newAchievementIsProgressive = EditorGUILayout.Toggle(newAchievementIsProgressive);
            EditorGUILayout.EndHorizontal();

            if (newAchievementIsProgressive) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Max Progress:", GUILayout.Width(120));
                newAchievementMaxProgress = EditorGUILayout.FloatField(newAchievementMaxProgress);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Hidden:", GUILayout.Width(120));
            newAchievementIsHidden = EditorGUILayout.Toggle(newAchievementIsHidden);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Point Value:", GUILayout.Width(120));
            newAchievementPointValue = EditorGUILayout.IntField(newAchievementPointValue);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add Achievement", GUILayout.Height(25))) {
                if (string.IsNullOrEmpty(newAchievementId)) {
                    EditorUtility.DisplayDialog("Error", "Achievement ID cannot be empty", "OK");
                } else if (achievementsConfig.HasAchievement(newAchievementId)) {
                    EditorUtility.DisplayDialog("Error", $"Achievement with ID '{newAchievementId}' already exists", "OK");
                } else {
                    AchievementDefinition newAchievement = new AchievementDefinition {
                        achievementId = newAchievementId,
                        displayName = newAchievementDisplayName,
                        description = newAchievementDescription,
                        isProgressive = newAchievementIsProgressive,
                        maxProgress = newAchievementIsProgressive ? newAchievementMaxProgress : 1f,
                        isHidden = newAchievementIsHidden,
                        pointValue = newAchievementPointValue
                    };
                    achievementsConfig.AddAchievement(newAchievement);
                    EditorUtility.SetDirty(achievementsConfig);
                    newAchievementId = "";
                    newAchievementDisplayName = "";
                    newAchievementDescription = "";
                    newAchievementIsProgressive = false;
                    newAchievementMaxProgress = 100f;
                    newAchievementIsHidden = false;
                    newAchievementPointValue = 10;
                    Debug.Log($"Added achievement: {newAchievement.displayName}");
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Defined Achievements List
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Defined Achievements ({achievementsConfig.achievements.Count})", EditorStyles.boldLabel);

            if (achievementsConfig.achievements.Count == 0) {
                EditorGUILayout.HelpBox("No achievements defined yet. Add achievements above.", MessageType.Info);
            } else {
                achievementsScrollPosition = EditorGUILayout.BeginScrollView(achievementsScrollPosition, GUILayout.MaxHeight(400));

                List<int> toDelete = new List<int>();

                for (int i = 0; i < achievementsConfig.achievements.Count; i++) {
                    AchievementDefinition achievement = achievementsConfig.achievements[i];

                    Color oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.9f, 0.6f);
                    EditorGUILayout.BeginVertical("box");
                    GUI.backgroundColor = oldBg;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(achievement.displayName, EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"[{achievement.achievementId}]", EditorStyles.miniLabel, GUILayout.Width(150));
                    if (achievement.isHidden) EditorGUILayout.LabelField("🔒", GUILayout.Width(30));
                    if (achievement.isProgressive) EditorGUILayout.LabelField("📊", GUILayout.Width(30));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete", GUILayout.Width(60))) {
                        if (EditorUtility.DisplayDialog("Delete Achievement", $"Delete achievement '{achievement.displayName}'?", "Yes", "No")) {
                            toDelete.Add(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;

                    // Editable fields
                    achievement.displayName = EditorGUILayout.TextField("Display Name:", achievement.displayName);
                    achievement.description = EditorGUILayout.TextField("Description:", achievement.description);
                    achievement.pointValue = EditorGUILayout.IntField("Point Value:", achievement.pointValue);
                    achievement.iconPath = EditorGUILayout.TextField("Icon Path:", achievement.iconPath);

                    achievement.isHidden = EditorGUILayout.Toggle("Is Hidden:", achievement.isHidden);
                    achievement.isProgressive = EditorGUILayout.Toggle("Is Progressive:", achievement.isProgressive);

                    if (achievement.isProgressive) {
                        EditorGUI.indentLevel++;
                        achievement.maxProgress = EditorGUILayout.FloatField("Max Progress:", achievement.maxProgress);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.EndScrollView();

                // Delete marked achievements
                for (int i = toDelete.Count - 1; i >= 0; i--) {
                    achievementsConfig.achievements.RemoveAt(toDelete[i]);
                    EditorUtility.SetDirty(achievementsConfig);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Runtime Session Data
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Runtime Session Data", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("View player's current achievement progress (runtime data only)", MessageType.Info);

            if (LocalStorageHelper.HasData("Aarware_Achievements")) {
                string json = LocalStorageHelper.LoadData("Aarware_Achievements");
                try {
                    PlayerAchievements achievements = JsonUtility.FromJson<PlayerAchievements>(json);

                    int unlocked = achievements.GetUnlockedAchievements().Count;
                    int total = achievements.achievements.Count;
                    float completion = achievements.GetOverallCompletionPercentage();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Unlocked: {unlocked} / {total}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Completion: {completion:F1}%", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Clear Runtime Data", GUILayout.Height(25))) {
                        if (EditorUtility.DisplayDialog("Clear Runtime Data", "Reset all achievement runtime data?", "Yes", "No")) {
                            LocalStorageHelper.DeleteData("Aarware_Achievements");
                            Debug.Log("Achievement runtime data cleared");
                        }
                    }
                } catch {
                    EditorGUILayout.HelpBox("Failed to parse runtime data.", MessageType.Error);
                }
            } else {
                EditorGUILayout.HelpBox("No runtime data found. Play the game to generate achievement data.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Storage Location Info
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Storage Location", EditorStyles.boldLabel);

            string achievementsStoragePath = LocalStorageHelper.GetStoragePath("Aarware_Achievements");
            if (achievementsStoragePath != null) {
                EditorGUILayout.LabelField("Storage Type:", "File-based (Desktop/Mobile)");
                EditorGUILayout.LabelField("File Path:", achievementsStoragePath);

                if (GUILayout.Button("Open Storage Folder")) {
                    string directory = LocalStorageHelper.GetStorageDirectory();
                    if (directory != null && Directory.Exists(directory)) {
                        EditorUtility.RevealInFinder(directory);
                    } else if (directory != null) {
                        Directory.CreateDirectory(directory);
                        EditorUtility.RevealInFinder(directory);
                    }
                }
            } else {
                EditorGUILayout.LabelField("Storage Type:", "PlayerPrefs (WebGL build target)");
                EditorGUILayout.HelpBox("WebGL builds store data in browser IndexedDB via PlayerPrefs. File access is not available on WebGL.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Networking Tab
        void DrawNetworkingTab() {
            GUILayout.Label("Networking Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure multiplayer networking services", MessageType.Info);

            EditorGUILayout.Space();

            // Configuration Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            NetworkingConfiguration newConfig = EditorGUILayout.ObjectField("Networking Config:", networkingConfig, typeof(NetworkingConfiguration), false) as NetworkingConfiguration;
            if (newConfig != networkingConfig) {
                networkingConfig = newConfig;
            }

            if (networkingConfig == null) {
                EditorGUILayout.HelpBox("No configuration selected. Create or select one.", MessageType.Warning);

                if (GUILayout.Button("Create New Networking Configuration", GUILayout.Height(30))) {
                    CreateNewNetworkingConfiguration();
                }

                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Platform Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Platform Configuration", EditorStyles.boldLabel);

            ServiceStorageMode newMode = (ServiceStorageMode)EditorGUILayout.EnumPopup("Networking Mode:", networkingConfig.storageMode);
            if (newMode != networkingConfig.storageMode) {
                networkingConfig.storageMode = newMode;
                EditorUtility.SetDirty(networkingConfig);
            }

            // Show network platform selector if not LocalOnly
            if (networkingConfig.storageMode != ServiceStorageMode.LocalOnly) {
                EditorGUI.indentLevel++;
                NetworkingPlatform newPlatform = (NetworkingPlatform)EditorGUILayout.EnumPopup("Network Platform:", networkingConfig.networkPlatform);
                if (newPlatform != networkingConfig.networkPlatform) {
                    networkingConfig.networkPlatform = newPlatform;
                    EditorUtility.SetDirty(networkingConfig);
                }

                string platformInfo = GetPlatformInfo(networkingConfig.networkPlatform);
                if (!string.IsNullOrEmpty(platformInfo)) {
                    EditorGUILayout.HelpBox(platformInfo, MessageType.Info);
                }

                // Validation warning
                if (!PlatformValidator.IsNetworkingPlatformValid(currentBuildTarget, networkingConfig.networkPlatform)) {
                    string error = PlatformValidator.GetValidationError(currentBuildTarget, networkingConfig.networkPlatform);
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }

            // Show mode explanation
            EditorGUILayout.Space();
            string modeInfo = "";
            switch (networkingConfig.storageMode) {
                case ServiceStorageMode.LocalOnly:
                    modeInfo = "Local Only: No multiplayer networking. Offline/single-player only.";
                    break;
                case ServiceStorageMode.LocalWithCloudSync:
                    modeInfo = "Local + Cloud: LAN networking with cloud matchmaking support.";
                    break;
                case ServiceStorageMode.CloudOnly:
                    modeInfo = "Cloud Only: Full cloud-based multiplayer using selected platform.";
                    break;
            }
            EditorGUILayout.HelpBox(modeInfo, MessageType.Info);

            // Connection settings
            if (networkingConfig.storageMode != ServiceStorageMode.LocalOnly) {
                EditorGUILayout.Space();
                networkingConfig.maxPlayersPerRoom = EditorGUILayout.IntField("Max Players Per Room:", networkingConfig.maxPlayersPerRoom);
                networkingConfig.connectionTimeout = EditorGUILayout.FloatField("Connection Timeout (s):", networkingConfig.connectionTimeout);
                networkingConfig.autoReconnect = EditorGUILayout.Toggle("Auto Reconnect:", networkingConfig.autoReconnect);
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(networkingConfig);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Platform-Specific Information
            if (networkingConfig.storageMode != ServiceStorageMode.LocalOnly) {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Platform Details", EditorStyles.boldLabel);

                switch (networkingConfig.networkPlatform) {
                    case NetworkingPlatform.Photon:
                        EditorGUILayout.HelpBox("Photon PUN2/Realtime Setup:\n\n1. Import Photon PUN2 from Asset Store\n2. Enter your Photon App ID in PhotonServerSettings\n3. Configure regions and connection settings\n4. Implement PhotonNetworkingProvider", MessageType.Info);
                        break;
                    case NetworkingPlatform.Custom:
                        EditorGUILayout.HelpBox("Custom Networking Setup:\n\n1. Implement INetworkingProvider interface\n2. Create connection logic for your backend\n3. Handle room/lobby management\n4. Implement event-based communication pattern\n5. Support for Node.js and GameLift backends", MessageType.Info);
                        break;
                    case NetworkingPlatform.Steam:
                        EditorGUILayout.HelpBox("Steam Networking Setup:\n\n1. Import Steamworks.NET SDK\n2. Configure Steam App ID\n3. Implement Steam P2P or Steam Sockets\n4. Handle Steam lobby system\n5. Implement SteamNetworkingProvider", MessageType.Info);
                        break;
                    default:
                        EditorGUILayout.HelpBox("Select a networking platform to see setup instructions.", MessageType.Info);
                        break;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Connection Test (Runtime Only)
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Connection Testing", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Network connection testing is only available during Play mode.", MessageType.Info);

            if (Application.isPlaying) {
                EditorGUILayout.LabelField("Status:", "Play Mode Active");
                // You could add real-time connection status here if needed
            } else {
                EditorGUILayout.LabelField("Status:", "Editor Mode (Enter Play mode to test)");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Example Code
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Example Code", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(@"// Setup networking service
var networkingService = new NetworkingService();
networkingService.SetProvider(new PhotonNetworkingProvider());
await networkingService.InitializeAsync();

// Connect to server
await networkingService.ConnectAsync();

// Create room
var roomOptions = new NetworkRoomOptions {
    maxPlayers = 4,
    isVisible = true
};
await networkingService.CreateRoomAsync(""MyRoom"", roomOptions);

// Join room
await networkingService.JoinRoomAsync(""MyRoom"");

// Send network event
networkingService.SendNetworkEvent(""player_action"", data);

// Listen for events
networkingService.OnNetworkEvent += (eventName, data) => {
    Debug.Log($""Received: {eventName}"");
};", MessageType.None);
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Data Storage Tab
        void DrawDataStorageTab() {
            GUILayout.Label("Data Storage Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure save data and storage platform", MessageType.Info);

            EditorGUILayout.Space();

            // Storage Architecture Overview
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Storage Architecture", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "THREE STORAGE TYPES:\n\n" +
                "1. SETTINGS STORAGE (PlayerPrefs)\n" +
                "   • Device settings, volumes, language\n" +
                "   • Session tokens, user preferences\n" +
                "   • Use: SettingsStorage.SetString(...)\n\n" +
                "2. LOCAL GAME DATA (JSON Files)\n" +
                "   • Player progress, inventory, saves\n" +
                "   • Always saved locally (offline support)\n" +
                "   • Desktop/Mobile: persistentDataPath/Saves/\n" +
                "   • WebGL: PlayerPrefs fallback\n\n" +
                "3. CLOUD GAME DATA (Optional Sync)\n" +
                "   • Same as local data, synced to cloud\n" +
                "   • Configure modes below (Local Only, Local+Cloud, Cloud Only)",
                MessageType.None);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Configuration Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            DataStorageConfiguration newConfig = EditorGUILayout.ObjectField("Storage Config:", dataStorageConfig, typeof(DataStorageConfiguration), false) as DataStorageConfiguration;
            if (newConfig != dataStorageConfig) {
                dataStorageConfig = newConfig;
            }

            if (dataStorageConfig == null) {
                EditorGUILayout.HelpBox("No configuration selected. Create or select one.", MessageType.Warning);

                if (GUILayout.Button("Create New Data Storage Configuration", GUILayout.Height(30))) {
                    CreateNewDataStorageConfiguration();
                }

                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Platform Selection
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Platform Configuration", EditorStyles.boldLabel);

            ServiceStorageMode newMode = (ServiceStorageMode)EditorGUILayout.EnumPopup("Storage Mode:", dataStorageConfig.storageMode);
            if (newMode != dataStorageConfig.storageMode) {
                dataStorageConfig.storageMode = newMode;
                EditorUtility.SetDirty(dataStorageConfig);
            }

            // Show cloud platform selector if not LocalOnly
            if (dataStorageConfig.storageMode != ServiceStorageMode.LocalOnly) {
                EditorGUI.indentLevel++;
                BackendPlatform newPlatform = (BackendPlatform)EditorGUILayout.EnumPopup("Cloud Platform:", dataStorageConfig.cloudPlatform);
                if (newPlatform != dataStorageConfig.cloudPlatform) {
                    dataStorageConfig.cloudPlatform = newPlatform;
                    EditorUtility.SetDirty(dataStorageConfig);
                }

                string platformInfo = GetPlatformInfo(dataStorageConfig.cloudPlatform);
                if (!string.IsNullOrEmpty(platformInfo)) {
                    EditorGUILayout.HelpBox(platformInfo, MessageType.Info);
                }

                // Validation warning
                if (!PlatformValidator.IsBackendPlatformValid(currentBuildTarget, dataStorageConfig.cloudPlatform)) {
                    string error = PlatformValidator.GetValidationError(currentBuildTarget, dataStorageConfig.cloudPlatform);
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }

            // Show mode explanation
            EditorGUILayout.Space();
            string modeInfo = GetStorageModeInfo(dataStorageConfig.storageMode);
            EditorGUILayout.HelpBox(modeInfo, MessageType.Info);

            EditorGUILayout.Space();

            dataStorageConfig.enableCloudSync = EditorGUILayout.Toggle("Enable Cloud Sync:", dataStorageConfig.enableCloudSync);
            dataStorageConfig.autoSaveInterval = EditorGUILayout.FloatField("Auto-Save Interval (s):", dataStorageConfig.autoSaveInterval);
            dataStorageConfig.maxSaveSlots = EditorGUILayout.IntField("Max Save Slots:", dataStorageConfig.maxSaveSlots);
            dataStorageConfig.encryptSaveData = EditorGUILayout.Toggle("Encrypt Save Data:", dataStorageConfig.encryptSaveData);
            dataStorageConfig.createBackups = EditorGUILayout.Toggle("Create Backups:", dataStorageConfig.createBackups);
            if (dataStorageConfig.createBackups) {
                EditorGUI.indentLevel++;
                dataStorageConfig.maxBackupsPerSlot = EditorGUILayout.IntField("Max Backups Per Slot:", dataStorageConfig.maxBackupsPerSlot);
                EditorGUI.indentLevel--;
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(dataStorageConfig);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Save Location Info
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Save Data Location", EditorStyles.boldLabel);
            string savePath = Path.Combine(Application.persistentDataPath, "Saves");
            EditorGUILayout.LabelField("Path:", savePath);

            if (GUILayout.Button("Open Save Folder")) {
                if (Directory.Exists(savePath)) {
                    EditorUtility.RevealInFinder(savePath);
                } else {
                    Directory.CreateDirectory(savePath);
                    EditorUtility.RevealInFinder(savePath);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Current Save Files
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Save Files", EditorStyles.boldLabel);

            if (Directory.Exists(savePath)) {
                string[] files = Directory.GetFiles(savePath, "*.json");

                if (files.Length > 0) {
                    EditorGUILayout.LabelField($"Total Files: {files.Length}");
                    EditorGUILayout.Space();

                    foreach (string file in files) {
                        EditorGUILayout.BeginHorizontal("box");
                        FileInfo info = new FileInfo(file);
                        EditorGUILayout.LabelField(Path.GetFileName(file), GUILayout.Width(200));
                        EditorGUILayout.LabelField($"{FormatBytes(info.Length)}", GUILayout.Width(80));
                        EditorGUILayout.LabelField(info.LastWriteTime.ToString("g"), GUILayout.Width(150));

                        if (GUILayout.Button("Delete", GUILayout.Width(60))) {
                            if (EditorUtility.DisplayDialog("Delete Save", $"Delete {Path.GetFileName(file)}?", "Yes", "No")) {
                                File.Delete(file);
                                Debug.Log($"Deleted {file}");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Clear All Save Data", GUILayout.Height(30))) {
                        if (EditorUtility.DisplayDialog("Clear All", "Delete all save files?", "Yes", "Cancel")) {
                            foreach (string file in files) {
                                File.Delete(file);
                            }
                            Debug.Log("All save files deleted");
                        }
                    }
                } else {
                    EditorGUILayout.HelpBox("No save files found.", MessageType.Info);
                }
            } else {
                EditorGUILayout.HelpBox($"Save directory doesn't exist yet: {savePath}", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Example Code
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Example Code", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(@"// Setup data storage service
var storageService = new DataStorageService();
storageService.SetProvider(new LocalDataStorageProvider());
await storageService.InitializeAsync();

// Save data
var saveData = new SaveData {
    playerName = ""Player1"",
    level = 10,
    score = 5000
};
await storageService.SaveAsync(""slot1"", saveData);

// Load data
var result = await storageService.LoadAsync<SaveData>(""slot1"");
if (result.Success) {
    SaveData loaded = result.Data;
    Debug.Log($""Loaded: {loaded.playerName}"");
}", MessageType.None);
            EditorGUILayout.EndVertical();
        }

        string GetPlatformInfo(BackendPlatform platform) {
            switch (platform) {
                case BackendPlatform.Local:
                    return "Local storage. Works offline.";
                case BackendPlatform.Steam:
                    return "Requires Steamworks.NET SDK.";
                case BackendPlatform.GooglePlay:
                    return "Requires Google Play Games SDK (Android).";
                case BackendPlatform.UniversalWindows:
                    return "Requires Microsoft GDK/Xbox Live SDK.";
                case BackendPlatform.Custom:
                    return "Custom backend implementation.";
                default:
                    return "";
            }
        }

        string GetPlatformInfo(NetworkingPlatform platform) {
            switch (platform) {
                case NetworkingPlatform.Local:
                    return "Local/offline networking (no multiplayer or LAN only).";
                case NetworkingPlatform.Photon:
                    return "Requires Photon PUN2 SDK.";
                case NetworkingPlatform.Steam:
                    return "Requires Steamworks.NET SDK (P2P networking).";
                case NetworkingPlatform.Custom:
                    return "Custom networking implementation.";
                default:
                    return "";
            }
        }

        string GetStorageModeInfo(ServiceStorageMode mode) {
            switch (mode) {
                case ServiceStorageMode.LocalOnly:
                    return "Local Only: Data stored locally only. Works completely offline. No cloud sync.";
                case ServiceStorageMode.LocalWithCloudSync:
                    return "Local + Cloud Sync: Data stored locally and synced to cloud when available. Best UX - works offline and online.";
                case ServiceStorageMode.CloudOnly:
                    return "Cloud Only: Data stored only in the cloud. Requires connection to platform. No local fallback.";
                default:
                    return "";
            }
        }

        // Build target validation (skeleton for future implementation)
        #endregion

        #region Scenes Tab
        void DrawScenesTab() {
            GUILayout.Label("Scene Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Manage build scenes and generate scene enums", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Scenes", GUILayout.Width(120))) {
                ScanScenes();
            }
            if (GUILayout.Button("Generate Scene Enum", GUILayout.Height(25))) {
                GenerateSceneEnum();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            DrawSceneCategory("Main Scenes", mainScenes, Color.green);
            DrawSceneCategory("Additive Scenes", additiveScenes, Color.cyan);
            DrawSceneCategory("Testing Scenes", testingScenes, Color.yellow);
            DrawSceneCategory("Other Scenes", otherScenes, Color.gray);
        }

        void DrawSceneCategory(string categoryName, List<SceneInfo> scenes, Color color) {
            if (scenes.Count == 0) return;

            EditorGUILayout.Space();

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = oldColor;

            GUILayout.Label($"{categoryName} ({scenes.Count})", EditorStyles.boldLabel);

            foreach (SceneInfo scene in scenes) {
                EditorGUILayout.BeginHorizontal();

                bool newEnabled = EditorGUILayout.Toggle(scene.enabledInBuild, GUILayout.Width(20));
                if (newEnabled != scene.enabledInBuild) {
                    scene.enabledInBuild = newEnabled;
                    UpdateBuildSettings();
                }

                EditorGUILayout.LabelField(scene.name, GUILayout.MinWidth(150));
                EditorGUILayout.LabelField(scene.path, EditorStyles.miniLabel);

                if (GUILayout.Button("Open", GUILayout.Width(60))) {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        void ScanScenes() {
            mainScenes.Clear();
            additiveScenes.Clear();
            testingScenes.Clear();
            otherScenes.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Scene");

            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = Path.GetFileNameWithoutExtension(path);

                SceneInfo info = new SceneInfo {
                    name = sceneName,
                    path = path,
                    enabledInBuild = IsSceneInBuildSettings(path)
                };

                if (path.Contains("/Scenes/") && !path.Contains("/Additive") && !path.Contains("/Testing")) {
                    mainScenes.Add(info);
                } else if (path.Contains("/Additive")) {
                    additiveScenes.Add(info);
                } else if (path.Contains("/Testing") || path.Contains("/Test")) {
                    testingScenes.Add(info);
                } else {
                    otherScenes.Add(info);
                }
            }

            Repaint();
        }

        bool IsSceneInBuildSettings(string scenePath) {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                if (scene.path == scenePath) {
                    return scene.enabled;
                }
            }
            return false;
        }

        void UpdateBuildSettings() {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            AddScenesToBuild(buildScenes, mainScenes);
            AddScenesToBuild(buildScenes, additiveScenes);
            AddScenesToBuild(buildScenes, testingScenes);
            AddScenesToBuild(buildScenes, otherScenes);

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("Build settings updated");
        }

        void AddScenesToBuild(List<EditorBuildSettingsScene> buildScenes, List<SceneInfo> scenes) {
            foreach (SceneInfo scene in scenes) {
                buildScenes.Add(new EditorBuildSettingsScene(scene.path, scene.enabledInBuild));
            }
        }

        void GenerateSceneEnum() {
            List<string> allEnabledScenes = new List<string>();

            allEnabledScenes.AddRange(mainScenes.Where(s => s.enabledInBuild).Select(s => s.name));
            allEnabledScenes.AddRange(additiveScenes.Where(s => s.enabledInBuild).Select(s => s.name));
            allEnabledScenes.AddRange(testingScenes.Where(s => s.enabledInBuild).Select(s => s.name));
            allEnabledScenes.AddRange(otherScenes.Where(s => s.enabledInBuild).Select(s => s.name));

            if (allEnabledScenes.Count == 0) {
                EditorUtility.DisplayDialog("No Scenes", "No enabled scenes found.", "OK");
                return;
            }

            string code = GenerateEnumCode(allEnabledScenes);

            string savePath = "Assets/Scripts/Generated";
            if (!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }

            string filePath = Path.Combine(savePath, "SceneNames.cs");
            File.WriteAllText(filePath, code);

            AssetDatabase.Refresh();

            Debug.Log($"Scene enum generated at {filePath}");
            EditorUtility.DisplayDialog("Success", $"Generated at:\n{filePath}", "OK");
        }

        string GenerateEnumCode(List<string> sceneNames) {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// Auto-generated by Aarware Scene Manager");
            sb.AppendLine("// Do not edit manually!");
            sb.AppendLine();
            sb.AppendLine("namespace Aarware {");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Auto-generated enum for all scenes in build.");
            sb.AppendLine("    /// Use SceneName.ToString() to get scene name.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public enum SceneNames {");

            for (int i = 0; i < sceneNames.Count; i++) {
                string sceneName = sceneNames[i];
                string enumName = SanitizeEnumName(sceneName);

                if (i < sceneNames.Count - 1) {
                    sb.AppendLine($"        {enumName},");
                } else {
                    sb.AppendLine($"        {enumName}");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        string SanitizeEnumName(string name) {
            name = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (char.IsDigit(name[0])) {
                name = "_" + name;
            }
            return name;
        }

        class SceneInfo {
            public string name;
            public string path;
            public bool enabledInBuild;
        }
        #endregion
    }
}
