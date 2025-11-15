using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{
    public class ME2_ShaderFloatCurve : MonoBehaviour, ME2_IScriptInstance
    {
        public ME2_ShaderPropertyNames ShaderName          = ME2_ShaderPropertyNames._Cutout;
        public AnimationCurve          IntensityOverTime   = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float                   IntensityMultiplier = 2;
        public float                   Duration            = 2;
        public bool                    Loop                = true;

        //fu.k unity, _materialPropertyBlock just work randomly, so I use material instances and SRP batcher 
        private static MaterialPropertyBlock _materialPropertyBlock;
        private        Material              _mat;
        private        Renderer              _rend;
        private        float                 _startTime;
        private        float                 _startValue;
        private        bool                  _frozen;
        private        int                   _shaderID;

        public enum ME2_ShaderPropertyNames
        {
            _Cutout,
            _Bump,
            _DistortScale,
            _DistortSpeed,
            _NoiseScale,
            _NoiseStrength,
            _Rotation,
            _Alpha
        }

        void Awake()
        {
            _rend                  = GetComponent<Renderer>();
            _mat                   = _rend.material;
            _shaderID              = Shader.PropertyToID(ShaderName.ToString());
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        void OnEnable()
        {
            ME2_GlobalUpdate.CreateInstanceIfRequired();
            ME2_GlobalUpdate.ScriptInstances.Add(this);
            _startTime = Time.time;
            _frozen    = false;


            _startValue = _rend.sharedMaterial.GetFloat(_shaderID);
        }

        void OnDisable()
        {
            ME2_GlobalUpdate.ScriptInstances.Remove(this);

            _rend.GetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
            ME2_CoreUtils.SharedMaterialPropertyBlock.SetFloat(_shaderID, _startValue);
            _rend.SetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
        }

        public void ManualUpdate()
        {
            if (_frozen) return;

            var leftTime       = Time.time - _startTime;
            if (Loop) leftTime %= Duration;
            var shaderValue    = IntensityOverTime.Evaluate(leftTime / Duration) * IntensityMultiplier * _startValue;

            _rend.GetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);
            ME2_CoreUtils.SharedMaterialPropertyBlock.SetFloat(_shaderID, shaderValue);
            _rend.SetPropertyBlock(ME2_CoreUtils.SharedMaterialPropertyBlock);

            if (!Loop && leftTime > Duration) _frozen = true;
        }

    }
}