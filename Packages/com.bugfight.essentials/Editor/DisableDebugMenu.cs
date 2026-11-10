using UnityEditor;
using UnityEngine.Rendering;

namespace Bugfight.Editor
{
    public static class DisableDebugMenu
    {
        [InitializeOnLoadMethod]
        static void OnEditorLoaded()
        {
            DebugManager.instance.enableRuntimeUI = false;
        }
    }
}
