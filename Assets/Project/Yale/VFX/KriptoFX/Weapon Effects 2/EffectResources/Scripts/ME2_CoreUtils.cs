using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshEffects2
{
    public static class ME2_CoreUtils
    {
        public static MaterialPropertyBlock SharedMaterialPropertyBlock = new MaterialPropertyBlock();

        //public static void SetFloatPropertyBlock(this Renderer rend, int shaderNameID, float value)
        //{
        //    rend.GetPropertyBlock(_materialPropertyBlock);
        //    _materialPropertyBlock.SetFloat(shaderNameID, value);
        //    rend.SetPropertyBlock(_materialPropertyBlock);
        //}

        //public static void SetColorPropertyBlock(this Renderer rend, int shaderNameID, Color value)
        //{
        //    rend.GetPropertyBlock(_materialPropertyBlock);
        //    _materialPropertyBlock.SetVector(shaderNameID, value); //set color doesnt work with hdr, wtf?
        //    rend.SetPropertyBlock(_materialPropertyBlock);
        //}

        //public static void SetMatrixPropertyBlock(this Renderer rend, int shaderNameID, Matrix4x4 value)
        //{
        //    rend.GetPropertyBlock(_materialPropertyBlock);
        //    _materialPropertyBlock.SetMatrix(shaderNameID, value); 
        //    rend.SetPropertyBlock(_materialPropertyBlock);
        //}
    }
}