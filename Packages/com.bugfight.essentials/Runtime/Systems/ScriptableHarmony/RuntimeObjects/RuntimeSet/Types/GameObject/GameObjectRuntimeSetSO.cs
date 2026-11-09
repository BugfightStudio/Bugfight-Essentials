using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSets/GameObject", 
        fileName = "New GameObject RuntimeSet")]
    internal class GameObjectRuntimeSetSO : RuntimeSetSO<GameObject> { }
}

