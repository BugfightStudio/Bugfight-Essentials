# Command console

The console creates one persistent GameObject automatically after the first scene loads. It uses `OnGUI` and needs no scene objects, Canvas, EventSystem, TextMesh Pro, prefabs, or icon assets. The package declares its Input System dependency.

**Set Player Settings > Active Input Handling to Both and restart the Editor.** The Input System handles the configurable shortcuts; Unity's legacy input backend supplies native IMGUI text editing, mouse controls, and scrolling. Unity does not send runtime IMGUI input events when only the Input System package is enabled. This project's setting has been changed to Both.

## Controls

- **Backquote (`)**: open/close; **Escape** or **Close**: close and clear the input.
- **Tab / Right Arrow**: complete the selected command.
- **Up / Down**: select a suggestion/overload; hold to repeat, including while time is paused.
- **Enter / Keypad Enter**: complete an unfinished command first, otherwise execute it.
- **Ctrl + Backspace**: delete the selection or preceding word at the caret.
- **Clear**: remove all logs. **Collapse**: group identical messages by text and severity, with counts. This preference is saved in PlayerPrefs. Turning collapse off restores individual entries.

The input supports IMGUI's normal caret movement, selection, clipboard operations, and text editing. Parameter hints and categorized, colored suggestions appear above it. Logs distinguish all Unity log types and follow the bottom until you scroll away; submitting a command scrolls to the bottom again.

## Commands and integration

Continue using `[ConsoleCommand("name", header: "Category")]` on static methods or MonoBehaviour instance methods. Overloads are selected with Up/Down. Commands support optional parameters, enums (case-insensitive), primitive values, and comma-separated Vector2/3 and Vector2Int/3Int arguments. String arguments remain space-delimited, as before. Non-void results are logged; IEnumerator results run as coroutines. Instance commands run on all active instances in loaded scenes.

Existing `CommandConsoleEvents.OnOpen` / `OnClose` subscribers and `OnCommandLogRecieved` subscribers are supported. Unity logs, including the default `log`, `warn`, and `error` commands, are captured even while the console is closed.

`CommandConsoleView.Instance` exposes the automatically created view, and `.Presenter` exposes UI-independent methods such as `SetInput`, `SubmitCommand`, `InvokeCommand(string)`, `EnableConsole`, `DisableConsole`, and `ClearMessages`. UI-control-based presenter methods have been replaced; callers no longer pass TMP or uGUI objects.

If an existing `Resources/SH_AssemblyContainer` configures assembly discovery, its registered/ignored lists are respected, and built-in commands are always included. Without it, loaded assemblies are scanned. Commands invoked only through reflection must be preserved from managed stripping in IL2CPP builds (for example with `[UnityEngine.Scripting.Preserve]` or `link.xml`).

Appearance, input actions, and repeat timings are serialized on `CommandConsoleView`. By default the console is anchored to the bottom and occupies 40% of the screen height, with a 24-pixel Collapse checkbox. An optional root GameObject with this component in the first scene can override the defaults; automatic startup reuses it. Action references are supported; actions already enabled by another system are not disabled or disposed by the console.

The obsolete console/message prefabs, icon ScriptableObject, and legacy icon textures have been removed. Remove old prefab instances when migrating existing scenes. No scenes in this project referenced those prefabs or legacy icon assets.

## Validation

Run `Bugfight.Essentials.CommandConsole.Tests` in Unity Test Runner's **PlayMode** tab. For an installed package, add `com.bugfight.essentials` to the project manifest's `testables` list.

Also check these interactive behaviors in the Game view and a standalone build:

1. Open, type, paste, select, move the caret, and Ctrl+Backspace in the middle of a line.
2. Check Tab/Right Arrow, repeated Up/Down, overload selection, hints, and Enter's completion-before-execution behavior.
3. Scroll a long, multiline log; clear it; toggle collapse on/off; restart to check the saved preference.
4. Close using Escape and the button; reopen to confirm focus and empty input. Check the toggle key never becomes input text.
5. Reload/load a scene and confirm only one console remains. Test paused time, custom input actions, different resolutions, and Enter Play Mode with domain/scene reload disabled.
