using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSingles/Transform", 
        fileName = "New Transform RuntimeSingle")]
    internal class TransformRuntimeSingleSO : RuntimeSingleSO<Transform> { }
}
