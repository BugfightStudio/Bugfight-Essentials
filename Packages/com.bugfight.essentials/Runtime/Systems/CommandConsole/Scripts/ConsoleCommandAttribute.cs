using System;
using JetBrains.Annotations;

namespace Bugfight.Systems.CommandConsole
{
#if RIDER
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
#endif
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute
    {
        public readonly string command;
        public readonly string commandHeader;
    
        public ConsoleCommandAttribute(string command, string header = "Uncategorized")
        {
            this.command = command;
            this.commandHeader = header;
        }
    }
}
