using Bugfight.Tools.SerializedInterface.Internal;
using UnityEditor.IMGUI.Controls;

namespace Bugfight.Tools.SerializedInterface.Internal.Editor
{
    public class NoneDropdownItem : AdvancedDropdownItem, IDropdownItem
    {
        public NoneDropdownItem()
            : base("None")
        {
        }

        ReferenceMode IDropdownItem.Mode => ReferenceMode.Raw;

        public object GetValue()
        {
            return null;
        }
    }
}
