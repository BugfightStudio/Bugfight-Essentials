# Changelog

## [0.2.0] - 2026-11-09

### Added

- Automatically initialized, prefab-free IMGUI command console with command discovery, overloads, autocomplete, parameter hints, log capture/collapse, configurable Input System shortcuts, and PlayMode regression tests.
- ScriptableHarmony scriptable variables and lists, runtime sets and single references, custom type generation, reference drawers, asset creation/search tools, and logging settings.
- Inspector attributes and drawers for method buttons, conditional visibility, read-only fields, play-mode read-only fields, and non-serialized values.
- Editor tools for centering objects to children, batch renaming, scene loading, gizmo drawing, and baking tags, layers, and scene names into C# code.
- Runtime helpers for persistent coroutine execution, timers, randomized timers, scriptable state machines, serialized interfaces, enum dictionaries, float ranges, and serialized waits.
- 3D tools for particle spawning, collision events, velocity calculation, ragdolls, and copying position/rotation.
- General, random, and vector utilities plus extensions for animators, bounds, cameras, collisions, colors, enumerables, joints, layers, MonoBehaviours, numbers, quaternions, and vectors.
- Editor-load initialization that disables the Render Pipeline DebugManager runtime UI.
- Explicit Input System and Render Pipeline Core package dependencies, with a Render Pipeline Core reference in the Editor-only assembly.

### Changed

- Migrated imported utilities and editor tooling to Bugfight namespaces and reorganized runtime attributes and extensions into dedicated folders.
- Documented console setup, controls, integration, migration, and validation, including the requirement for Active Input Handling to be set to Both.

## [0.1.0] - 2026-11-08

### Added

- Initial Unity 6.0 UPM package structure.
- Runtime and Editor-only assembly definitions.
- Package documentation and proprietary contributor-use license.
