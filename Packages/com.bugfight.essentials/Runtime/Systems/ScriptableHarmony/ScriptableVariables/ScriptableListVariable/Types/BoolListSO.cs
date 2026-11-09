using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/ListVariables/bool",
        fileName = "New Bool ListVariable")]
    internal class BoolListSO : ScriptableListSO<bool> { }
}
