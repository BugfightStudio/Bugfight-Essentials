using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Bugfight.Systems.CommandConsole
{
    public class CommandConsoleModel : MonoBehaviour
    {
        const string COLLAPSE_MESSAGES_KEY = "COMMAND_CONSOLE_COLLAPSE_MESSAGES";

        readonly List<ConsoleMessage> messages = new();
        readonly List<ConsoleMessage> collapsedMessages = new();
        readonly Dictionary<MessageKey, ConsoleMessage> messagesByKey = new();

        public Dictionary<CommandKey, MethodInfo> RegisteredCommands { get; set; } = new();
        public Dictionary<CommandKey, ConsoleCommandAttribute> RegisteredCommandAttributes { get; set; } = new();
        public List<CommandKey> AutocompleteOptions { get; } = new();
        public CommandKey SelectedCommand { get; set; } = CommandKey.empty;
        public int SelectedAutocompleteIndex { get; set; } = -1;
        public bool IsConsoleEnabled { get; set; }
        public bool CollapseMessages { get; set; }
        public IReadOnlyList<ConsoleMessage> Logs => CollapseMessages ? collapsedMessages : messages;

        public void AddMessage(object message, LogType logType)
        {
            MessageKey key = new(message?.ToString() ?? "null", logType);
            messages.Add(new ConsoleMessage(key));
            if (messagesByKey.TryGetValue(key, out ConsoleMessage existing))
            {
                existing.IncrementMessageCount();
                return;
            }

            ConsoleMessage collapsedMessage = new(key);
            messagesByKey.Add(key, collapsedMessage);
            collapsedMessages.Add(collapsedMessage);
        }

        public void ClearMessages()
        {
            messages.Clear();
            collapsedMessages.Clear();
            messagesByKey.Clear();
        }

        public bool GetSavedCollapseMessagesValue()
        {
            CollapseMessages = PlayerPrefs.GetInt(COLLAPSE_MESSAGES_KEY, CollapseMessages ? 1 : 0) == 1;
            return CollapseMessages;
        }

        public void SetSavedCollapseMessagesValue()
        {
            PlayerPrefs.SetInt(COLLAPSE_MESSAGES_KEY, CollapseMessages ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
