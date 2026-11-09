using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/float", 
        fileName = "New Float Variable")]
    internal class FloatVariableSO : ScriptableVariableSO<float> { }
}
