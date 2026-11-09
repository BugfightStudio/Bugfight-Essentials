using UnityEngine;
using Bugfight.Systems.ScriptableHarmony.Internal;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/RuntimeSets/AudioSource",
        fileName = "New AudioSource RuntimeSet")]
    internal class AudioSourceRuntimeSetSO : RuntimeSetSO<AudioSource> { }
}
