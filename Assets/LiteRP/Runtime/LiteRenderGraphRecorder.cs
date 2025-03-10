using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder : IRenderGraphRecorder,IDisposable
    {
        private TextureHandle _BackbufferColorHandle =TextureHandle.nullHandle;
        private RTHandle _targetColorHandle = null;

        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
           var cameraData = frameData.Get<CameraData>();
            CreateRenderGraphCameraRenderTargets(renderGraph, cameraData);
            AddSetupCameraPropertiesPass(renderGraph, cameraData);
            AddDrawObjectsPass(renderGraph, cameraData);
        }

        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, CameraData cameraData)
        {
            RenderTargetIdentifier targetColorId = BuiltinRenderTextureType.CameraTarget;
            if (_targetColorHandle == null)
            {
                _targetColorHandle = RTHandles.Alloc(targetColorId, "BackBuffer color");
             }

            var CameraBackGroundColor =CoreUtils.ConvertSRGBToActiveColorSpace(cameraData.Camera.backgroundColor);

            var importResourceParams = new ImportResourceParams();
            importResourceParams.clearOnFirstUse = true;
            importResourceParams.clearColor =CameraBackGroundColor;
            importResourceParams.discardOnLastUse = false;

            var colorRT_sRGB =(QualitySettings.activeColorSpace ==ColorSpace.Linear);

            RenderTargetInfo importInfoColor = new RenderTargetInfo();
            importInfoColor.width =Screen.width;
            importInfoColor.height =Screen.height;
            importInfoColor.volumeDepth = 1;
            importInfoColor.msaaSamples = 1;
            importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);

            _BackbufferColorHandle = renderGraph.ImportTexture(_targetColorHandle, importInfoColor, importResourceParams);
        }

        public void Dispose()
        {
            RTHandles.Release( _targetColorHandle );
            GC.SuppressFinalize(this);
        }
    }
}