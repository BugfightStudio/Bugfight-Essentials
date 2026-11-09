using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEditor;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony.Internal.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GetScriptableList<>))]
    internal class GetListVariableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.DrawGUI(SOType.List, Access.Getter, "list", position, property, label, fieldInfo);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.GetPropertyHeight(property, label);
    }

    [CustomPropertyDrawer(typeof(SetScriptableList<>))]
    internal class SetListVariableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => SOReferenceGUIHelper.DrawGUI(SOType.List, Access.Setter, "list", position, property, label, fieldInfo);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => SOReferenceGUIHelper.GetPropertyHeight(property, label);
    }
#endif
}

