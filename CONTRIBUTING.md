# Contributing to PM_AutoSave

Thank you for your interest in contributing to PM_AutoSave! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Contributing Code](#contributing-code)
- [Development Setup](#development-setup)
- [Code Style Guidelines](#code-style-guidelines)
- [Quality Standards](#quality-standards)
- [Pull Request Process](#pull-request-process)
- [Commit Message Guidelines](#commit-message-guidelines)

## Code of Conduct

This project and everyone participating in it is expected to uphold professional and respectful behavior. We are committed to providing a welcoming and inspiring community for all.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/Unity-AutoSave.git
   cd Unity-AutoSave
   ```
3. **Create a branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## How Can I Contribute?

### Reporting Bugs

Before creating a bug report, please check existing issues to avoid duplicates.

**When submitting a bug report, include:**

- **Clear title** - A concise description of the issue
- **Unity version** - Which Unity version you're using (e.g., 2022.3.15f1, Unity 6.0.1)
- **Steps to reproduce** - Detailed steps to reproduce the behavior
- **Expected behavior** - What you expected to happen
- **Actual behavior** - What actually happened
- **Screenshots** - If applicable, add screenshots to help explain the problem
- **Console output** - Any relevant error messages or warnings
- **Additional context** - Any other relevant information

**Example:**
```
Title: AutoSave fails when backup path contains special characters

Unity Version: 2022.3.10f1

Steps to Reproduce:
1. Open AutoSave window (Tools > ProjectMakers > AutoSave)
2. Set backup path to "Backups/Test#Folder"
3. Enable AutoSave and Backup
4. Wait for auto-save interval

Expected: Backup is created successfully
Actual: Error in console: "Invalid backup path"
```

### Suggesting Features

We welcome feature suggestions! Before submitting:

1. **Check existing issues** - Your feature might already be proposed
2. **Verify scope** - Ensure the feature aligns with AutoSave's purpose
3. **Consider compatibility** - How does it affect Unity 2022.3+ and Unity 6?

**When submitting a feature request, include:**

- **Clear title** - A concise description of the feature
- **Problem statement** - What problem does this solve?
- **Proposed solution** - How would this feature work?
- **Alternatives considered** - What other solutions did you consider?
- **Unity compatibility** - Any version-specific considerations
- **Implementation notes** - Technical details if applicable

### Contributing Code

We love code contributions! Here are the types of contributions we're looking for:

- **Bug fixes** - Fixes for reported issues
- **Performance improvements** - Optimization of existing features
- **New features** - After discussion in an issue
- **Documentation** - Improvements to README, comments, or examples
- **Tests** - Additional test coverage
- **Compatibility** - Support for new Unity versions

## Development Setup

### Prerequisites

- **Unity 2022.3 LTS** or newer (including Unity 6)
- **Git** for version control
- **Text editor or IDE** with C# support (Visual Studio, Rider, VS Code)

### Setting Up Your Development Environment

1. **Install Unity** (if not already installed)
   - Download from [Unity Hub](https://unity.com/download)
   - Install Unity 2022.3 LTS or newer

2. **Create a test Unity project**
   ```bash
   # Create a new Unity project via Unity Hub or:
   unity -createProject ~/UnityProjects/AutoSaveTest
   ```

3. **Add PM_AutoSave to your test project**
   ```bash
   cd ~/UnityProjects/AutoSaveTest/Packages
   git clone https://github.com/YOUR-USERNAME/Unity-AutoSave.git
   ```

4. **Open your test project in Unity**
   - Unity will compile the package automatically
   - Access AutoSave via: **Tools > ProjectMakers > AutoSave**

### Testing Your Changes

1. **Manual testing:**
   - Create or open a Unity scene
   - Modify the scene to make it "dirty"
   - Test all AutoSave features:
     - Auto-save interval
     - Save on Play
     - Backup creation
     - Backup cleanup
     - Path validation
     - Debug logging

2. **Test across Unity versions:**
   - Test with Unity 2022.3 LTS
   - Test with Unity 6 (6000.x) if possible

3. **Test edge cases:**
   - Empty scenes
   - Multiple open scenes
   - Invalid backup paths
   - Maximum backup count
   - Rapid play mode entry/exit

## Code Style Guidelines

Follow these coding standards to maintain consistency with the existing codebase:

### Naming Conventions

```csharp
// Private fields: _camelCase with underscore prefix
private bool _autoSave;
private int _saveInterval;

// Constants: PascalCase
private const string LOGColor = "#CC0000";
private const string EditorPrefPrefix = "PM_AS_";

// Public/private methods: PascalCase
private void CacheTextures()
private static void Save()

// Properties: PascalCase
private string TexturePath { get; }
```

### Code Organization

```csharp
// 1. Using statements (organized, no unused)
using System;
using UnityEditor;
using UnityEngine;

// 2. Namespace
namespace PM.Tools
{
   // 3. Class definition
   public class AutoSave : EditorWindow
   {
      // 4. Constants
      private const string LOGColor = "#CC0000";

      // 5. Fields (grouped logically)
      private static bool _autoSave;

      // 6. Properties
      private string TexturePath { get; }

      // 7. Methods (public first, then private)
      private static void Save() { }
   }
}
```

### Comments

- **Use comments to explain "why", not "what"**
  ```csharp
  // Good: Explains intent
  // Always reset timer to prevent checking every frame
  _lastAutosave = DateTime.Now;

  // Bad: States the obvious
  // Set lastAutosave to current time
  _lastAutosave = DateTime.Now;
  ```

- **Document complex logic**
  ```csharp
  // O(n) algorithm to find oldest file instead of O(n log n) sort
  foreach (var file in fileInfo) { ... }
  ```

- **Add XML documentation for public APIs**
  ```csharp
  /// <summary>
  /// Saves all dirty scenes and creates backups if enabled.
  /// </summary>
  private static void Save() { }
  ```

### Unity-Specific Guidelines

- **Use EditorPrefs for persistent settings**
  ```csharp
  private const string AutoSaveKey = EditorPrefPrefix + "AUTOSAVE";
  _autoSave = EditorPrefs.GetBool(AutoSaveKey, true);
  ```

- **Cache Unity assets to avoid repeated lookups**
  ```csharp
  // Cache in OnEnable, use throughout lifecycle
  private void OnEnable()
  {
     CacheTextures();
  }
  ```

- **Clean up event handlers properly**
  ```csharp
  // Unsubscribe before subscribing to prevent duplicates
  EditorApplication.update -= EditorUpdate;
  EditorApplication.update += EditorUpdate;
  ```

## Quality Standards

All contributions must meet the quality standards established in version 1.7.0:

### Performance

- **Cache expensive operations**
  - Load textures once in `OnEnable`, not every frame
  - Calculate paths once and cache the result
  - Initialize GUIStyles once, not every `OnGUI` call

- **Use efficient algorithms**
  - Prefer O(n) over O(n log n) when possible
  - Avoid unnecessary sorting or repeated operations

- **Prevent blocking operations**
  - Use `EditorApplication.delayCall` for deferred cleanup
  - Use `AssetDatabase.ImportAsset` instead of full `Refresh`

### Memory Management

- **Prevent memory leaks**
  - Unsubscribe from event handlers in `OnDisable`
  - Clean up cached resources properly
  - Avoid duplicate event registrations

- **Manage resources efficiently**
  - Cache frequently-used assets
  - Dispose of temporary resources

### Error Handling

- **Use try-catch for risky operations**
  ```csharp
  try
  {
     EditorSceneManager.SaveScene(scene);
  }
  catch (Exception e)
  {
     Log(2, $"Error saving scene: {e}");
  }
  ```

- **Validate user input**
  ```csharp
  private static bool ValidateBackupPath(string path)
  {
     if (string.IsNullOrWhiteSpace(path))
        return false;

     // Prevent path traversal attacks
     if (path.Contains("..") || path.StartsWith("/"))
        return false;
  }
  ```

- **Add null checks for Unity objects**
  ```csharp
  if (_cachedTexture != null)
     GUILayout.Label(_cachedTexture);
  ```

### Thread Safety

- **Prevent race conditions**
  ```csharp
  // Use flags to prevent concurrent execution
  if (_isSaving) return;
  _isSaving = true;
  try
  {
     // ... perform operation
  }
  finally
  {
     _isSaving = false;
  }
  ```

### Security

- **Validate file paths**
  - Prevent path traversal attacks (e.g., `../../../etc/passwd`)
  - Reject absolute paths in user-configurable paths
  - Sanitize user input before file operations

## Pull Request Process

1. **Update your fork**
   ```bash
   git checkout main
   git pull upstream main
   git checkout feature/your-feature-name
   git rebase main
   ```

2. **Ensure code quality**
   - [ ] Code follows style guidelines
   - [ ] No compiler warnings or errors
   - [ ] No debug statements (`Debug.Log` for debugging only)
   - [ ] All edge cases handled
   - [ ] Memory leaks prevented
   - [ ] Performance optimized

3. **Test thoroughly**
   - [ ] Manual testing completed
   - [ ] Tested in Unity 2022.3 LTS
   - [ ] Tested in Unity 6 (if possible)
   - [ ] Edge cases verified
   - [ ] No regression of existing features

4. **Update documentation**
   - [ ] Update README.md if feature is user-facing
   - [ ] Update CHANGELOG.md following [Keep a Changelog](https://keepachangelog.com/) format
   - [ ] Add XML documentation for new public methods
   - [ ] Update tooltips for new UI elements

5. **Commit your changes**
   ```bash
   git add .
   git commit -m "Add feature: brief description"
   ```

6. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create Pull Request**
   - Go to the original repository on GitHub
   - Click "New Pull Request"
   - Select your fork and branch
   - Fill out the PR template

### Pull Request Template

When creating a PR, include:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update

## Testing
- [ ] Tested in Unity 2022.3 LTS
- [ ] Tested in Unity 6
- [ ] Manual testing performed
- [ ] Edge cases verified

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] CHANGELOG.md updated

## Screenshots (if applicable)
Add screenshots demonstrating the change
```

## Commit Message Guidelines

Follow these conventions for commit messages:

### Format

```
<type>: <subject>

<body>

<footer>
```

### Types

- **feat:** A new feature
- **fix:** A bug fix
- **perf:** Performance improvement
- **refactor:** Code change that neither fixes a bug nor adds a feature
- **docs:** Documentation only changes
- **style:** Code style changes (formatting, missing semicolons, etc.)
- **test:** Adding or updating tests
- **chore:** Changes to build process or auxiliary tools

### Examples

```bash
# Good commit messages
git commit -m "feat: add visual save status indicator to editor window"
git commit -m "fix: prevent race condition when save triggers from multiple sources"
git commit -m "perf: cache textures on window enable instead of every frame"
git commit -m "docs: update README with installation instructions"

# Bad commit messages (avoid these)
git commit -m "fixed bug"
git commit -m "updates"
git commit -m "WIP"
```

### Detailed Commit Message

For complex changes, add a body:

```
feat: add path validation for backup paths

Implements security checks to prevent path traversal attacks.
Validates that backup paths don't contain "..", absolute paths,
or other dangerous patterns.

Closes #123
```

## Questions or Need Help?

If you have questions or need help:

- **Open an issue** - For general questions or discussions
- **Email** - Contact info@projectmakers.de
- **Check existing issues** - Your question might already be answered

## License

By contributing to PM_AutoSave, you agree that your contributions will be licensed under the MIT License.

## Recognition

All contributors will be recognized in the project. Thank you for helping make PM_AutoSave better!

---

**ProjectMakers**
- Website: [projectmakers.de](https://projectmakers.de)
- Email: info@projectmakers.de
