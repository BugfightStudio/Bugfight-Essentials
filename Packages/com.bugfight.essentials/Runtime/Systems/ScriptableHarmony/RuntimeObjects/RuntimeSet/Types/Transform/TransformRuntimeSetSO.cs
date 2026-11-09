using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSets/Transform", 
        fileName = "New Transform RuntimeSet")]
    internal class TransformRuntimeSetSO : RuntimeSetSO<Transform> { }
}
