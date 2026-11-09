namespace Bugfight.Systems.CommandConsole
{
    /// <summary>A log entry, independent of the console's rendering system.</summary>
    public sealed class ConsoleMessage
    {
        public MessageKey Key { get; }
        public int Count { get; private set; } = 1;

        public ConsoleMessage(MessageKey key) => Key = key;

        public void IncrementMessageCount() => Count++;
    }
}
