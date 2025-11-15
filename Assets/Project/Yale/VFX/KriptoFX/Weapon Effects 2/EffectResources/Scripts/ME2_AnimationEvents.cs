using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace MeshEffects2
{
    public class ME2_AnimationEvents : MonoBehaviour
    {
        public ParentConstraint ParentConstraint;

        // Start is called before the first frame update
        void Start()
        {

        }

        void OnDisable()
        {
            OnParentConstraintChanged(0);
        }


        public void OnParentConstraintChanged(int state)
        {
            var isEnabled = state > 0;
            UpdateConstrainWeight(0, isEnabled ? 0 : 1);
            UpdateConstrainWeight(1, isEnabled ? 1 : 0);
        }

        void UpdateConstrainWeight(int sourceIndex, float weight)
        {
            var source = ParentConstraint.GetSource(sourceIndex);
            source.weight = weight;
            ParentConstraint.SetSource(sourceIndex, source);
        }
    }
}