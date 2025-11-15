using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{
    public class ME2_ParticlesShapeGizmo : MonoBehaviour
    {
        public GizmoMeshTypeEnum GizmoMeshType = GizmoMeshTypeEnum.Sphere;
        public Vector3           Offset;

        public enum GizmoMeshTypeEnum
        {
            Sphere,
            Box
        }

        private Transform _t;

        void OnDrawGizmos()
        {
            if (_t == null) _t = transform;
            Gizmos.matrix = _t.localToWorldMatrix;
            switch (GizmoMeshType)
            {
                case GizmoMeshTypeEnum.Sphere:
                    Gizmos.DrawWireSphere(Offset, 1);
                    break;
                case GizmoMeshTypeEnum.Box:
                    Gizmos.DrawWireCube(Offset, Vector3.one);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }
}