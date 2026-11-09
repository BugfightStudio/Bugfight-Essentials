using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Bugfight.Systems.CommandConsole
{
    /// <summary>A persistent, prefab-free IMGUI frontend for the command console.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CommandConsolePresenter))]
    public class CommandConsoleView : MonoBehaviour
    {
        const string INPUT_CONTROL_NAME = "Bugfight.CommandConsole.Input";
        const float PADDING = 8f;
        const float ROW_HEIGHT = 28f;
        const float COLLAPSE_CHECKBOX_SIZE = 24f;

        [Header("Input (empty actions use the default bindings)")]
        [SerializeField] InputActionProperty toggleConsoleInputAction;
        [SerializeField] InputActionProperty deleteLastWordInputAction;
        [SerializeField] InputActionProperty autoCompleteInputAction;
        [SerializeField] InputActionProperty commandHistoryUpInputAction;
        [SerializeField] InputActionProperty commandHistoryDownInputAction;
        [SerializeField, Min(0f)] float autocompleteRepeatDelay = 0.35f;
        [SerializeField, Min(0.01f)] float autocompleteRepeatRate = 0.08f;

        [Header("Appearance")]
        [SerializeField, Range(0.25f, 1f)] float screenHeight = 0.4f;
        [SerializeField, Min(10)] int fontSize = 16;
        [SerializeField, Min(1)] int maximumAutocompleteOptions = 6;

        [SerializeField] CommandConsolePresenter presenter;

        InputAction[] actions;
        bool[] enabledByConsole;
        bool[] ownedByConsole;
        bool inputActive;
        GUIStyle labelStyle;
        GUIStyle suggestionsStyle;
        GUIStyle inputStyle;
        GUIStyle buttonStyle;
        TextEditor inputEditor;
        Vector2 messageScroll;
        bool focusInput;
        int requestedCaret = -1;
        bool deleteWordRequested;
        bool scrollToBottom = true;
        bool followMessages = true;
        bool consoleWasVisible;
        int inputControlId;
        int completedInputFrame = -1;
        float autocompleteRepeatTimer;
        int autocompleteRepeatDirection;

        public static CommandConsoleView Instance { get; private set; }
        public CommandConsolePresenter Presenter => presenter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetInstance() => Instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateConsole()
        {
            if (Instance != null) return;
            CommandConsoleView existing = FindFirstObjectByType<CommandConsoleView>();
            if (existing != null)
            {
                existing.Initialize();
                return;
            }
            new GameObject("CommandConsole").AddComponent<CommandConsoleView>();
        }

        void Awake() => Initialize();

        void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                // Do not destroy a scene object that may contain unrelated components.
                enabled = false;
                GetComponent<CommandConsolePresenter>().enabled = false;
                return;
            }
            Instance = this;
            if (presenter == null) presenter = GetComponent<CommandConsolePresenter>();
            presenter.Initialize();
            DontDestroyOnLoad(gameObject);
            if (actions == null) InitializeActions();
            if (isActiveAndEnabled) EnableInput();
        }

        void InitializeActions()
        {
            InputAction deleteWord = new("Delete Last Word", InputActionType.Button);
            deleteWord.AddCompositeBinding("OneModifier")
                .With("Modifier", "<Keyboard>/ctrl").With("Binding", "<Keyboard>/backspace");
            InputAction complete = new("Auto Complete", InputActionType.Button, "<Keyboard>/tab");
            complete.AddBinding("<Keyboard>/rightArrow");
            InputAction[] defaults =
            {
                new("Toggle Console", InputActionType.Button, "<Keyboard>/backquote"),
                deleteWord, complete,
                new("Previous Completion", InputActionType.Button, "<Keyboard>/upArrow"),
                new("Next Completion", InputActionType.Button, "<Keyboard>/downArrow")
            };
            InputActionProperty[] configured =
            {
                toggleConsoleInputAction, deleteLastWordInputAction, autoCompleteInputAction,
                commandHistoryUpInputAction, commandHistoryDownInputAction
            };
            actions = new InputAction[defaults.Length];
            enabledByConsole = new bool[defaults.Length];
            ownedByConsole = new bool[defaults.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                InputAction configuredAction = configured[i].action;
                ownedByConsole[i] = configuredAction == null || configuredAction.bindings.Count == 0;
                actions[i] = ownedByConsole[i] ? defaults[i] : configuredAction;
                if (!ownedByConsole[i]) defaults[i].Dispose();
            }
        }

        void OnEnable()
        {
            if (Instance != this) return;
            EnableInput();
        }

        void EnableInput()
        {
            if (inputActive) return;
            inputActive = true;
            actions[0].performed += ToggleConsoleInputHandler;
            actions[1].performed += DeleteWordInputHandler;
            actions[2].performed += CompleteInputHandler;
            actions[3].performed += PreviousCompletionInputHandler;
            actions[4].performed += NextCompletionInputHandler;
            for (int i = 0; i < actions.Length; i++)
            {
                enabledByConsole[i] = !actions[i].enabled;
                if (enabledByConsole[i]) actions[i].Enable();
            }
            presenter.OnCommandLogRecieved += MessageReceived;
        }

        void OnDisable()
        {
            if (actions == null) return;
            inputActive = false;
            actions[0].performed -= ToggleConsoleInputHandler;
            actions[1].performed -= DeleteWordInputHandler;
            actions[2].performed -= CompleteInputHandler;
            actions[3].performed -= PreviousCompletionInputHandler;
            actions[4].performed -= NextCompletionInputHandler;
            presenter.OnCommandLogRecieved -= MessageReceived;
            presenter.DisableConsole();
            for (int i = 0; i < actions.Length; i++)
            {
                if (enabledByConsole[i]) actions[i].Disable();
            }
            autocompleteRepeatDirection = 0;
            deleteWordRequested = false;
            inputEditor = null;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (actions == null) return;
            for (int i = 0; i < actions.Length; i++)
            {
                if (ownedByConsole[i]) actions[i].Dispose();
            }
        }

        void MessageReceived(object message, LogType logType)
        {
            if (followMessages) scrollToBottom = true;
        }

        public void ToggleConsole()
        {
            presenter.ToggleConsole();
            if (presenter.IsConsoleEnabled) RequestInputFocus(presenter.InputText.Length);
            else
            {
                focusInput = false;
                requestedCaret = -1;
                deleteWordRequested = false;
                inputEditor = null;
            }
        }

        void RequestInputFocus(int caret)
        {
            focusInput = true;
            requestedCaret = caret;
        }

        void ToggleConsoleInputHandler(InputAction.CallbackContext context) => ToggleConsole();
        void DeleteWordInputHandler(InputAction.CallbackContext context)
        {
            if (presenter.IsConsoleEnabled) deleteWordRequested = true;
        }

        void CompleteInputHandler(InputAction.CallbackContext context)
        {
            if (presenter.FillAutoCompletedText())
            {
                completedInputFrame = Time.frameCount;
                RequestInputFocus(presenter.InputText.Length);
            }
        }

        void PreviousCompletionInputHandler(InputAction.CallbackContext context)
        {
            if (presenter.IsConsoleEnabled) CycleAutocomplete(-1);
        }

        void NextCompletionInputHandler(InputAction.CallbackContext context)
        {
            if (presenter.IsConsoleEnabled) CycleAutocomplete(1);
        }

        void Update()
        {
            if (!presenter.IsConsoleEnabled)
            {
                autocompleteRepeatDirection = 0;
                return;
            }
            RepeatAutocomplete();
        }

        void CycleAutocomplete(int direction)
        {
            presenter.CycleAutocompleteSelection(direction);
            // Navigation keeps the caret where it was; it only changes the selected suggestion.
            focusInput = true;
        }

        void RepeatAutocomplete()
        {
            int direction = actions[3].IsPressed() ? -1 : actions[4].IsPressed() ? 1 : 0;
            if (direction == 0 || direction != autocompleteRepeatDirection)
            {
                autocompleteRepeatDirection = direction;
                autocompleteRepeatTimer = autocompleteRepeatDelay;
                return;
            }
            autocompleteRepeatTimer -= Time.unscaledDeltaTime;
            if (autocompleteRepeatTimer > 0f) return;
            CycleAutocomplete(direction);
            autocompleteRepeatTimer = autocompleteRepeatRate;
        }

        void OnGUI()
        {
            if (!presenter.IsConsoleEnabled)
            {
                if (consoleWasVisible && GUIUtility.keyboardControl == inputControlId) GUIUtility.keyboardControl = 0;
                consoleWasVisible = false;
                return;
            }
            if (!consoleWasVisible) RequestInputFocus(presenter.InputText.Length);
            consoleWasVisible = true;
            InitializeStyles();
            HandleKeyboardEvent();
            if (!presenter.IsConsoleEnabled) return;
            DeleteRequestedWord();

            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            GUI.depth = -10000;
            GUI.color = Color.white;
            try { DrawConsole(); }
            finally
            {
                GUI.depth = previousDepth;
                GUI.color = previousColor;
            }
        }

        void InitializeStyles()
        {
            if (labelStyle != null && labelStyle.fontSize == fontSize) return;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true, wordWrap = true };
            labelStyle.normal.textColor = Color.white;
            suggestionsStyle = new GUIStyle(labelStyle) { wordWrap = false };
            inputStyle = new GUIStyle(GUI.skin.textField) { fontSize = fontSize, richText = false };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };
        }

        void HandleKeyboardEvent()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown) return;
            if (current.keyCode == KeyCode.Escape)
            {
                presenter.DisableConsole();
                GUIUtility.keyboardControl = 0;
                current.Use();
                return;
            }
            if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) &&
                (GUI.GetNameOfFocusedControl() == INPUT_CONTROL_NAME || focusInput))
            {
                if (presenter.SubmitCommand()) scrollToBottom = true;
                RequestInputFocus(presenter.InputText.Length);
                current.Use();
                return;
            }

            // IMGUI must not also insert a shortcut character or apply native arrow/tab navigation.
            for (int i = 0; i < actions.Length; i++)
            {
                // Right Arrow remains normal caret navigation when there is nothing to complete.
                if (i == 2 && current.keyCode == KeyCode.RightArrow && completedInputFrame != Time.frameCount) continue;
                InputAction action = actions[i];
                if (!action.IsPressed() && !action.WasPerformedThisFrame()) continue;
                foreach (InputControl control in action.controls)
                {
                    if (!(control is KeyControl key)) continue;
                    if (Enum.TryParse(current.keyCode.ToString(), true, out Key eventKey) && key.keyCode == eventKey)
                    {
                        current.Use();
                        return;
                    }
                }
            }
        }

        void DeleteRequestedWord()
        {
            if (!deleteWordRequested) return;
            deleteWordRequested = false;
            int cursor = inputEditor?.cursorIndex ?? presenter.InputText.Length;
            int selection = inputEditor?.selectIndex ?? cursor;
            presenter.SetInput(CommandConsolePresenter.DeleteTextBlock(presenter.InputText, ref cursor, ref selection));
            if (inputEditor != null)
            {
                inputEditor.text = presenter.InputText;
                inputEditor.cursorIndex = cursor;
                inputEditor.selectIndex = selection;
            }
            RequestInputFocus(cursor);
        }

        void DrawConsole()
        {
            float height = Mathf.Max(1f, Screen.height * screenHeight - PADDING * 2);
            Rect panel = new(PADDING, Screen.height - height - PADDING,
                Mathf.Max(1f, Screen.width - PADDING * 2), height);
            DrawBackground(panel, new Color(0.06f, 0.06f, 0.07f, 0.97f));
            float left = panel.x + PADDING;
            float width = Mathf.Max(1f, panel.width - PADDING * 2);
            DrawToolbar(new Rect(left, panel.y + PADDING, width, ROW_HEIGHT));
            if (!presenter.IsConsoleEnabled) return;

            float inputY = panel.yMax - ROW_HEIGHT - PADDING;
            float hintY = inputY - ROW_HEIGHT;
            float maximumOptionsHeight = panel.height * 0.4f;
            // Every visible command may have a header. Fit both rows so the selection is never clipped.
            float optionRowHeight = Mathf.Max(fontSize, suggestionsStyle.lineHeight);
            int optionCapacity = Mathf.Max(1, Mathf.FloorToInt((maximumOptionsHeight - suggestionsStyle.padding.vertical) / (optionRowHeight * 2f)));
            int visibleOptions = Mathf.Min(maximumAutocompleteOptions, optionCapacity);
            string options = presenter.BuildAutocompleteOptionsString(visibleOptions);
            float optionsHeight = options.Length == 0 ? 0f : Mathf.Min(suggestionsStyle.CalcHeight(new GUIContent(options), width), maximumOptionsHeight);
            float optionsY = hintY - optionsHeight - PADDING;
            float messagesY = panel.y + ROW_HEIGHT + PADDING * 2;
            DrawMessages(new Rect(left, messagesY, width, Mathf.Max(1f, optionsY - messagesY - PADDING)));
            if (optionsHeight > 0f)
            {
                Rect optionsRect = new(left, optionsY, width, optionsHeight);
                DrawBackground(optionsRect, new Color(0f, 0f, 0f, 0.6f));
                GUI.Label(optionsRect, options, suggestionsStyle);
            }
            GUI.Label(new Rect(left, hintY, width, ROW_HEIGHT), presenter.BuildPlaceholderString(), labelStyle);
            DrawInput(new Rect(left, inputY, width, ROW_HEIGHT));

            // Consume pointer events inside the overlay so other IMGUI controls cannot click through.
            if (Event.current.isMouse && panel.Contains(Event.current.mousePosition)) Event.current.Use();
        }

        void DrawToolbar(Rect rect)
        {
            GUI.Label(new Rect(rect.x, rect.y, Mathf.Max(0, rect.width - 310), rect.height), "Command Console", labelStyle);
            float right = rect.xMax;
            if (GUI.Button(new Rect(right - 70, rect.y, 70, rect.height), "Close", buttonStyle))
            {
                ToggleConsole();
                GUIUtility.keyboardControl = 0;
            }
            if (GUI.Button(new Rect(right - 148, rect.y, 70, rect.height), "Clear", buttonStyle))
            {
                presenter.ClearMessages();
                messageScroll = Vector2.zero;
                followMessages = true;
                RequestInputFocus(presenter.InputText.Length);
            }
            bool collapse = DrawCollapseToggle(new Rect(right - 278, rect.y, 120, rect.height), presenter.CollapseMessages);
            if (collapse != presenter.CollapseMessages)
            {
                presenter.ToggleMessageCollapsing(collapse);
                scrollToBottom = true;
            }
        }

        bool DrawCollapseToggle(Rect rect, bool value)
        {
            // Keep native toggle interaction and make the whole label clickable, but draw a larger indicator.
            bool selected = GUI.Toggle(rect, value, GUIContent.none, GUIStyle.none);
            Rect checkbox = new(rect.x, rect.y + (rect.height - COLLAPSE_CHECKBOX_SIZE) * 0.5f,
                COLLAPSE_CHECKBOX_SIZE, COLLAPSE_CHECKBOX_SIZE);
            DrawBackground(checkbox, new Color(0.65f, 0.65f, 0.65f));
            DrawBackground(new Rect(checkbox.x + 2, checkbox.y + 2, checkbox.width - 4, checkbox.height - 4),
                new Color(0.12f, 0.12f, 0.12f));
            if (selected)
            {
                Vector2 start = checkbox.position + new Vector2(0.22f, 0.5f) * COLLAPSE_CHECKBOX_SIZE;
                Vector2 corner = checkbox.position + new Vector2(0.43f, 0.72f) * COLLAPSE_CHECKBOX_SIZE;
                Vector2 end = checkbox.position + new Vector2(0.8f, 0.28f) * COLLAPSE_CHECKBOX_SIZE;
                DrawLine(start, corner, 3f);
                DrawLine(corner, end, 3f);
            }
            float labelLeft = checkbox.xMax + PADDING;
            GUI.Label(new Rect(labelLeft, rect.y + (rect.height - fontSize) * 0.5f - labelStyle.padding.top,
                rect.xMax - labelLeft, rect.height), "Collapse", labelStyle);
            return selected;
        }

        static void DrawLine(Vector2 start, Vector2 end, float thickness)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            Vector2 direction = end - start;
            try
            {
                GUIUtility.RotateAroundPivot(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, start);
                GUI.DrawTexture(new Rect(start.x, start.y - thickness * 0.5f, direction.magnitude, thickness), Texture2D.whiteTexture);
            }
            finally { GUI.matrix = previousMatrix; }
        }

        void DrawMessages(Rect rect)
        {
            float contentWidth = Mathf.Max(1f, rect.width - 20f);
            float textWidth = Mathf.Max(1f, contentWidth - 100f);
            float contentHeight = 0f;
            foreach (ConsoleMessage message in presenter.Logs)
                contentHeight += Mathf.Max(ROW_HEIGHT, labelStyle.CalcHeight(new GUIContent(message.Key.message), textWidth));
            if (scrollToBottom)
            {
                messageScroll.y = Mathf.Max(0f, contentHeight - rect.height);
                scrollToBottom = false;
            }

            messageScroll = GUI.BeginScrollView(rect, messageScroll, new Rect(0, 0, contentWidth, contentHeight), false, true);
            float y = 0f;
            foreach (ConsoleMessage message in presenter.Logs)
            {
                float height = Mathf.Max(ROW_HEIGHT, labelStyle.CalcHeight(new GUIContent(message.Key.message), textWidth));
                if (y + height >= messageScroll.y && y <= messageScroll.y + rect.height)
                {
                    GUI.color = GetLogColor(message.Key.logType);
                    GUI.Label(new Rect(0, y, 45, height), GetLogSymbol(message.Key.logType), labelStyle);
                    GUI.Label(new Rect(45, y, textWidth, height), message.Key.message, labelStyle);
                    GUI.color = Color.white;
                    if (presenter.CollapseMessages)
                        GUI.Label(new Rect(contentWidth - 48, y, 48, height), message.Count.ToString(), labelStyle);
                }
                y += height;
            }
            GUI.EndScrollView();
            followMessages = messageScroll.y >= Mathf.Max(0f, contentHeight - rect.height) - 1f;
        }

        void DrawInput(Rect rect)
        {
            GUI.SetNextControlName(INPUT_CONTROL_NAME);
            string text = GUI.TextField(rect, presenter.InputText, inputStyle);
            if (text != presenter.InputText) presenter.SetInput(text);
            if (focusInput)
            {
                GUI.FocusControl(INPUT_CONTROL_NAME);
                // Wait until the named control has actually acquired keyboard focus.
                focusInput = GUI.GetNameOfFocusedControl() != INPUT_CONTROL_NAME;
            }
            if (GUI.GetNameOfFocusedControl() != INPUT_CONTROL_NAME) return;
            inputControlId = GUIUtility.keyboardControl;
            inputEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), inputControlId);
            if (requestedCaret < 0) return;
            inputEditor.text = presenter.InputText;
            inputEditor.cursorIndex = inputEditor.selectIndex = Mathf.Clamp(requestedCaret, 0, presenter.InputText.Length);
            requestedCaret = -1;
        }

        static void DrawBackground(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        static Color GetLogColor(LogType type) => type switch
        {
            LogType.Warning => new Color(1f, 0.8f, 0.3f),
            LogType.Error or LogType.Exception or LogType.Assert => new Color(1f, 0.4f, 0.4f),
            _ => Color.white
        };

        static string GetLogSymbol(LogType type) => type switch
        {
            LogType.Warning => "[!]",
            LogType.Error => "[X]",
            LogType.Exception => "[EX]",
            LogType.Assert => "[A]",
            _ => "[i]"
        };
    }
}
