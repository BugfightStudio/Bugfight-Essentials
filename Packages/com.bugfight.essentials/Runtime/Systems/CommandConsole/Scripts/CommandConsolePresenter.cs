using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Bugfight.Internal;
using UnityEngine;

namespace Bugfight.Systems.CommandConsole
{
    /// <summary>Command discovery, execution and completion; no dependencies on UI controls.</summary>
    [RequireComponent(typeof(CommandConsoleModel))]
    public class CommandConsolePresenter : MonoBehaviour
    {
        // Keep the original event name for existing subscribers.
        public event Action<object, LogType> OnCommandLogRecieved = delegate { };

        [SerializeField] CommandConsoleModel model;
        static int playSession;
        int initializedSession = -1;
        bool dispatchingLog;

        public string InputText { get; private set; } = string.Empty;
        public bool IsConsoleEnabled => model.IsConsoleEnabled;
        public bool CollapseMessages => model.CollapseMessages;
        public IReadOnlyList<ConsoleMessage> Logs => model.Logs;

        void Awake() => Initialize();
        void OnEnable()
        {
            Initialize();
            Application.logMessageReceived -= UnityLogReceived;
            Application.logMessageReceived += UnityLogReceived;
        }
        void OnDisable() => Application.logMessageReceived -= UnityLogReceived;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void BeginPlaySession() => playSession++;

        public void Initialize()
        {
            if (initializedSession == playSession) return;
            if (model == null) model = GetComponent<CommandConsoleModel>();
            initializedSession = playSession;
            model.ClearMessages();
            model.IsConsoleEnabled = false;
            InputText = string.Empty;
            model.GetSavedCollapseMessagesValue();
            RegisterCommands();
            SetInput(string.Empty);
            if (isActiveAndEnabled)
            {
                Application.logMessageReceived -= UnityLogReceived;
                Application.logMessageReceived += UnityLogReceived;
            }
        }

        void UnityLogReceived(string message, string stackTrace, LogType logType) => Log(message, logType);

        void Log(object message, LogType logType)
        {
            model.AddMessage(message, logType);
            // An observer may itself call Debug.Log; record it without recursively notifying observers.
            if (dispatchingLog) return;
            dispatchingLog = true;
            try { OnCommandLogRecieved.Invoke(message, logType); }
            finally { dispatchingLog = false; }
        }

        public void RegisterCommands()
        {
            model.RegisteredCommands.Clear();
            model.RegisteredCommandAttributes.Clear();

            // Respect the existing assembly configuration when present, but require no asset setup.
            AssemblyContainerSO configuration = Resources.Load<AssemblyContainerSO>("SH_AssemblyContainer");
            Assembly ownAssembly = typeof(ConsoleCommandAttribute).Assembly;
            foreach (Assembly assembly in GetCommandAssemblies(ownAssembly))
            {
                string name = assembly.GetName().Name;
                if (configuration != null && assembly != ownAssembly)
                {
                    if (configuration.IgnoredAssemblies.Contains(name)) continue;
                    if (configuration.RegisteredAssemblies.Count > 0 && !configuration.RegisteredAssemblies.Contains(name)) continue;
                }

                foreach (Type type in GetLoadableTypes(assembly))
                {
                    const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public |
                                               BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                    foreach (MethodInfo method in type.GetMethods(flags))
                    {
                        ConsoleCommandAttribute attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
                        if (attribute == null || string.IsNullOrWhiteSpace(attribute.command)) continue;
                        if (!method.IsStatic && !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                        CommandKey key = new(attribute.command, method.GetParameters());
                        if (model.RegisteredCommands.TryAdd(key, method))
                            model.RegisteredCommandAttributes.Add(key, attribute);
                        else
                            Log($"Command already declared for [{attribute.command}] in [{type}]", LogType.Warning);
                    }
                }
            }

            model.RegisteredCommands = model.RegisteredCommands
                .OrderBy(entry => GetCommandHeader(entry.Key), StringComparer.Ordinal)
                .ThenBy(entry => entry.Key.name, StringComparer.Ordinal)
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            SetInput(InputText);
        }

        static IEnumerable<Assembly> GetCommandAssemblies(Assembly ownAssembly)
        {
            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic).ToArray();
            HashSet<string> candidates = new() { ownAssembly.GetName().Name };
            bool added;
            do
            {
                added = false;
                foreach (Assembly assembly in loaded)
                {
                    string name = assembly.GetName().Name;
                    if (candidates.Contains(name)) continue;
                    if (assembly.GetReferencedAssemblies().Any(reference => candidates.Contains(reference.Name)))
                        added |= candidates.Add(name);
                }
            } while (added);
            // Assemblies unable to reference ConsoleCommandAttribute cannot declare commands.
            return loaded.Where(assembly => candidates.Contains(assembly.GetName().Name));
        }

        static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type != null); }
            catch (NotSupportedException) { return Array.Empty<Type>(); }
        }

        public void ToggleConsole()
        {
            if (IsConsoleEnabled) DisableConsole();
            else EnableConsole();
        }

        public void EnableConsole()
        {
            if (IsConsoleEnabled) return;
            model.IsConsoleEnabled = true;
            ResetAutocompleteSelection();
            SetInput(InputText);
            CommandConsoleEvents.InvokeOpen();
        }

        public void DisableConsole()
        {
            if (!IsConsoleEnabled) return;
            model.IsConsoleEnabled = false;
            InputText = string.Empty;
            ResetAutocompleteSelection();
            CommandConsoleEvents.InvokeClose();
        }

        void ResetAutocompleteSelection()
        {
            model.SelectedCommand = CommandKey.empty;
            model.SelectedAutocompleteIndex = -1;
            model.AutocompleteOptions.Clear();
        }

        public void SetInput(string text)
        {
            InputText = text ?? string.Empty;
            CommandKey previousSelection = model.SelectedCommand;
            ResetAutocompleteSelection();
            foreach (CommandKey key in model.RegisteredCommands.Keys)
            {
                if (DoesCommandMatchInput(key, InputText)) model.AutocompleteOptions.Add(key);
            }

            if (model.AutocompleteOptions.Count == 0) return;
            int previousIndex = model.AutocompleteOptions.FindIndex(key => key.Equals(previousSelection));
            SelectAutocompleteOption(previousIndex >= 0 ? previousIndex : 0);
        }

        static bool DoesCommandMatchInput(CommandKey key, string input)
        {
            if (input.Length == 0) return true;
            string[] parts = input.Split(new[] { ' ' }, 2);
            if (parts[0].Length == 0) return false;
            return key.name.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase)
                   && (parts.Length == 1 || string.Equals(key.name, parts[0], StringComparison.OrdinalIgnoreCase));
        }

        void SelectAutocompleteOption(int index)
        {
            model.SelectedAutocompleteIndex = index;
            model.SelectedCommand = model.AutocompleteOptions[index];
        }

        public void CycleAutocompleteSelection(int direction)
        {
            if (!IsConsoleEnabled || model.AutocompleteOptions.Count == 0) return;
            int count = model.AutocompleteOptions.Count;
            SelectAutocompleteOption((model.SelectedAutocompleteIndex + direction % count + count) % count);
        }

        public bool FillAutoCompletedText()
        {
            if (!IsConsoleEnabled || InputText.Contains(" ") || string.IsNullOrEmpty(model.SelectedCommand.name)) return false;
            MethodInfo method = model.RegisteredCommands[model.SelectedCommand];
            string completedText = model.SelectedCommand.name + (method.GetParameters().Any(parameter => !parameter.IsOptional) ? " " : string.Empty);
            if (InputText == completedText) return false;
            SetInput(completedText);
            return true;
        }

        /// <returns>True when submission ran, false when Enter only filled a completion.</returns>
        public bool SubmitCommand()
        {
            if (FillAutoCompletedText()) return false;
            string command = InputText;
            InvokeCommand(command);
            SetInput(string.Empty);
            return true;
        }

        public void InvokeCommand(string fullCommand)
        {
            if (string.IsNullOrWhiteSpace(fullCommand)) return;
            if (InputText != fullCommand) SetInput(fullCommand);
            string[] parts = fullCommand.Split(new[] { ' ' }, 2);
            if (!model.RegisteredCommands.TryGetValue(model.SelectedCommand, out MethodInfo method) ||
                !string.Equals(model.SelectedCommand.name, parts[0], StringComparison.OrdinalIgnoreCase))
            {
                Log("Command not found!", LogType.Warning);
                return;
            }

            string[] arguments = parts.Length > 1
                ? parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();
            ParameterInfo[] parameterInfos = method.GetParameters();
            int minimumCount = parameterInfos.Count(parameter => !parameter.IsOptional);
            if (arguments.Length < minimumCount) { Log("Not enough parameters!", LogType.Warning); return; }
            if (arguments.Length > parameterInfos.Length) { Log("Too many parameters!", LogType.Warning); return; }

            object[] parameters = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                parameters[i] = i < arguments.Length
                    ? GetParsedArg(parameterInfos[i].ParameterType, arguments[i])
                    : parameterInfos[i].DefaultValue;
                if (i >= arguments.Length || parameters[i] != null) continue;
                Log($"Invalid Parameter: {parameterInfos[i].Name}", LogType.Warning);
                return;
            }

            try
            {
                if (method.IsStatic) InvokeMethod(method, null, parameters);
                else
                {
                    UnityEngine.Object[] instances = FindObjectsByType(method.DeclaringType, FindObjectsSortMode.None);
                    if (instances.Length == 0) Log("No instances found to run the command", LogType.Warning);
                    foreach (MonoBehaviour instance in instances) InvokeMethod(method, instance, parameters);
                }
            }
            catch (Exception exception)
            {
                Log(exception is TargetInvocationException { InnerException: not null } invocation
                    ? invocation.InnerException.Message : exception.Message, LogType.Error);
            }
        }

        void InvokeMethod(MethodInfo method, MonoBehaviour instance, object[] parameters)
        {
            object result = method.Invoke(instance, parameters);
            if (result is IEnumerator routine) (instance != null ? instance : this).StartCoroutine(routine);
            else if (result != null) Log(result, LogType.Log);
        }

        static object GetParsedArg(Type type, string argument)
        {
            if (type == typeof(string)) return argument;
            if (type.IsEnum) return Enum.TryParse(type, argument, true, out object value) ? value : null;

            string[] values = argument.Split(',');
            if (type == typeof(Vector2) && values.Length == 2 &&
                float.TryParse(values[0], out float x2) && float.TryParse(values[1], out float y2)) return new Vector2(x2, y2);
            if (type == typeof(Vector3) && values.Length == 3 &&
                float.TryParse(values[0], out float x3) && float.TryParse(values[1], out float y3) &&
                float.TryParse(values[2], out float z3)) return new Vector3(x3, y3, z3);
            if (type == typeof(Vector2Int) && values.Length == 2 &&
                int.TryParse(values[0], out int ix2) && int.TryParse(values[1], out int iy2)) return new Vector2Int(ix2, iy2);
            if (type == typeof(Vector3Int) && values.Length == 3 &&
                int.TryParse(values[0], out int ix3) && int.TryParse(values[1], out int iy3) &&
                int.TryParse(values[2], out int iz3)) return new Vector3Int(ix3, iy3, iz3);

            if (!type.IsPrimitive || type == typeof(IntPtr) || type == typeof(UIntPtr)) return null;
            try { return Convert.ChangeType(argument, type, CultureInfo.CurrentCulture); }
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }
            catch (InvalidCastException) { return null; }
        }

        /// <summary>Deletes the selection or the preceding whitespace and word, preserving the rest of the line.</summary>
        public static string DeleteTextBlock(string text, ref int cursor, ref int selection)
        {
            cursor = Mathf.Clamp(cursor, 0, text.Length);
            selection = Mathf.Clamp(selection, 0, text.Length);
            int start = Math.Min(cursor, selection);
            int end = Math.Max(cursor, selection);
            if (start == end)
            {
                while (start > 0 && char.IsWhiteSpace(text[start - 1])) start--;
                while (start > 0 && !char.IsWhiteSpace(text[start - 1])) start--;
            }
            cursor = selection = start;
            return text.Remove(start, end - start);
        }

        public void ToggleMessageCollapsing(bool value)
        {
            model.CollapseMessages = value;
            model.SetSavedCollapseMessagesValue();
        }

        public void ClearMessages() => model.ClearMessages();

        public string BuildPlaceholderString()
        {
            if (!model.RegisteredCommands.TryGetValue(model.SelectedCommand, out MethodInfo method)) return string.Empty;
            ParameterInfo[] parameters = method.GetParameters();
            string[] parts = InputText.Split(new[] { ' ' }, 2);
            string arguments = parts.Length > 1 ? parts[1] : string.Empty;
            int enteredCount = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            string prefix = model.SelectedCommand.name;
            if (!string.IsNullOrWhiteSpace(arguments)) prefix += " " + arguments.TrimEnd();
            string remaining = string.Join(" ", parameters.Skip(enteredCount).Select(parameter => parameter.Name));
            return prefix + (remaining.Length > 0 ? $" <color=#CC7744>{remaining}</color>" : string.Empty);
        }

        public string BuildAutocompleteOptionsString(int maximumOptions = 6)
        {
            if (model.AutocompleteOptions.Count == 0) return string.Empty;
            int visible = Math.Min(model.AutocompleteOptions.Count, Math.Max(1, maximumOptions));
            int start = Mathf.Clamp(model.SelectedAutocompleteIndex - visible + 1, 0, model.AutocompleteOptions.Count - visible);
            List<string> lines = new();
            string currentHeader = null;
            for (int i = start; i < start + visible; i++)
            {
                CommandKey key = model.AutocompleteOptions[i];
                string header = GetCommandHeader(key);
                if (!string.IsNullOrEmpty(header) && header != currentHeader)
                    lines.Add($"<color=#{GetSeededHeaderColor(header)}>{header}</color>");
                currentHeader = header;
                string signature = key.name + "(" + string.Join(", ", model.RegisteredCommands[key].GetParameters()
                    .Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}")) + ")";
                lines.Add(i == model.SelectedAutocompleteIndex ? $"<color=#CC7744>> {signature}</color>" : $"  {signature}");
            }
            return string.Join("\n", lines);
        }

        string GetCommandHeader(CommandKey key) => model.RegisteredCommandAttributes.TryGetValue(key, out ConsoleCommandAttribute attribute)
            ? attribute.commandHeader ?? string.Empty : string.Empty;

        static string GetSeededHeaderColor(string seed)
        {
            unchecked
            {
                int hash = 17;
                foreach (char character in seed) hash = hash * 31 + character;
                return ColorUtility.ToHtmlStringRGB(Color.HSVToRGB(Mathf.Abs(hash % 360) / 360f, 0.55f, 1f));
            }
        }
    }
}
