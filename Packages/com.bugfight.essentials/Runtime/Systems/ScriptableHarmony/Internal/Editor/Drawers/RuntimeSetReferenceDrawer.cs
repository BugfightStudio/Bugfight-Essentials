using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEditor;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony.Internal.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GetRuntimeSet<>))]
    internal class GetRuntimeSetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.DrawGUI(SOType.RuntimeSet, Access.Getter, "runtimeSet", position, property, label, fieldInfo);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.GetPropertyHeight(property, label);
    }

    [CustomPropertyDrawer(typeof(SetRuntimeSet<>))]
    internal class SetRuntimeSetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => SOReferenceGUIHelper.DrawGUI(SOType.RuntimeSet, Access.Setter, "runtimeSet", position, property, label, fieldInfo);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.GetPropertyHeight(property, label);
    }
#endif
}


