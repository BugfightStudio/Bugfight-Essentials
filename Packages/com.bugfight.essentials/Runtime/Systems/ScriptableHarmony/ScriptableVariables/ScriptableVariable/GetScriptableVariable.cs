using System;
using Bugfight.Systems.ScriptableHarmony.Internal;

namespace Bugfight.Systems.ScriptableHarmony
{
    [Serializable]
    public class GetScriptableVariable<T> : ScriptableVariableReference<T>
    {
        public T Val => variable.value;
        public T DefaultVal => variable.DefaultValue;
    }
}
