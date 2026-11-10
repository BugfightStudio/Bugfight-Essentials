# Bugfight Essentials

A Unity Package Manager (UPM) package maintained by **BUGFIGHT STUDIO LTD.**

## Requirements

- Unity **6.0 (6000.0)** or newer.
- Unity Input System **1.11.2** or newer (installed automatically as a package dependency).
- Unity Render Pipeline Core **17.0.3** or newer (installed automatically as a package dependency).
- **Active Input Handling: Both** for the IMGUI console (requires an Editor restart after changing it).
- Git installed and available to Unity.
- Permission to use the package under [LICENSE.md](LICENSE.md).

## Install from Git

In Unity, open **Window > Package Manager**, select **+ > Install package from Git URL**, and enter:

```text
https://github.com/NuiN99/Bugfight-Essentials.git?path=/Packages/com.bugfight.essentials
```

Alternatively, add this entry to your project's `Packages/manifest.json` dependencies:

```json
"com.bugfight.essentials": "https://github.com/NuiN99/Bugfight-Essentials.git?path=/Packages/com.bugfight.essentials"
```

For reproducible installations, append `#<commit-sha>` or an existing release tag after the path. Package version `0.2.0` does not itself create a Git tag. Private repository access requires Git credentials configured on the installing machine; repository access does not itself grant a license.

## Repository layout

```text
Packages/com.bugfight.essentials/
  package.json
  Runtime/    # Bugfight.Essentials assembly
  Editor/     # Bugfight.Essentials.Editor assembly (Editor only)
```

The runtime includes an automatically initialized, prefab-free IMGUI command console and PlayMode regression tests. See the [console guide](Packages/com.bugfight.essentials/Runtime/Systems/CommandConsole/README.md). This repository is not a Unity project; import the package into a separate Unity project to develop or validate it.

See the [package README](Packages/com.bugfight.essentials/README.md) for development instructions and the [changelog](Packages/com.bugfight.essentials/CHANGELOG.md) for release notes.

## Licensing and contributions

This is **proprietary software**, not an open-source package. Anyone with an accepted contribution may use and modify it in personal or commercial projects and distribute compiled projects, but may not redistribute the package or its source without separate written permission. Other users need written permission from BUGFIGHT STUDIO LTD. See [LICENSE.md](LICENSE.md) for the complete terms.

Contribution acceptance does not transfer copyright. Any assignment of contributor rights to BUGFIGHT STUDIO LTD. must be handled separately. The custom license is a draft and should receive Canadian legal review before being relied upon.
