using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/Vector3", 
        fileName = "New Vector3 Variable")]
    internal class Vector3VariableSO : ScriptableVariableSO<Vector3> { }
}
