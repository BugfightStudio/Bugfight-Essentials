using System;
using System.Collections;
using System.Linq;
using Bugfight.Systems.CommandConsole;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Bugfight.Tests.CommandConsole
{
    public class CommandConsoleTests
    {
        const string COLLAPSE_KEY = "COMMAND_CONSOLE_COLLAPSE_MESSAGES";
        GameObject root;
        CommandConsolePresenter presenter;
        CommandConsoleModel model;
        object returnedValue;
        bool hadSavedCollapse;
        int savedCollapse;

        [SetUp]
        public void SetUp()
        {
            hadSavedCollapse = PlayerPrefs.HasKey(COLLAPSE_KEY);
            savedCollapse = PlayerPrefs.GetInt(COLLAPSE_KEY);
            root = new GameObject("Console Regression Test");
            presenter = root.AddComponent<CommandConsolePresenter>();
            model = root.GetComponent<CommandConsoleModel>();
            presenter.Initialize();
            presenter.OnCommandLogRecieved += (value, _) => returnedValue = value;
            presenter.EnableConsole();
        }

        [TearDown]
        public void TearDown()
        {
            presenter.DisableConsole();
            Object.DestroyImmediate(root);
            if (hadSavedCollapse) PlayerPrefs.SetInt(COLLAPSE_KEY, savedCollapse);
            else PlayerPrefs.DeleteKey(COLLAPSE_KEY);
            PlayerPrefs.Save();
        }

        [Test]
        public void DiscoversDefaultAndAttributedCommandsWithoutConfigurationAssets()
        {
            Assert.That(model.RegisteredCommands.Keys.Any(key => key.name == "reload-scene"));
            Assert.That(model.RegisteredCommands.Keys.Count(key => key.name == "__regression-overload"), Is.EqualTo(2));
        }

        [Test]
        public void CommandKeyEqualityIncludesTheEntireSignature()
        {
            CommandKey[] keys = model.RegisteredCommands.Keys.Where(key => key.name == "__regression-overload").ToArray();
            Assert.That(keys[0].Equals(keys[1]), Is.False);
            Assert.That(model.RegisteredCommands[keys[0]], Is.Not.EqualTo(model.RegisteredCommands[keys[1]]));
            Assert.That(default(CommandKey).Equals(default(CommandKey)), Is.True);
            Assert.DoesNotThrow(() => default(CommandKey).GetHashCode());
        }

        [Test]
        public void EnterCompletesBeforeExecutingAndTabOnlyAddsSpaceForRequiredParameters()
        {
            presenter.SetInput("__regression-opt");
            Assert.That(presenter.SubmitCommand(), Is.False);
            Assert.That(presenter.InputText, Is.EqualTo("__regression-optional"));
            Assert.That(presenter.SubmitCommand(), Is.True);
            Assert.That(returnedValue, Is.EqualTo(7));
            Assert.That(presenter.InputText, Is.Empty);
            presenter.SetInput("__regression-overl");
            Assert.That(presenter.FillAutoCompletedText(), Is.True);
            Assert.That(presenter.InputText, Is.EqualTo("__regression-overload "));
        }

        [Test]
        public void CompletionWrapsAndPreservesTheSelectedOverloadWhileTypingArguments()
        {
            presenter.SetInput("__regression-overload");
            presenter.CycleAutocompleteSelection(-1);
            Assert.That(model.SelectedAutocompleteIndex, Is.EqualTo(1));
            CommandKey selected = model.SelectedCommand;
            presenter.SetInput("__regression-overload 3");
            Assert.That(model.SelectedCommand.Equals(selected), Is.True);
            presenter.CycleAutocompleteSelection(1);
            Assert.That(model.SelectedAutocompleteIndex, Is.Zero);
            presenter.SetInput("__regression-overload");
            Assert.That(presenter.BuildPlaceholderString(), Does.Contain("value"));
            Assert.That(presenter.BuildAutocompleteOptionsString(), Does.Contain("Regression"));
        }

        [TestCase(typeof(int), "42", 42)]
        [TestCase(typeof(float), "3", 3f)]
        public void InvokesTheSelectedOverload(Type parameterType, string argument, object expected)
        {
            presenter.SetInput("__regression-overload " + argument);
            while (model.SelectedCommand.parameterTypes[0] != parameterType) presenter.CycleAutocompleteSelection(1);
            presenter.InvokeCommand(presenter.InputText);
            Assert.That(returnedValue, Is.EqualTo(expected));
            Assert.That(returnedValue.GetType(), Is.EqualTo(parameterType));
        }

        [TestCase("__regression-optional", "7")]
        [TestCase("__regression-optional ", "7")]
        [TestCase("__regression-optional 12", "12")]
        [TestCase("__regression-null", "null default")]
        [TestCase("__regression-vector 1,2,3", "(1, 2, 3)")]
        [TestCase("__regression-enum friday", "Friday")]
        [TestCase("__regression-bool true", "True")]
        [TestCase("__regression-char x", "x")]
        [TestCase("__regression-long 9223372036854775807", "9223372036854775807")]
        public void SupportsOptionalDefaultsEnumsVectorsAndPrimitiveParameters(string command, string expected)
        {
            presenter.InvokeCommand(command);
            Assert.That(returnedValue.ToString(), Is.EqualTo(expected));
        }

        [TestCase("__not-a-command", "Command not found!")]
        [TestCase("__regression-overload", "Not enough parameters!")]
        [TestCase("__regression-optional 1 2", "Too many parameters!")]
        [TestCase("__regression-overload invalid", "Invalid Parameter")]
        [TestCase("__regression-throws", "regression failure")]
        public void ReportsInvalidCommandsAndInvocationExceptions(string command, string expected)
        {
            presenter.InvokeCommand(command);
            Assert.That(model.Logs.Last().Key.message, Does.Contain(expected));
        }

        [Test]
        public void EmptySubmissionDoesNotInvokeAnArbitrarySelectedCommand()
        {
            presenter.SetInput(" ");
            int count = model.Logs.Count;
            presenter.SubmitCommand();
            Assert.That(model.Logs.Count, Is.EqualTo(count));
        }

        [TestCase("one two three", 7, 7, "one  three", 4)]
        [TestCase("one two   ", 10, 10, "one ", 4)]
        [TestCase("one two", 0, 0, "one two", 0)]
        [TestCase("one two", 4, 7, "one ", 4)]
        public void WordDeletionPreservesTheSuffixAndHandlesSelections(string text, int cursor, int selection, string expected, int expectedCursor)
        {
            string result = CommandConsolePresenter.DeleteTextBlock(text, ref cursor, ref selection);
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(cursor, Is.EqualTo(expectedCursor));
            Assert.That(selection, Is.EqualTo(cursor));
        }

        [Test]
        public void CollapsingIsReversibleDistinguishesSeverityAndClearsEveryMessage()
        {
            model.ClearMessages();
            model.AddMessage("same", LogType.Log);
            model.AddMessage("same", LogType.Warning);
            model.AddMessage("same", LogType.Log);
            presenter.ToggleMessageCollapsing(true);
            Assert.That(model.Logs.Count, Is.EqualTo(2));
            Assert.That(model.Logs[0].Count, Is.EqualTo(2));
            Assert.That(PlayerPrefs.GetInt(COLLAPSE_KEY), Is.EqualTo(1));
            presenter.ToggleMessageCollapsing(false);
            Assert.That(model.Logs.Count, Is.EqualTo(3));
            presenter.ClearMessages();
            presenter.ToggleMessageCollapsing(true);
            Assert.That(model.Logs, Is.Empty);
            model.AddMessage(null, LogType.Exception);
            Assert.That(model.Logs[0].Key.message, Is.EqualTo("null"));
            Assert.That(model.Logs[0].Count, Is.EqualTo(1));
        }

        [Test]
        public void ForwardsUnityLogsIncludingTheDefaultLogCommand()
        {
            model.ClearMessages();
            LogAssert.Expect(LogType.Log, "console-captured");
            presenter.InvokeCommand("log console-captured");
            Assert.That(model.Logs.Any(log => log.Key.message == "console-captured"), Is.True);
        }

        [Test]
        public void InstanceCommandsRunOnEveryActiveInstance()
        {
            GameObject first = new("First command target");
            GameObject second = new("Second command target");
            try
            {
                CommandConsoleTestTarget a = first.AddComponent<CommandConsoleTestTarget>();
                CommandConsoleTestTarget b = second.AddComponent<CommandConsoleTestTarget>();
                presenter.InvokeCommand("__regression-instance 4");
                Assert.That(a.Value, Is.EqualTo(4));
                Assert.That(b.Value, Is.EqualTo(4));
                second.SetActive(false);
                presenter.InvokeCommand("__regression-instance 8");
                Assert.That(a.Value, Is.EqualTo(8));
                Assert.That(b.Value, Is.EqualTo(4));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void SupportsAllVectorTypes()
        {
            presenter.InvokeCommand("__regression-vector2 1,2");
            Assert.That(returnedValue, Is.EqualTo(new Vector2(1, 2)));
            presenter.InvokeCommand("__regression-vector3 1,2,3");
            Assert.That(returnedValue, Is.EqualTo(new Vector3(1, 2, 3)));
            presenter.InvokeCommand("__regression-vector2int 1,2");
            Assert.That(returnedValue, Is.EqualTo(new Vector2Int(1, 2)));
        }

        [Test]
        public void OpenCloseEventsAreIdempotentAndClosingClearsInput()
        {
            presenter.DisableConsole();
            int opened = 0;
            int closed = 0;
            void Opened() => opened++;
            void Closed() => closed++;
            CommandConsoleEvents.OnOpen += Opened;
            CommandConsoleEvents.OnClose += Closed;
            try
            {
                presenter.EnableConsole();
                presenter.EnableConsole();
                presenter.SetInput("unfinished");
                presenter.DisableConsole();
                presenter.DisableConsole();
                Assert.That(opened, Is.EqualTo(1));
                Assert.That(closed, Is.EqualTo(1));
                Assert.That(presenter.InputText, Is.Empty);
            }
            finally
            {
                CommandConsoleEvents.OnOpen -= Opened;
                CommandConsoleEvents.OnClose -= Closed;
            }
        }

        [UnityTest]
        public IEnumerator StaticCoroutineCommandsRunToCompletion()
        {
            RegressionCommands.CoroutineFinished = false;
            presenter.InvokeCommand("__regression-coroutine");
            yield return null;
            yield return null;
            Assert.That(RegressionCommands.CoroutineFinished, Is.True);
        }

        [UnityTest]
        public IEnumerator AutomaticConsoleNeedsNoCanvasAndSupportsShortcutsAndUnscaledRepeat()
        {
            CommandConsoleView view = CommandConsoleView.Instance;
            Assert.That(view, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<CommandConsoleView>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(view.GetComponentInChildren<Canvas>(), Is.Null);
            if (view.Presenter.IsConsoleEnabled) view.ToggleConsole();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            float oldTimeScale = Time.timeScale;
            InputSettings.UpdateMode oldUpdateMode = InputSystem.settings.updateMode;
            InputSettings.BackgroundBehavior oldBackgroundBehavior = InputSystem.settings.backgroundBehavior;
#if UNITY_EDITOR
            InputSettings.EditorInputBehaviorInPlayMode oldEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.EnableDevice(keyboard);
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Backquote));
                InputSystem.Update();
                float deadline = Time.realtimeSinceStartup + 2f;
                while (!view.Presenter.IsConsoleEnabled && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(view.Presenter.IsConsoleEnabled, Is.True,
                    $"Toggle failed: keyboard enabled={keyboard.enabled}, key pressed={keyboard.backquoteKey.isPressed}, view enabled={view.isActiveAndEnabled}, update mode={InputSystem.settings.updateMode}");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                yield return new WaitForSecondsRealtime(0.05f);
                view.Presenter.SetInput("__regression-opt");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab));
                InputSystem.Update();
                yield return new WaitForSecondsRealtime(0.05f);
                Assert.That(view.Presenter.InputText, Is.EqualTo("__regression-optional"));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                yield return new WaitForSecondsRealtime(0.05f);
                view.Presenter.SetInput(string.Empty);
                CommandConsoleModel viewModel = view.GetComponent<CommandConsoleModel>();
                Time.timeScale = 0f;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.DownArrow));
                InputSystem.Update();
                yield return new WaitForSecondsRealtime(0.6f);
                Assert.That(viewModel.SelectedAutocompleteIndex, Is.GreaterThan(1));
            }
            finally
            {
                Time.timeScale = oldTimeScale;
                InputSystem.settings.updateMode = oldUpdateMode;
                InputSystem.settings.backgroundBehavior = oldBackgroundBehavior;
#if UNITY_EDITOR
                InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorBehavior;
#endif
                InputSystem.RemoveDevice(keyboard);
                if (view.Presenter.IsConsoleEnabled) view.ToggleConsole();
            }
        }
    }

    static class RegressionCommands
    {
        public static bool CoroutineFinished;
        [ConsoleCommand("__regression-overload", "Regression")] static int Overload(int value) => value;
        [ConsoleCommand("__regression-overload", "Regression")] static float Overload(float value) => value;
        [ConsoleCommand("__regression-optional", "Regression")] static int Optional(int value = 7) => value;
        [ConsoleCommand("__regression-null", "Regression")] static string Null(string value = null) => value ?? "null default";
        [ConsoleCommand("__regression-vector", "Regression")] static string Vector(Vector3Int value) => value.ToString();
        [ConsoleCommand("__regression-vector2", "Regression")] static Vector2 Vector2Value(Vector2 value) => value;
        [ConsoleCommand("__regression-vector3", "Regression")] static Vector3 Vector3Value(Vector3 value) => value;
        [ConsoleCommand("__regression-vector2int", "Regression")] static Vector2Int Vector2IntValue(Vector2Int value) => value;
        [ConsoleCommand("__regression-enum", "Regression")] static DayOfWeek Enum(DayOfWeek value) => value;
        [ConsoleCommand("__regression-bool", "Regression")] static bool Boolean(bool value) => value;
        [ConsoleCommand("__regression-char", "Regression")] static char Character(char value) => value;
        [ConsoleCommand("__regression-long", "Regression")] static long Long(long value) => value;
        [ConsoleCommand("__regression-throws", "Regression")] static void Throws() => throw new InvalidOperationException("regression failure");
        [ConsoleCommand("__regression-coroutine", "Regression")]
        static IEnumerator Coroutine()
        {
            yield return null;
            CoroutineFinished = true;
        }
    }
}
