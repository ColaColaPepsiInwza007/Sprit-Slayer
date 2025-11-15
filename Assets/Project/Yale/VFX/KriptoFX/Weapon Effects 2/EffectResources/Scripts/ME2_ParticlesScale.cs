using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{

    public class ME2_ParticlesScale : MonoBehaviour
    {
        public float Scale           = 1;
        public bool  AffectStartSize = true;

        public ParticleSystem[] ParticleSystems;

        void Awake()
        {
            foreach (var ps in ParticleSystems)
            {
                if(ps == null) continue;
                
                var main  = ps.GetComponent<ParticleSystem>().main;

                if (AffectStartSize)
                {
                    var curve = main.startSize;
                    curve.constantMin *= Scale;
                    curve.constantMax *= Scale;
                    main.startSize    =  curve;
                }
            }
        }

    }
}
