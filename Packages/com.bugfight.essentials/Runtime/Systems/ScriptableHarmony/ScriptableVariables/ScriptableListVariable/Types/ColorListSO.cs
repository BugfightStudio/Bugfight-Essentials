using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/ListVariables/Color",
        fileName = "New Color ListVariable")]
    internal class ColorListSO : ScriptableListSO<Color> { }
}
