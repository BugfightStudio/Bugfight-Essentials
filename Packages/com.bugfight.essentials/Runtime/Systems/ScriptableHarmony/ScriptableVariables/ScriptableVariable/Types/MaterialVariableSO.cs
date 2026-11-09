using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/Material", 
        fileName = "New Material Variable")]
    internal class MaterialVariableSO : ScriptableVariableSO<Material> { }
}
