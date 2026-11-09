using Bugfight.Systems.CommandConsole;
using UnityEngine;

namespace Bugfight.Tests.CommandConsole
{
    public class CommandConsoleTestTarget : MonoBehaviour
    {
        public int Value { get; private set; }

        [ConsoleCommand("__regression-instance", "Regression")]
        void SetValue(int value) => Value = value;
    }
}
