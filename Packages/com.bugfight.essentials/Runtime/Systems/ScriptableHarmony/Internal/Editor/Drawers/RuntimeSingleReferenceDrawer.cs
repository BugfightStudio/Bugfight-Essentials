using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEditor;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony.Internal.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GetRuntimeSingle<>))]
    internal class GetRuntimeSingleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.DrawGUI(SOType.RuntimeSingle, Access.Getter, "runtimeSingle", position, property, label, fieldInfo);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.GetPropertyHeight(property, label);
    }

    [CustomPropertyDrawer(typeof(SetRuntimeSingle<>))]
    internal class SetRuntimeSingleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => SOReferenceGUIHelper.DrawGUI(SOType.RuntimeSingle, Access.Setter, "runtimeSingle", position, property, label, fieldInfo);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.GetPropertyHeight(property, label);
    }
#endif
}

