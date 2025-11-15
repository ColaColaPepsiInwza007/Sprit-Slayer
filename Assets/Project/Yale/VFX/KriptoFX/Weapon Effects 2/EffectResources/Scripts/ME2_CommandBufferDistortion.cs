using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshEffects2
{
    [ExecuteAlways]
    public class ME2_CommandBufferDistortion : MonoBehaviour
    {
        void OnEnable()
        {
            ME2_GlobalUpdate.CreateInstanceIfRequired();
            ME2_GlobalUpdate.DistortionInstances.Add(this);

        }

        void OnDisable()
        {
            ME2_GlobalUpdate.DistortionInstances.Remove(this);
        }
    }
}