# Bugfight Essentials

Runtime utilities and Unity Editor tooling from **BUGFIGHT STUDIO LTD.**

## Requirements

- Unity 6.0 (6000.0) or newer.
- Unity Input System 1.11.2 or newer (installed as a package dependency).
- Active Input Handling set to **Both** for runtime IMGUI input (restart the Editor after changing it).

## Development

The runtime includes a prefab-free, automatically initialized IMGUI command console.
See the [command console guide](Runtime/Systems/CommandConsole/README.md) for controls,
command registration, migration notes, and regression tests.

- Add runtime code to `Runtime/`, using the `Bugfight.Essentials` namespace.
- Add Unity Editor tooling to `Editor/`, using `Bugfight.Essentials.Editor`.
- The Editor assembly references Runtime and is excluded from player builds.
- Runtime code must not reference `UnityEditor` or the Editor assembly.
- Commit Unity-generated `.meta` files alongside every new asset and folder.

To edit the package locally, add it to a separate Unity project's
`Packages/manifest.json` using a local dependency such as:

```json
"com.bugfight.essentials": "file:C:/path/to/Bugfight-Essentials/Packages/com.bugfight.essentials"
```

Replace the example path with your checkout's absolute path, using forward
slashes. Git-installed packages are cached by Unity; use a local dependency when
editing this repository directly.

## License

Proprietary. Eligible contributors may use and modify the package in personal
and commercial projects and distribute compiled projects, but may not
redistribute the package or its source without separate written permission.
See [LICENSE.md](LICENSE.md) for the complete terms.
