using System;

namespace Bugfight.Internal
{
    // modified from https://github.com/BrainswitchMachina/Show-In-Inspector
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowInInspectorAttribute : Attribute { }
}