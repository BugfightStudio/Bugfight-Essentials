using UnityEngine;

namespace Bugfight.Tools.Ragdoll
{
    public class RagdollLimb : MonoBehaviour
    {
        public Rigidbody rb;

        public void Setup(Ragdoll ragdoll)
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}