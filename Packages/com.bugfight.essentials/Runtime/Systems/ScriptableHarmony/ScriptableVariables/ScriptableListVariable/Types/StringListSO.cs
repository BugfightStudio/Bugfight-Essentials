using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/ListVariables/string",
        fileName = "New String ListVariable")]
    internal class StringListSO : ScriptableListSO<string> { }
}
