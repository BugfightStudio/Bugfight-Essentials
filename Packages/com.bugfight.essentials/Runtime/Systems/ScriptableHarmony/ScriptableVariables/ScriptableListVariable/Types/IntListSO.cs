using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/ListVariables/int", 
        fileName = "New Int ListVariable")]
    internal class IntListSO : ScriptableListSO<int> { }
}
