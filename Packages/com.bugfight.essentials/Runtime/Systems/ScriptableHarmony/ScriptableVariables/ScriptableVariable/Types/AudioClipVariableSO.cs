using Bugfight.Systems.ScriptableHarmony.Internal;
using UnityEngine;

namespace Bugfight.Systems.ScriptableHarmony
{   
    [CreateAssetMenu(
        menuName = "ScriptableHarmony/Common/Variables/AudioClip", 
        fileName = "New AudioClip Variable")]
    internal class AudioClipVariableSO : ScriptableVariableSO<AudioClip> { }
}
