using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/Color", 
        fileName = "New Color Variable")]
    internal class ColorVariableSO : ScriptableVariableSO<Color> { }
}
