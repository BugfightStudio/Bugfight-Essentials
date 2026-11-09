using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/ListVariables/AudioClip",
        fileName = "New AudioClip ListVariable")]
    internal class AudioClipListSO : ScriptableListSO<AudioClip> { }
}
