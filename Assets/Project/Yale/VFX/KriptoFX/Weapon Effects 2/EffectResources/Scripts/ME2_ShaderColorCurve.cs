using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{
    public class ME2_ShaderColorCurve : MonoBehaviour, ME2_IScriptInstance
    {
        public ME2_ShaderPropertyName ShaderName = ME2_ShaderPropertyName._Color;

        public Gradient ColorOverTime = new Gradient();
        public float    Duration      = 2;
        public bool     Loop          = false;

        private Renderer _rend;
        private float    _startTime;
        private Color    _startColor;
        private bool     _frozen;
        private int      _shaderID;


        public enum ME2_ShaderPropertyName
        {
            _Color,
            _EmissionColor,
        }

        void Awake()
        {
            _rend     = GetComponent<Renderer>();
            _shaderID = Shader.PropertyToID(ShaderName.ToString());
        }

        void OnEnable()
        {
            ME2_GlobalUpdate.CreateInstanceIfRequired();
            ME2_GlobalUpdate.ScriptInstances.Add(this);
            _startTime = Time.time;
            _frozen    = false;

            _startColor = _rend.sharedMaterial.GetColor(_shaderID);
        }


        void OnDisable()
        {
            ME2_GlobalUpdate.ScriptInstances.Remove(this);

            _rend.GetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
            ME2_CoreUtils.SharedMaterialPropertyBlock.SetVector(_shaderID, _startColor);
            _rend.SetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
        }

        public void ManualUpdate()
        {
            if (_frozen) return;

            var leftTime       = Time.time - _startTime;
            if (Loop) leftTime %= Duration;
            var shaderValue    = ColorOverTime.Evaluate(leftTime / Duration) * _startColor;

            _rend.GetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
            ME2_CoreUtils.SharedMaterialPropertyBlock.SetVector(_shaderID, shaderValue);
            _rend.SetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);

            if (!Loop && leftTime > Duration) _frozen = true;
        }

    }
}