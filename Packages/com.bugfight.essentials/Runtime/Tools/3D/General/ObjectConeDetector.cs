using System;
using System.Collections.Generic;
using Bugfight.Utils;
using UnityEngine;

namespace Bugfight.Tools.General
{
    [Serializable]
    public class ObjectConeDetector
    {
        [SerializeField] public Collider self;
        [field: SerializeField] public float MaxRange { get; private set; }
        [field: SerializeField] public float FOVRadius { get; private set; }
        [field: SerializeField] public float BaseRadius { get; private set; } // NEW
        [field: SerializeField] public LayerMask LayerMask { get; private set; }
        [field: SerializeField] public LayerMask ObstacleMask { get; private set; }
        [field: SerializeField] public float ObstacleCastOffset { get; private set; }
        
        public Collider Self => self;

        HashSet<Collider> IgnoredColliders
        {
            get
            {
                _ignoredColliders ??= new HashSet<Collider>();
                _ignoredColliders.RemoveWhere(col => col == null);
                return _ignoredColliders;
            }
        }
        HashSet<Collider> _ignoredColliders;

        public ObjectConeDetector(float maxRange, float fovRadius, float baseRadius, LayerMask layerMask, Collider self = null)
        {
            this.self = self;
            this.MaxRange = maxRange;
            this.FOVRadius = fovRadius;
            this.BaseRadius = baseRadius;
            this.LayerMask = layerMask;
        }

        bool InThickCone(Vector3 baseCenter, Vector3 forward, Vector3 point)
        {
            Vector3 baseToPoint = point - baseCenter;
            
            Vector3 axis = forward.normalized;
            float axisDot = Vector3.Dot(baseToPoint, axis);
            if (axisDot < 0 || axisDot > MaxRange) return false;

            float radiusAtPoint = Mathf.Lerp(BaseRadius, Mathf.Tan(FOVRadius * Mathf.Deg2Rad) * MaxRange, axisDot / MaxRange);
            
            Vector3 proj = baseCenter + axis * axisDot;
            float perpendicularDist = (point - proj).magnitude;

            return perpendicularDist <= radiusAtPoint;
        }

        /// <summary>
        /// Returns unique transforms whose positions are inside the cone and are not obstructed.
        /// </summary>
        public List<Transform> DetectAll(Vector3 startPos, Vector3 dir, Func<Collider, bool> ignoreAction = null)
        {
            List<Transform> targets = new();
            if (MaxRange <= 0 || dir.sqrMagnitude == 0) return targets;

            dir.Normalize();

            float tipRadius = Mathf.Tan(FOVRadius * Mathf.Deg2Rad) * MaxRange;
            float searchRadius = Mathf.Max(BaseRadius, Mathf.Sqrt(MaxRange * MaxRange + tipRadius * tipRadius));
            Collider[] hits = Physics.OverlapSphere(startPos, searchRadius, LayerMask);
            HashSet<Collider> ignoredColliders = IgnoredColliders;
            HashSet<Transform> detected = new();

            foreach (var hit in hits)
            {
                if (self != null && hit.transform == self.transform) continue;
                if (ignoredColliders.Contains(hit)) continue;
                if (ignoreAction != null && ignoreAction.Invoke(hit)) continue;

                Transform target = hit.transform;
                if (detected.Contains(target)) continue;
                if (!InThickCone(startPos, dir, target.position)) continue;
                if (HitObstacle(startPos, target.position, target)) continue;

                detected.Add(target);
                targets.Add(target);
            }

            return targets;
        }

        public Transform DetectBest(Vector3 startPos, Vector3 dir, Func<Collider, bool> ignoreAction = null)
        {
            dir.Normalize();
            
            Collider[] hits = Physics.OverlapSphere(startPos, MaxRange + BaseRadius, LayerMask);
            Transform bestTarget = null;
            float bestScore = float.MinValue;

            bool hasCol = self != null;

            foreach (var hit in hits)
            {
                if (hasCol && hit.transform == self.transform) continue;
                if (ignoreAction != null && ignoreAction.Invoke(hit)) continue;

                Vector3 targetPos = hit.transform.position;
                if (!InThickCone(startPos, dir, targetPos)) continue;

                if (HitObstacle(startPos, targetPos, hit.transform))
                {
                    continue;
                }

                Vector3 dirToTarget = targetPos - startPos;
                float angle = Vector3.Angle(dir, dirToTarget);

                float distance = dirToTarget.magnitude;
                float angleScore = Mathf.Cos(angle * Mathf.Deg2Rad);
                float distanceScore = 1f / (distance + 0.01f);

                float score = angleScore * 2f + distanceScore;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = hit.transform;
                }
            }

            return bestTarget;
        }

        public bool TryDetectBest(Vector3 startPos, Vector3 dir, out Transform selected, Func<Collider, bool> ignoreAction = null)
        {
            selected = DetectBest(startPos, dir, ignoreAction);
            return selected != null;
        }

        public bool DetectBestPoint(Vector3 startPos, Vector3 dir, out Vector3 hitPoint, out Transform hitTransform)
        {
            hitPoint = Vector3.zero;
            dir.Normalize();

            hitTransform = null;

            // Center ray first
            if (SphereCastHit(startPos, dir, out RaycastHit hit))
            {
                hitTransform = hit.transform;
                hitPoint = hit.point;
                return true;
            }

            // Then rings inside-out
            int rings = 3;
            int raysPerRing = 8;

            for (int ring = 1; ring <= rings; ring++)
            {
                float ringAngle = (FOVRadius / rings) * ring;
                for (int i = 0; i < raysPerRing; i++)
                {
                    float azimuth = (360f / raysPerRing) * i;
                    Vector3 coneDir = GetConeDirection(dir, ringAngle, azimuth);
                    if (SphereCastHit(startPos, coneDir, out hit))
                    {
                        hitTransform = hit.transform;
                        hitPoint = hit.point;
                        return true;
                    }
                }
            }

            return false;
        }

        bool SphereCastHit(Vector3 startPos, Vector3 dir, out RaycastHit hit)
        {
            RaycastHit[] hits = Physics.SphereCastAll(startPos, BaseRadius, dir, MaxRange, LayerMask);
            hit = new();
            foreach (var h in hits)
            {
                if ((self != null && h.transform == self.transform) || Mathf.Approximately(h.distance, 0)) continue;
                if(IgnoredColliders.Contains(h.collider)) continue;

                if (HitObstacle(startPos, h.point, hit.transform))
                {
                    continue;
                }
                
                hit = h;
                return true;
            }
            return false;
        }

        Vector3 GetConeDirection(Vector3 forward, float coneAngle, float azimuthDeg)
        {
            Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f
                ? Vector3.forward
                : Vector3.up;

            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            float azRad = azimuthDeg * Mathf.Deg2Rad;
            Vector3 perp = (Mathf.Cos(azRad) * right + Mathf.Sin(azRad) * up);
            return (forward + Mathf.Tan(coneAngle * Mathf.Deg2Rad) * perp).normalized;
        }

        public void IgnoreCollider(Collider col, bool ignore = true)
        {
            if (ignore) IgnoredColliders.Add(col);
            else IgnoredColliders.Remove(col);
        }

        public void ClearIgnoredColliders()
        {
            IgnoredColliders.Clear();
        }
        
        bool HitObstacle(Vector3 start, Vector3 end, Transform target)
        {
            Vector3 dir = VectorUtils.Direction(start, end);
            start += dir * ObstacleCastOffset;
            
            return ObstacleMask != default 
                   && Physics.Linecast(start, end, out RaycastHit obstacleHit, ObstacleMask) 
                   && (self == null || obstacleHit.transform != self.transform)
                   && obstacleHit.transform != target;
        }
        
        public void DrawGizmos(Vector3 origin, Vector3 forward)
        {
            Gizmos.color = Color.yellow;
            forward.Normalize();

            Vector3 arbitrary = (Mathf.Abs(forward.y) > 0.99f) ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(forward, arbitrary).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;

            int segments = 48;
            Vector3 prevPoint = origin + right * BaseRadius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = (360f / segments) * i;
                Vector3 dir = Mathf.Cos(angle * Mathf.Deg2Rad) * right + Mathf.Sin(angle * Mathf.Deg2Rad) * up;
                Vector3 nextPoint = origin + dir * BaseRadius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            Vector3 tip = origin + forward * MaxRange;
            float tipRadius = Mathf.Tan(FOVRadius * Mathf.Deg2Rad) * MaxRange;
            prevPoint = tip + right * tipRadius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = (360f / segments) * i;
                Vector3 dir = Mathf.Cos(angle * Mathf.Deg2Rad) * right + Mathf.Sin(angle * Mathf.Deg2Rad) * up;
                Vector3 nextPoint = tip + dir * tipRadius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            for (int i = 0; i < segments; i += segments / 8)
            {
                float angle = (360f / segments) * i;
                Vector3 dir = Mathf.Cos(angle * Mathf.Deg2Rad) * right + Mathf.Sin(angle * Mathf.Deg2Rad) * up;
                Gizmos.DrawLine(origin + dir * BaseRadius, tip + dir * tipRadius);
            }
        }
    }
}
