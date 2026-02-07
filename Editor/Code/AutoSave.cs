#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

namespace PM.Tools
{
   public class AutoSave : EditorWindow
   {
      // Cached GUIStyle - initialized once
      private GUIStyle _guiStyleLabel;
      private bool _guiStyleInitialized;

      private const string LOGColor = "#CC0000";
      private const int LOGSize = 14;

      private const string EditorPrefPrefix = "PM_AS_";
      private const string AutoSaveKey = EditorPrefPrefix + "AUTOSAVE";
      private const string SaveOnPlayKey = EditorPrefPrefix + "SAVEONPLAY";
      private const string SaveAssetsKey = EditorPrefPrefix + "SAVEASSET";
      private const string DebugLogKey = EditorPrefPrefix + "DEBUGLOG";
      private const string SaveIntervalKey = EditorPrefPrefix + "SAVEINTERVAL";
      private const string BackupKey = EditorPrefPrefix + "BACKUP";
      private const string BackupPathKey = EditorPrefPrefix + "BACKUPPATH";
      private const string BackupCountKey = EditorPrefPrefix + "BACKUPCOUNT";
      private const string BackupPrefabKey = EditorPrefPrefix + "BACKUPPREFAB";
      private const string MachineIdentifierKey = EditorPrefPrefix + "MACHINEIDENTIFIER";

      private static bool _autoSave;
      private static bool _saveOnPlay;
      private static bool _saveAssets;
      private static bool _debugLog;
      private static bool _backup;
      private static bool _showBackup;
      private static bool _backupPrefab;

      // Race condition prevention
      private static bool _isSaving;

      // Cached dirty state to avoid repeated SceneManager iterations
      private static bool _hasAnyDirtyScene;

      // Timer tracking - uses EditorApplication.timeSinceStartup instead of DateTime.Now
      // for better performance (no memory allocation on each access)
      private static double _lastAutosave;

      private static int _saveInterval = 5;
      private static int _saveIntervalSlider = 5;

      private static string _backupPath;
      private static int _backupCount;

      // Cached textures - loaded once in OnEnable
      private Texture2D _cachedAsset;
      private Texture2D _cachedDisable;
      private Texture2D _cachedEnable;
      private Texture2D _cachedInfo;
      private Texture2D _cachedOnOff;
      private Texture2D _cachedOnPlay;
      private Texture2D _cachedTime;
      private Texture2D _cachedBackup;
      private Texture2D _cachedLogo;

      // Cached texture path - calculated once
      private string _cachedTexturePath;

      // Status indicator
      private static string _lastSaveStatus = "";
      private static double _statusDisplayTime;
      private const double StatusDisplayDuration = 3.0;

      // Backup path validation
      private static string _backupPathError = "";

      private string TexturePath
      {
         get
         {
            if (string.IsNullOrEmpty(_cachedTexturePath))
            {
               string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
               path = path.Substring(0, path.LastIndexOf('/'));
               path = path.Substring(0, path.LastIndexOf('/') + 1);
               _cachedTexturePath = path + "Textures/";
            }
            return _cachedTexturePath;
         }
      }

      private void CacheTextures()
      {
         string path = TexturePath;
         _cachedAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "asset.png");
         _cachedDisable = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "disable.png");
         _cachedEnable = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "enable.png");
         _cachedInfo = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "info.png");
         _cachedOnOff = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "onoff.png");
         _cachedOnPlay = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "play.png");
         _cachedTime = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "time.png");
         _cachedBackup = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "backup.png");
         _cachedLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "logo.png");
      }

      private void InitializeGUIStyle()
      {
         if (_guiStyleInitialized) return;

         _guiStyleLabel = new GUIStyle
         {
            fontSize = 12,
            fontStyle = FontStyle.Italic
         };
         _guiStyleLabel.normal.textColor = new Color(.58f, .58f, .58f);
         _guiStyleInitialized = true;
      }

      private void UpdateWindowTitle()
      {
         titleContent = new GUIContent(_autoSave ? "AutoSave - Enabled" : "AutoSave - Disabled");
      }

      private static void UpdateAllWindowTitles()
      {
         // Find all open AutoSave windows and update their titles
         var windows = Resources.FindObjectsOfTypeAll<AutoSave>();
         foreach (var window in windows)
         {
            window.UpdateWindowTitle();
         }
      }

      private static void AutosaveOff()
      {
         // Always unsubscribe first to prevent duplicates
         EditorApplication.update -= EditorUpdate;
         EditorApplication.playModeStateChanged -= OnEnterInPlayMode;
         EditorSceneManager.sceneDirtied -= OnSceneDirtied;
         EditorSceneManager.sceneSaved -= OnSceneSaved;
         _autoSave = false;
         Log(0, "OFF !");
         UpdateAllWindowTitles();
      }

      private static void AutosaveOn()
      {
         // Unsubscribe first to prevent duplicate registrations
         EditorApplication.update -= EditorUpdate;
         EditorApplication.playModeStateChanged -= OnEnterInPlayMode;
         EditorSceneManager.sceneDirtied -= OnSceneDirtied;
         EditorSceneManager.sceneSaved -= OnSceneSaved;

         _lastAutosave = EditorApplication.timeSinceStartup;
         EditorApplication.update += EditorUpdate;
         EditorApplication.playModeStateChanged += OnEnterInPlayMode;
         EditorSceneManager.sceneDirtied += OnSceneDirtied;
         EditorSceneManager.sceneSaved += OnSceneSaved;
         _autoSave = true;
         Log(0, "ON !");
         UpdateAllWindowTitles();
      }

      private static void OnSceneDirtied(Scene scene)
      {
         _hasAnyDirtyScene = true;
      }

      private static void OnSceneSaved(Scene scene)
      {
         // Check if any scenes are still dirty after this save
         _hasAnyDirtyScene = false;
         for (int i = 0; i < SceneManager.sceneCount; i++)
         {
            if (SceneManager.GetSceneAt(i).isDirty)
            {
               _hasAnyDirtyScene = true;
               break;
            }
         }
      }

      private static void EditorUpdate()
      {
         // Use EditorApplication.timeSinceStartup instead of DateTime.Now for performance:
         // - No memory allocation on each access (DateTime.Now creates new objects)
         // - Lightweight double value representing seconds since Unity started
         // - Specifically designed for Unity editor timing comparisons
         if (EditorApplication.timeSinceStartup - _lastAutosave < _saveInterval * 60.0) return;

         // Always reset timer to prevent checking every frame
         _lastAutosave = EditorApplication.timeSinceStartup;

         if (_hasAnyDirtyScene)
         {
            Save();
            _hasAnyDirtyScene = false;
         }
      }

      private static TimeSpan GetTimeUntilNextSave()
      {
         double elapsed = EditorApplication.timeSinceStartup - _lastAutosave;
         double remainingSeconds = (_saveInterval * 60.0) - elapsed;

         // Return zero if time has already passed
         return remainingSeconds > 0 ? TimeSpan.FromSeconds(remainingSeconds) : TimeSpan.Zero;
      }

      private static void LoadSettings()
      {
         _autoSave = EditorPrefs.GetBool(AutoSaveKey, true);
         _saveOnPlay = EditorPrefs.GetBool(SaveOnPlayKey, true);
         _saveAssets = EditorPrefs.GetBool(SaveAssetsKey, true);
         _debugLog = EditorPrefs.GetBool(DebugLogKey, false);
         _saveInterval = EditorPrefs.GetInt(SaveIntervalKey, 2);
         _saveIntervalSlider = EditorPrefs.GetInt(SaveIntervalKey, 2);
         _backup = EditorPrefs.GetBool(BackupKey, true);
         _backupPath = EditorPrefs.GetString(BackupPathKey, "_project/AutoSave");
         _backupCount = EditorPrefs.GetInt(BackupCountKey, 10);
         _backupPrefab = EditorPrefs.GetBool(BackupPrefabKey, true);
      }

      private static void SaveSettings()
      {
         EditorPrefs.SetBool(AutoSaveKey, _autoSave);
         EditorPrefs.SetBool(SaveOnPlayKey, _saveOnPlay);
         EditorPrefs.SetBool(SaveAssetsKey, _saveAssets);
         EditorPrefs.SetBool(DebugLogKey, _debugLog);
         EditorPrefs.SetBool(BackupKey, _backup);
         EditorPrefs.SetInt(SaveIntervalKey, _saveInterval);
         EditorPrefs.SetString(BackupPathKey, _backupPath);
         EditorPrefs.SetInt(BackupCountKey, _backupCount);
         EditorPrefs.SetBool(BackupPrefabKey, _backupPrefab);
      }

      private static string GetMachineIdentifier()
      {
         string identifier = EditorPrefs.GetString(MachineIdentifierKey, string.Empty);

         if (string.IsNullOrEmpty(identifier))
         {
            identifier = Guid.NewGuid().ToString();
            EditorPrefs.SetString(MachineIdentifierKey, identifier);
         }

         return identifier;
      }

      private static void OnEnterInPlayMode(PlayModeStateChange state)
      {
         if (_saveOnPlay && !EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            Save();
      }

      [MenuItem("Tools/ProjectMakers/AutoSave")]
      private static void OpenWindow()
      {
         GetWindow<AutoSave>("AutoSave");
      }

      private static void Save()
      {
         // Prevent race condition - don't save if already saving
         if (_isSaving) return;
         _isSaving = true;

         try
         {
            Scene activeScene = SceneManager.GetActiveScene();
            bool saveSuccessful = SaveAllDirtyScenes();

            if (_saveAssets)
               AssetDatabase.SaveAssets();

            if (saveSuccessful)
            {
               Log(0, $"Scene '{activeScene.name}' has been saved.");
               _lastSaveStatus = $"Saved: {activeScene.name}";
               _statusDisplayTime = EditorApplication.timeSinceStartup;
            }

            if (!_backup)
               return;

            BackupActiveScene(activeScene);
         }
         finally
         {
            _isSaving = false;
         }
      }

      private static bool SaveAllDirtyScenes()
      {
         bool allSuccessful = true;

         for (int i = 0; i < SceneManager.sceneCount; i++)
         {
            var scene = SceneManager.GetSceneAt(i);

            if (!scene.isDirty) continue;

            // Skip unsaved new scenes (no path) - they require user interaction
            if (string.IsNullOrEmpty(scene.path))
            {
               Log(1, $"Scene '{scene.name}' has no path. Please save it manually first.");
               continue;
            }

            try
            {
               EditorSceneManager.SaveScene(scene);
            }
            catch (Exception e)
            {
               Log(2, $"Error occurred while saving scene '{scene.name}'.\nException: {e}");
               allSuccessful = false;
            }
         }

         return allSuccessful;
      }

      private static bool ValidateBackupPath(string path)
      {
         // Prevent path traversal attacks
         if (string.IsNullOrWhiteSpace(path))
         {
            _backupPathError = "Backup path cannot be empty";
            return false;
         }

         try
         {
            // Check for null byte injection
            if (path.Contains("\0"))
            {
               _backupPathError = "Path contains null bytes";
               Log(1, "Invalid backup path: Path contains null bytes.");
               return false;
            }

            // Check for invalid path characters
            char[] invalidChars = Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0)
            {
               _backupPathError = "Path contains invalid characters";
               Log(1, "Invalid backup path: Path contains invalid characters.");
               return false;
            }

            // Check for absolute paths (drive letters or network paths)
            if (Path.IsPathRooted(path))
            {
               _backupPathError = "Absolute paths not allowed";
               Log(1, "Invalid backup path: Absolute paths are not allowed.");
               return false;
            }

            // Check for network paths (UNC paths like \\server\share)
            if (path.StartsWith("\\\\") || path.StartsWith("//"))
            {
               _backupPathError = "Network paths not allowed";
               Log(1, "Invalid backup path: Network paths are not allowed.");
               return false;
            }

            // Get the full path to the Assets folder
            string assetsPath = Path.GetFullPath(Application.dataPath);

            // Construct and normalize the proposed backup path
            string proposedPath = Path.Combine(Application.dataPath, path);
            string normalizedPath = Path.GetFullPath(proposedPath);

            // Check for maximum path length
            if (normalizedPath.Length > 260)
            {
               _backupPathError = "Path exceeds maximum length";
               Log(1, "Invalid backup path: Path exceeds maximum length.");
               return false;
            }

            // Ensure the normalized path stays within the Assets folder
            if (!normalizedPath.StartsWith(assetsPath + Path.DirectorySeparatorChar) &&
                !normalizedPath.Equals(assetsPath))
            {
               _backupPathError = "Path must be within Assets folder";
               Log(1, "Invalid backup path: Path must be within the Assets folder.");
               return false;
            }

            // Clear error when validation passes
            _backupPathError = "";
            return true;
         }
         catch (Exception e)
         {
            _backupPathError = $"Invalid path: {e.Message}";
            Log(1, $"Invalid backup path: {e.Message}");
            return false;
         }
      }

      private static void BackupActiveScene(Scene activeScene)
      {
         if (!ValidateBackupPath(_backupPath))
         {
            Log(2, "Backup skipped due to invalid backup path.");
            return;
         }

         var username = GetMachineIdentifier();
         var curSceneName = activeScene.name;
         var fileName = BackupFileName(curSceneName);
         var path = Path.Combine("Assets", _backupPath, username, curSceneName);
         var filePath = Path.Combine(path, fileName);

         if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

         try
         {
            EditorSceneManager.SaveScene(activeScene, filePath, true);
            Log(0, $"Backup created for scene '{curSceneName}'.");

            // Deferred cleanup to avoid blocking
            EditorApplication.delayCall += () => ClearBackupFolder(path, filePath);
         }
         catch (Exception e)
         {
            Log(2, $"Error occurred while creating a backup for scene '{curSceneName}'.\nException: {e}");
         }
      }

      internal static void BackupModifiedPrefabs(string[] assetPaths)
      {
         if (!_backupPrefab)
            return;

         if (!ValidateBackupPath(_backupPath))
         {
            Log(2, "Prefab backup skipped due to invalid backup path.");
            return;
         }

         var username = GetMachineIdentifier();

         // Filter for prefab files only
         var prefabPaths = assetPaths.Where(path => path.EndsWith(".prefab")).ToArray();

         if (prefabPaths.Length == 0)
            return;

         foreach (var assetPath in prefabPaths)
         {
            try
            {
               // Extract prefab name without extension
               var prefabFileName = Path.GetFileNameWithoutExtension(assetPath);
               var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
               var fileName = $"{prefabFileName} v.{timestamp}.prefab";
               var path = Path.Combine("Assets", _backupPath, username, prefabFileName);
               var filePath = Path.Combine(path, fileName);

               // Skip if source prefab doesn't exist (e.g. newly created and not yet on disk)
               if (!File.Exists(assetPath))
               {
                  Log(1, $"Prefab '{prefabFileName}' not found on disk, skipping backup.");
                  continue;
               }

               if (!Directory.Exists(path))
                  Directory.CreateDirectory(path);

               // Use File.Copy instead of AssetDatabase.CopyAsset to avoid
               // triggering additional AssetDatabase operations
               File.Copy(assetPath, filePath, true);
               Log(0, $"Backup created for prefab '{prefabFileName}'.");

               // Deferred cleanup to avoid blocking
               EditorApplication.delayCall += () => ClearBackupFolder(path, filePath, "*.prefab");
            }
            catch (Exception e)
            {
               var prefabName = Path.GetFileNameWithoutExtension(assetPath);
               Log(2, $"Error occurred while creating a backup for prefab '{prefabName}'.\nException: {e}");
            }
         }
      }

      private static void ClearBackupFolder(string path, string newFilePath)
      {
         ClearBackupFolder(path, newFilePath, "*.unity");
      }

      private static void ClearBackupFolder(string path, string newFilePath, string filePattern)
      {
         try
         {
            var fileInfo = new DirectoryInfo(path).GetFiles(filePattern);

            if (fileInfo.Length > _backupCount)
            {
               // O(n) algorithm to find oldest file instead of O(n log n) sort
               FileInfo oldestFile = null;
               DateTime oldestTime = DateTime.MaxValue;

               foreach (var file in fileInfo)
               {
                  if (file.LastWriteTime < oldestTime)
                  {
                     oldestTime = file.LastWriteTime;
                     oldestFile = file;
                  }
               }

               if (oldestFile != null)
               {
                  var metaFilePath = oldestFile.FullName + ".meta";

                  oldestFile.Delete();

                  if (File.Exists(metaFilePath))
                     File.Delete(metaFilePath);
               }
            }

            // Only refresh the specific backup folder, not entire project
            if (!string.IsNullOrEmpty(newFilePath))
            {
               AssetDatabase.ImportAsset(newFilePath);
            }
         }
         catch (Exception e)
         {
            Log(1, $"Warning: Could not clean up old backups. {e.Message}");
         }
      }

      private static string BackupFileName(string curSceneName)
      {
         var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
         return $"{curSceneName} v.{timestamp}.unity";
      }

      private static void Log(int type, string body)
      {
         if (!_debugLog) return;

         string prefix = $"<color={LOGColor}><size={LOGSize}><b>PM - Autosave: </b></size></color>";

         switch (type)
         {
            case 1:
               Debug.LogWarning(prefix + body);
               break;
            case 2:
               Debug.LogError(prefix + body);
               break;
            default:
               Debug.Log(prefix + body);
               break;
         }
      }

      private void OnEnable()
      {
         CacheTextures();
         LoadSettings();

         if (_autoSave)
            AutosaveOn();
         else
            AutosaveOff();
      }

      private void OnDisable()
      {
         // Don't remove event handlers here - AutoSave should continue running
         // even when the window is closed. Handlers are static and managed by
         // AutosaveOn()/AutosaveOff(). Removing them here would stop AutoSave
         // unexpectedly when the user just closes the settings window.
      }

      private void OnGUI()
      {
         InitializeGUIStyle();
         EditorGUI.BeginChangeCheck();

         if (_saveInterval != _saveIntervalSlider)
         {
            _saveInterval = _saveIntervalSlider;
            Log(0, "Saveinterval = " + _saveInterval + " min!");
         }

         GUILayout.Space(20);
         GUILayout.BeginHorizontal();
         GUILayout.FlexibleSpace();
         if (_cachedLogo != null)
            GUILayout.Label(_cachedLogo);
         GUILayout.FlexibleSpace();
         GUILayout.EndHorizontal();
         GUILayout.Space(10);
         EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

         // Status indicator
         if (!string.IsNullOrEmpty(_lastSaveStatus) &&
             EditorApplication.timeSinceStartup - _statusDisplayTime < StatusDisplayDuration)
         {
            EditorGUILayout.HelpBox(_lastSaveStatus, MessageType.Info);
         }

         // Countdown timer display
         if (_autoSave)
         {
            TimeSpan timeRemaining = GetTimeUntilNextSave();
            int minutes = (int)timeRemaining.TotalMinutes;
            int seconds = timeRemaining.Seconds;
            string countdownText = $"Next autosave in: {minutes:D2}:{seconds:D2}";
            EditorGUILayout.HelpBox(countdownText, MessageType.None);
            Repaint();
         }

         GUILayout.Space(10);
         GUILayout.BeginHorizontal();
         DrawButton(ref _debugLog, _cachedInfo, "Toggle debug logging to console", "Debug.Log");
         DrawButton(ref _saveOnPlay, _cachedOnPlay, "Automatically save before entering Play mode", "Save on Play");
         DrawButton(ref _saveAssets, _cachedAsset, "Also save modified assets (ScriptableObjects, prefabs, etc.)", "Save Assets");
         GUILayout.FlexibleSpace();
         GUILayout.BeginVertical();
         GUILayout.BeginHorizontal();
         GUILayout.Space(10);
         GUILayout.Label(string.Empty, GUILayout.MaxHeight(16), GUILayout.MaxWidth(16));
         GUILayout.EndHorizontal();
         GUILayout.Space(2);
         GUILayout.BeginHorizontal();

         _saveIntervalSlider = EditorGUILayout.IntSlider(string.Empty, _saveIntervalSlider, 1, 30);

         GUILayout.BeginVertical();
         GUILayout.Space(-4);
         EditorGUILayout.LabelField(new GUIContent(_cachedTime, "Auto-save interval in minutes (1-30). Saves only when scenes have unsaved changes."), GUILayout.MaxHeight(28), GUILayout.MaxWidth(28));
         GUILayout.EndVertical();
         GUILayout.EndHorizontal();
         GUILayout.EndVertical();

         bool previousAutoSave = _autoSave;
         DrawButton(ref _autoSave, _cachedOnOff, "Enable or disable automatic saving", "AutoSave");

         // Call AutosaveOn/Off when state changes via GUI button
         if (_autoSave != previousAutoSave)
         {
            if (_autoSave)
               AutosaveOn();
            else
               AutosaveOff();

            // Update title immediately for instant visual feedback
            UpdateWindowTitle();
         }

         GUILayout.EndHorizontal();
         GUILayout.Space(10);
         GUILayout.BeginHorizontal();

         // Save and restore GUI colors
         Color originalTextColor = GUI.skin.button.normal.textColor;
         Color originalBgColor = GUI.backgroundColor;

         GUI.skin.button.normal.textColor = Color.white;
         GUI.backgroundColor = new Color(0.63f, 0f, 0f);

         if (GUILayout.Button("Save it manually!", EditorStyles.toolbarButton))
            Save();

         if (GUILayout.Button("Change backup settings...", EditorStyles.toolbarButton))
            _showBackup = !_showBackup;

         // Restore GUI colors
         GUI.skin.button.normal.textColor = originalTextColor;
         GUI.backgroundColor = originalBgColor;

         GUILayout.EndHorizontal();

         if (_showBackup)
         {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            DrawButton(ref _backup, _cachedBackup, "Create timestamped backup copies of scenes", "Backup");
            GUILayout.Label("Scene", _guiStyleLabel);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            DrawButton(ref _backupPrefab, _cachedBackup, "Create timestamped backup copies of prefabs", "Prefab Backup");
            GUILayout.Label("Prefab", _guiStyleLabel);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            EditorGUIUtility.labelWidth = 115;
            string newBackupPath = EditorGUILayout.TextField(
               new GUIContent("Backup save path:", "Relative path inside Assets/ folder (e.g., '_project/AutoSave')"),
               _backupPath, GUILayout.ExpandWidth(true));

            // Validate and update backup path
            if (newBackupPath != _backupPath)
            {
               if (ValidateBackupPath(newBackupPath))
               {
                  _backupPath = newBackupPath;
               }
            }

            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            // Display inline validation error if present
            if (!string.IsNullOrEmpty(_backupPathError))
            {
               EditorGUILayout.HelpBox(_backupPathError, MessageType.Error);
            }

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            EditorGUIUtility.labelWidth = 88;
            int newBackupCount = EditorGUILayout.IntField(
               new GUIContent("Backup count:", "Maximum number of backup files to keep per scene"),
               _backupCount);

            // Use field instead of direct EditorPrefs access
            if (newBackupCount != _backupCount)
            {
               _backupCount = Mathf.Clamp(newBackupCount, 1, 100);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Backups are saved to: Assets/<your path>/<hostname>/<scene name>/", _guiStyleLabel);
         }

         GUILayout.Space(10);

         GUILayout.FlexibleSpace();
         GUILayout.BeginHorizontal();

         if (GUILayout.Button("ProjectMakers.de", EditorStyles.toolbarButton))
            Application.OpenURL("https://projectmakers.de");

         GUILayout.EndHorizontal();

         if (EditorGUI.EndChangeCheck())
            SaveSettings();
      }

      private void DrawButton(ref bool buttonState, Texture buttonInfoText, string buttonTooltip, string logMessage)
      {
         GUILayout.BeginVertical();
         GUILayout.BeginHorizontal();
         GUILayout.Space(10);
         Texture2D stateTexture = buttonState ? _cachedEnable : _cachedDisable;
         if (stateTexture != null)
            GUILayout.Label(stateTexture, GUILayout.MaxHeight(16), GUILayout.MaxWidth(16));
         else
            GUILayout.Label(buttonState ? "ON" : "OFF", GUILayout.MaxHeight(16), GUILayout.MaxWidth(16));
         GUILayout.EndHorizontal();

         if (GUILayout.Button(new GUIContent(buttonInfoText, buttonTooltip), GUILayout.MaxHeight(28), GUILayout.MaxWidth(28)))
         {
            buttonState = !buttonState;
            Log(0, $"{logMessage} = {buttonState} !");
            // SaveSettings() is called via EditorGUI.EndChangeCheck() in OnGUI()
         }

         GUILayout.EndVertical();
      }
   }

   /// <summary>
   /// Asset processor that intercepts save operations to backup modified prefabs
   /// </summary>
   public class AutoSaveAssetProcessor : UnityEditor.AssetModificationProcessor
   {
      static string[] OnWillSaveAssets(string[] paths)
      {
         // Defer prefab backup until after the save completes.
         // Calling AssetDatabase.CopyAsset during OnWillSaveAssets causes errors
         // because the asset may not exist yet (new prefabs) or the AssetDatabase
         // is mid-operation (reentrancy).
         var pathsCopy = (string[])paths.Clone();
         EditorApplication.delayCall += () => AutoSave.BackupModifiedPrefabs(pathsCopy);
         return paths;
      }
   }
}
#endif
