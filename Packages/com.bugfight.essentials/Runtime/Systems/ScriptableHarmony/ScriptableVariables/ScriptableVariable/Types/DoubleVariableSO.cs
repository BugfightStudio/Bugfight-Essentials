using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/double", 
        fileName = "New Double Variable")]
    internal class DoubleVariableSO : ScriptableVariableSO<double> { }
}
