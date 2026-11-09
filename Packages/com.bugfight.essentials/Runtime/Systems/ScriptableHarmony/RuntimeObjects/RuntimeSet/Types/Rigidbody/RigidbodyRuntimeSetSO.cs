using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSets/Rigidbody", 
        fileName = "New Rigidbody RuntimeSet")]
    internal class RigidbodyRuntimeSetSO : RuntimeSetSO<Rigidbody> { }
}
