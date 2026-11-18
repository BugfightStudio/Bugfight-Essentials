using UnityEngine;

namespace Bugfight.Tools.General
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyCenterOfMassGizmo : MonoBehaviour
    {
        [SerializeField, Min(0f)] float radius = 0.1f;
        [SerializeField] Color color = Color.yellow;

        Rigidbody _rigidbody;

        void OnDrawGizmos()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody == null)
                return;

            Color previousColor = Gizmos.color;
            Matrix4x4 previousMatrix = Gizmos.matrix;

            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawSphere(_rigidbody.worldCenterOfMass, Mathf.Max(0f, radius));

            Gizmos.color = previousColor;
            Gizmos.matrix = previousMatrix;
        }
    }
}
