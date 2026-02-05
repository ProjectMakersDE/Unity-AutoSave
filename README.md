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

Backups are organized by hostname and scene name:
```
Assets/<backup-path>/<hostname>/<scene-name>/
    SceneName v.2024_01_15_14_30_00_123.unity
    SceneName v.2024_01_15_14_35_00_456.unity
```

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

## Deutsche Beschreibung

### Beschreibung

Nichts ist wichtiger als regelmäßiges Speichern. Doch es passiert schnell, dass man vergisst, die Szene vor dem Testen zu speichern. Um dem zu entgehen gibt es jetzt den AutoSave Editor.

### Projektumfang

Das Asset besteht aus einem C#-Skript und 9 Texturen. Getestet mit Unity 2022.3 bis Unity 6 (6000.x).

### Funktionen

- **Debug.Log:** Zeigt alle Aktionen, die von diesem Asset ausgeführt werden, im Debug.Log an.
- **Save on play:** Speichert bei Betätigung des Playbuttons.
- **Save Assets:** Speichert auch modifizierte Assets.
- **Save interval:** Gibt an, nach wieviel Minuten (1 - 30) gespeichert wird.
- **On/Off:** Schaltet die Autosave-Funktion an bzw. aus.
- **Backup On/Off:** Schaltet die Backup-Funktion an bzw. aus.
- **Scene Folder:** Ein Ordner wird erstellt mit dem Hostname und dem Namen der Szene.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**ProjectMakers**
- Website: [projectmakers.de](https://projectmakers.de)
- Email: info@projectmakers.de
