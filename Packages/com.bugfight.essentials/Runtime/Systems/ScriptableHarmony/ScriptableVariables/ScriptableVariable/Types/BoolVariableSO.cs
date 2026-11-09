using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/bool", 
        fileName = "New Bool Variable")]
    internal class BoolVariableSO : ScriptableVariableSO<bool> { }
}
