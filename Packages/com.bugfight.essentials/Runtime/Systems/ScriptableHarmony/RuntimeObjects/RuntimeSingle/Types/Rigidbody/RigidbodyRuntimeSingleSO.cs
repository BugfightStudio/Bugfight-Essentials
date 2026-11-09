using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSingles/Rigidbody", 
        fileName = "New Rigidbody RuntimeSingle")]
    internal class RigidbodyRuntimeSingleSO : RuntimeSingleSO<Rigidbody> { }
}
