using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSingles/Rigidbody2D", 
        fileName = "New Rigidbody2D RuntimeSingle")]
    internal class Rigidbody2DRuntimeSingleSO : RuntimeSingleSO<Rigidbody2D> { }
}
