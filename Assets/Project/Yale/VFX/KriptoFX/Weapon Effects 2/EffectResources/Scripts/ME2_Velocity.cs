using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{
    public class ME2_Velocity : MonoBehaviour, ME2_IScriptInstance
    {
        internal float             Velocity;
        public   float             VelocityMultiplier = 300;
        public   float             Decay              = 0.25f;
  
        private  Vector3           _lastPosition;

        private Transform             _t;
        private Material              _material;
        private ParticleSystem        _ps;
        private Renderer[]            _renderers;
        private MaterialPropertyBlock _propertyBlock;



        private const float Threshold = 0.000000001f;

        void OnEnable()
        {
            ME2_GlobalUpdate.CreateInstanceIfRequired();
            ME2_GlobalUpdate.ScriptInstances.Add(this);
            _t            = transform;
            _lastPosition = _t.position;

            _renderers     = GetComponentsInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        void OnDisable()
        {
            ME2_GlobalUpdate.ScriptInstances.Remove(this);
        }

        public void ManualUpdate()
        {
            var currentDistance = (_lastPosition - _t.position).sqrMagnitude * VelocityMultiplier;

            _lastPosition =  _t.position;
            Velocity      += 1 - Mathf.Exp(-currentDistance);
            Velocity      =  Mathf.Clamp(Velocity - (Decay * Time.deltaTime), Threshold, 1.0f);

            _propertyBlock.SetFloat("_Velocity", Velocity);
            foreach (var rend in _renderers)
            {
                rend.SetPropertyBlock(_propertyBlock);
            }

        }

    }
}