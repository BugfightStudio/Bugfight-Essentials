using System;
using Bugfight.Systems.ScriptableHarmony.Internal;

namespace Bugfight.Systems.ScriptableHarmony
{
    [Serializable]
    public class GetRuntimeSingle<T> : RuntimeSingleReference<T>
    {
        public T Entity => runtimeSingle.entity;
    }
}
