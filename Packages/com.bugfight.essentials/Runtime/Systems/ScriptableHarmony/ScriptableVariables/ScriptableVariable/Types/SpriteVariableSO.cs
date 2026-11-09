using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/Sprite", 
        fileName = "New Sprite Variable")]
    internal class SpriteVariableSO : ScriptableVariableSO<Sprite> { }
}
