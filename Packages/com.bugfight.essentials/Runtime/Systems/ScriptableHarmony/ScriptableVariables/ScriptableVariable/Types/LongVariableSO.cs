using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/long", 
        fileName = "New Long Variable")]
    internal class LongVariableSO : ScriptableVariableSO<long> { }
}
