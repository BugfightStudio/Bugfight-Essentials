using System;
using System.Diagnostics;
using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEditor;

namespace Bugfight.Systems.ScriptableHarmony
{
    [Serializable]
    public class SetScriptableVariable<T> : ScriptableVariableReference<T>
    {
        public T Val => variable.value;
        public T DefaultVal => variable.DefaultValue;
        
        public void Set(T value)
        {
            T oldValue = variable.value;
            variable.value = value;
            
            SetDirty();

            variable.onChangeWithOld?.Invoke(oldValue, value);
            variable.onChange?.Invoke(value);
            
            SHLogger.LogSet("Set Value", SOType.Variable, oldValue?.ToString(), Val?.ToString(), variable);
        }

        public void ResetValue()
        {
            Set(variable.DefaultValue);
        }

        [Conditional("UNITY_EDITOR")]
        void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(variable);
#endif
        }
    }
}
