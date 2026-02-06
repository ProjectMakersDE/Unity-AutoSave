# PM_AutoSave

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity 2022.3+](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg)](https://unity.com/)
[![Unity 6](https://img.shields.io/badge/Unity%206-6000.x-blue.svg)](https://unity.com/)

A handy tool to automatically save and backup your Unity scenes.

## Installation

### Via Package Manager (Recommended)

1. Open Unity and go to `Window > Package Manager`
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL...`
4. Enter: `https://github.com/ProjectMakers/Unity-AutoSave.git`
5. Click `Add`

### Manual Installation

Clone or download this repository into your project's `Packages/` folder:

```
YourProject/
├── Assets/
├── Packages/
│   └── Unity-AutoSave/    <-- Place here
└── ProjectSettings/
```

## Compatibility

- **Unity 2022.3 LTS** and newer
- **Unity 6 (6000.x)** - Fully compatible

## Usage

Open the AutoSave window via menu: **Tools > ProjectMakers > AutoSave**

## Features

| Feature | Description |
|---------|-------------|
| **Debug.Log** | Toggle console logging for all AutoSave actions |
| **Save on Play** | Automatically saves before entering Play mode |
| **Save Assets** | Also saves modified assets (ScriptableObjects, prefabs, etc.) |
| **Save Interval** | Configure auto-save timing from 1-30 minutes |
| **On/Off** | Enable or disable the AutoSave function |
| **Backup** | Create timestamped backup copies of scenes |
| **Backup Path** | Customize where backups are stored (relative to Assets/) |
| **Backup Count** | Limit the number of backup files per scene |

## Backup Organization

Backups are organized by machine identifier and scene name:
```
Assets/<backup-path>/<machine-id>/<scene-name>/
    SceneName v.2024_01_15_14_30_00_123.unity
    SceneName v.2024_01_15_14_35_00_456.unity
```

**Privacy Note:** Backups use a persistent machine identifier (GUID) instead of your device hostname. This prevents accidental disclosure of your machine name if backup folders are committed to version control or shared with others.

## Troubleshooting

### Scenes Not Being Saved

**Problem:** Changes to your scene are not being saved automatically.

**Solutions:**
- Verify that the **On/Off** toggle is enabled in the AutoSave window
- Check that your **Save Interval** is set to a reasonable time (1-30 minutes)
- Ensure you have an active scene open (AutoSave only works with saved scenes)
- Check the Console for any error messages if **Debug.Log** is enabled

### Backup Path Validation Errors

**Problem:** Getting errors about invalid backup paths.

**Solutions:**
- The backup path must be relative to the `Assets/` folder
- Do not include `Assets/` in the path (use `Backups` not `Assets/Backups`)
- Avoid special characters in the path name
- Ensure the path doesn't start with `/` or contain `..`
- Valid example: `AutoSaveBackups` or `Backups/Scenes`

### Permission Errors

**Problem:** Receiving permission denied or access errors.

**Solutions:**
- Ensure Unity has write permissions to your project folder
- Check that backup folder is not marked as read-only
- On Windows: Run Unity as Administrator if working in protected directories
- On macOS/Linux: Verify folder permissions with `chmod` if needed
- Close any programs that might be locking scene files (external editors, source control tools)

### Understanding When Saves Trigger

AutoSave triggers in the following situations:

1. **Interval-based:** Automatically after the configured time (1-30 minutes) has elapsed
2. **Save on Play:** When enabled, saves occur before entering Play mode
3. **Manual operations:** Regular Unity save operations (Ctrl/Cmd+S) still work independently

**Note:** AutoSave only operates when:
- The AutoSave function is enabled (On/Off toggle)
- A scene is currently open and has been saved at least once

If you continue to experience issues, please enable **Debug.Log** in the AutoSave window to see detailed information about save operations in the Console.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

## Deutsche Beschreibung

Ein praktisches Tool zum automatischen Speichern und Sichern Ihrer Unity-Szenen.

## Installation

### Via Package Manager (Empfohlen)

1. Öffnen Sie Unity und gehen Sie zu `Window > Package Manager`
2. Klicken Sie auf die `+` Schaltfläche in der oberen linken Ecke
3. Wählen Sie `Add package from git URL...`
4. Geben Sie ein: `https://github.com/ProjectMakers/Unity-AutoSave.git`
5. Klicken Sie auf `Add`

### Manuelle Installation

Klonen oder laden Sie dieses Repository in den `Packages/` Ordner Ihres Projekts:

```
IhrProjekt/
├── Assets/
├── Packages/
│   └── Unity-AutoSave/    <-- Hier platzieren
└── ProjectSettings/
```

## Kompatibilität

- **Unity 2022.3 LTS** und neuer
- **Unity 6 (6000.x)** - Vollständig kompatibel

## Verwendung

Öffnen Sie das AutoSave-Fenster über das Menü: **Tools > ProjectMakers > AutoSave**

## Funktionen

| Feature | Beschreibung |
|---------|-------------|
| **Debug.Log** | Aktiviert/deaktiviert die Konsolenausgabe für alle AutoSave-Aktionen |
| **Save on Play** | Speichert automatisch vor dem Starten des Play-Modus |
| **Save Assets** | Speichert auch modifizierte Assets (ScriptableObjects, Prefabs, etc.) |
| **Save Interval** | Konfiguriert das automatische Speichern von 1-30 Minuten |
| **On/Off** | Aktiviert oder deaktiviert die AutoSave-Funktion |
| **Backup** | Erstellt zeitgestempelte Backup-Kopien von Szenen |
| **Backup Path** | Legt fest, wo Backups gespeichert werden (relativ zu Assets/) |
| **Backup Count** | Begrenzt die Anzahl der Backup-Dateien pro Szene |

## Backup-Organisation

Backups werden nach Hostname und Szenenname organisiert:
```
Assets/<backup-path>/<hostname>/<scene-name>/
    SceneName v.2024_01_15_14_30_00_123.unity
    SceneName v.2024_01_15_14_35_00_456.unity
```

## Changelog

Siehe [CHANGELOG.md](CHANGELOG.md) für die Versionshistorie.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**ProjectMakers**
- Website: [projectmakers.de](https://projectmakers.de)
- Email: info@projectmakers.de
