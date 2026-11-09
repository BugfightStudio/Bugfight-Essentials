using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/ListVariables/Material",
        fileName = "New Material ListVariable")]
    internal class MaterialListSO : ScriptableListSO<Material> { }
}
