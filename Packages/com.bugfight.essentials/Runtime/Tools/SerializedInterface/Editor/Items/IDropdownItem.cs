using Bugfight.Tools.SerializedInterface.Internal;

namespace Bugfight.Tools.SerializedInterface.Internal.Editor
{
    internal interface IDropdownItem
    {
        internal ReferenceMode Mode { get; }
        object GetValue();
    }
}
