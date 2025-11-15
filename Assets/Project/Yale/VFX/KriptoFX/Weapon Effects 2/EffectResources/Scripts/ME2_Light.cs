using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{
    public class ME2_Light : MonoBehaviour, ME2_IScriptInstance
    {
        public AnimationCurve IntensityOverTime = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float          Duration          = 2;
        public bool           Loop              = true;

        private Light _light;
        private float _startTime;
        private float _startIntensity;
        private bool  _frozen;

        void Awake()
        {
            _light = GetComponent<Light>();
        }

        void OnEnable()
        {
            ME2_GlobalUpdate.CreateInstanceIfRequired();
            ME2_GlobalUpdate.ScriptInstances.Add(this);
            _startTime      = Time.time;
            _startIntensity = _light.intensity;
            _frozen         = false;

        }

        void OnDisable()
        {
            ME2_GlobalUpdate.ScriptInstances.Remove(this);

            _light.intensity = _startIntensity;
        }

        public void ManualUpdate()
        {
            if (_frozen) return;

            var leftTime       = Time.time - _startTime;
            if (Loop) leftTime %= Duration;
            _light.intensity = IntensityOverTime.Evaluate(leftTime / Duration) * _startIntensity;

            if (!Loop && leftTime > Duration) _frozen = true;
        }

    }
}