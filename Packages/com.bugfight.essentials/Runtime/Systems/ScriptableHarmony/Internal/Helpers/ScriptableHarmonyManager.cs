using System;

namespace Bugfight.Systems.ScriptableHarmony
{
    public static class ScriptableHarmonyManager
    {
        internal static event Action OnResetAllVariableObjects;
        public static void ResetAllVariableObjects() => OnResetAllVariableObjects?.Invoke();
    }
}
