using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/int", 
        fileName = "New Int Variable")]
    internal class IntVariableSO : ScriptableVariableSO<int> { }
}
