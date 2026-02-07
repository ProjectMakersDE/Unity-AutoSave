# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.7.1] - 2026-02-07

### Changed
- Backup settings UI: Scene and Prefab backup toggles now show distinct labels
- Backup path input field auto-sizes to full window width

### Fixed
- Two identical-looking backup enable buttons now clearly labeled "Scene" and "Prefab"
- Added missing CONTRIBUTING.md.meta file to resolve Unity import error

## [1.7.0] - 2026-02-05

### Added
- Visual save status indicator in editor window
- Path validation to prevent backup path traversal attacks
- OnDisable cleanup to prevent memory leaks
- Null checks for cached textures
- Improved tooltips with detailed descriptions

### Changed
- Textures now cached on window enable (major performance improvement)
- Texture path calculated once and cached
- GUIStyle initialized once instead of every frame
- Backup cleanup uses O(n) algorithm instead of O(n log n) sort
- AssetDatabase.ImportAsset used instead of full Refresh
- Backup cleanup runs via delayCall to prevent blocking
- GUI colors properly saved and restored
- BackupCount uses field synchronization instead of direct EditorPrefs
- Improved error handling with success tracking
- Assembly definition now properly specifies Editor platform
- README reorganized with English first, installation instructions added
- package.json updated with better description and keywords

### Fixed
- Event handler memory leak (missing OnDisable)
- Duplicate event registration when reopening window
- Race condition when save triggers from multiple sources
- BackupCount field desynchronization with EditorPrefs
- Unnecessary Debug.Log on window open removed

## [1.6.0] - 2023-07-04

### Added
Log colors

### Changed
Clean code rewrite

### Fixed
BackupPath does not save correctly

## [1.5.1] - 2022-08-25

### Added
Support saving multible Scenes in Editor

## [1.5.0] - 2022-05-05

### Added
...

### Changed
Backup Path creation
Performance improved

### Fixed
A few small bugs

### Deprecated
...

### Removed
...

### Known Bugs
...

## [1.0.2] - 2021-10-02

### Added
...

### Changed
...

### Fixed
Backup-Path error

### Deprecated
...

### Removed
...

### Known Bugs
...

1.0.0] - 2020-09-09

### Added
- ...

### Changed
- ...

### Fixed
- ...