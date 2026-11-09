using UnityEngine;

namespace Bugfight.Tools.SerializedInterface.Internal.Editor
{
    internal interface IReferenceDrawer
    {
        float GetHeight();
        void OnGUI(Rect position);
    }
}
