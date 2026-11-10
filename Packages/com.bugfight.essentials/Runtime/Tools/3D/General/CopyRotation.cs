using UnityEngine;

namespace Bugfight.Tools.General
{
    /// <summary>Copies a target's world-space rotation during the selected update phase.</summary>
    public class CopyRotation : MonoBehaviour
    {
        public enum UpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        [SerializeField] Transform target;
        [SerializeField] UpdateMode updateMode = UpdateMode.Update;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public UpdateMode Mode
        {
            get => updateMode;
            set => updateMode = value;
        }

        void Update()
        {
            if (updateMode == UpdateMode.Update) Copy();
        }

        void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate) Copy();
        }

        void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate) Copy();
        }

        void Copy()
        {
            // Keep the component enabled so a target can be assigned later.
            if (target != null) transform.rotation = target.rotation;
        }
    }
}
