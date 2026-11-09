using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSingles/GameObject", 
        fileName = "New GameObject RuntimeSingle")]
    internal class GameObjectRuntimeSingleSO : RuntimeSingleSO<GameObject> { }
}

