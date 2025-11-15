using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshEffects2
{
    public partial class ME2_GlobalUpdate : MonoBehaviour
    {
     
        void RenderDistortion(Camera cam, ScriptableRenderContext context = default)
        {
            if (DistortionInstances.Count == 0) return;

            if (_cmd == null)
            {
                _cmd = new CommandBuffer() { name = "MeshEffects_CameraDistortionRendering" };
                _cmd.Clear();

                int screenCopyID = Shader.PropertyToID("_CameraOpaqueTextureRT");
                _cmd.GetTemporaryRT(screenCopyID, Mathf.Min(1920, Screen.width), Mathf.Min(1080, Screen.height), 0, FilterMode.Bilinear);
                _cmd.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);
                _cmd.SetGlobalTexture("_CameraOpaqueTexture", screenCopyID);
            }

            cam.AddCommandBuffer(_cameraEvent, _cmd);
        }

        void ClearDistortion(Camera cam)
        {
            if (DistortionInstances.Count == 0) return;

            if (_cmd != null)
            {
                cam.RemoveCommandBuffer(_cameraEvent, _cmd);
            }
        }

        void UpdateCameraParams(Camera cam, ScriptableRenderContext context = default)
        {
            var cameraProjectionMatrix = cam.projectionMatrix;
            var matrix_V               = GL.GetGPUProjectionMatrix(cameraProjectionMatrix, true);
            var maitix_P               = cam.worldToCameraMatrix;
            var currentVPMatrix        = matrix_V * maitix_P;
            Shader.SetGlobalMatrix("ME2_MATRIX_I_VP", currentVPMatrix.inverse);

            cam.depthTextureMode |= DepthTextureMode.Depth;
        }
    }
}