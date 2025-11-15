using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshEffects2
{
    public partial class ME2_GlobalUpdate : MonoBehaviour
    {
        public static GameObject                           Instance;
        public static HashSet<ME2_IScriptInstance>         ScriptInstances     = new HashSet<ME2_IScriptInstance>();
        public static HashSet<ME2_CommandBufferDistortion> DistortionInstances = new HashSet<ME2_CommandBufferDistortion>();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunOnStart()
        {
            Destroy(Instance);
            Instance = null;

            ScriptInstances.Clear();
            DistortionInstances.Clear();
        }

        public static void CreateInstanceIfRequired()
        {
            if (Instance != null) return;

            Instance = new GameObject("ME2_GlobalUpdate") { hideFlags = HideFlags.DontSave };
            Instance.AddComponent<ME2_GlobalUpdate>();
        }


        void Update()
        {
            foreach (var scriptInstances in ScriptInstances)
            {
                scriptInstances.ManualUpdate();
            }
        }

        //builtin distortion rendering command buffer

        private CommandBuffer _cmd;
        private CameraEvent   _cameraEvent = CameraEvent.BeforeForwardAlpha;


        void OnEnable()
        {
            if (QualitySettings.renderPipeline == null)
            {
                Camera.onPreCull    += OnBeforeCameraRendering;
                Camera.onPostRender += OnAfterCameraRendering;
            }
            else
            {
                RenderPipelineManager.beginCameraRendering += OnBeforeCameraRendering;
                RenderPipelineManager.endCameraRendering   += OnAfterCameraRendering;

            }
        }

        void OnDisable()
        {
            if (QualitySettings.renderPipeline == null)
            {
                Camera.onPreCull    -= OnBeforeCameraRendering;
                Camera.onPostRender -= OnAfterCameraRendering;
            }
            else
            {
                RenderPipelineManager.beginCameraRendering -= OnBeforeCameraRendering;
                RenderPipelineManager.endCameraRendering  -= OnAfterCameraRendering;
            }
        }

        private void OnBeforeCameraRendering(Camera cam)
        {
            RenderDistortion(cam);
            UpdateCameraParams(cam);
        }

      
        private void OnAfterCameraRendering(Camera cam)
        {
           ClearDistortion(cam);
        }


        private void OnBeforeCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            RenderDistortion(cam, context);
            UpdateCameraParams(cam, context);
        }

        private void OnAfterCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            ClearDistortion(cam);
        }
    }
}