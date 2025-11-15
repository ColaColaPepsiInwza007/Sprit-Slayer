using UnityEngine;

namespace MeshEffects2
{
    [ExecuteAlways]
    public class ME2_Decal : MonoBehaviour
    {
        private Transform _t;
        private Renderer  _rend;

        private bool _initialized;

        //I can't use awake initialistion because "domain reloading" and ExecuteAlways doesnt work together
        private void Initialize()
        {
            _t           = transform;
            _rend        = GetComponent<Renderer>();
            _initialized = true;
        }

        void OnEnable()
        {
            if (!_initialized) Initialize();

            _rend.GetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
            ME2_CoreUtils.SharedMaterialPropertyBlock.SetMatrix("ME2_WorldToObject", _t.worldToLocalMatrix);
            _rend.SetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
        }


        private void OnDrawGizmos()
        {
            Gizmos.matrix = _t.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
