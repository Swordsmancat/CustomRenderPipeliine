using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler _DrawObjectsProfilingSampler = new ProfilingSampler("Draw Objects");
        private static readonly ShaderTagId _shaderTagId = new ShaderTagId("SRPDefaultUnlit");//渲染标签ID

        internal class DrawObjectsPassData
        {
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transparentRendererListHandle;
        }

        private void AddDrawObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass", out var passdata, _DrawObjectsProfilingSampler))
            {
                //声明创建或引用的资源
                //创建不透明对象渲染列表
                var opaqueRendererDesc = new RendererListDesc(_shaderTagId, cameraData.CullingResults, cameraData.Camera);
                opaqueRendererDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererDesc.renderQueueRange = RenderQueueRange.opaque;
                passdata.opaqueRendererListHandle = renderGraph.CreateRendererList(opaqueRendererDesc);
                //RenderGraph引入不透明渲染列表
                builder.UseRendererList(passdata.opaqueRendererListHandle);

                //创建半透明对象渲染列表
                var transparentRendererDesc = new RendererListDesc(_shaderTagId, cameraData.CullingResults, cameraData.Camera);
                transparentRendererDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererDesc.renderQueueRange = RenderQueueRange.transparent;
                passdata.transparentRendererListHandle = renderGraph.CreateRendererList(transparentRendererDesc);
                //RenderGraph引入不透明渲染列表
                builder.UseRendererList(passdata.transparentRendererListHandle);

                //导入BackBuffer
                builder.SetRenderAttachment(_BackbufferColorHandle, 0, AccessFlags.Write);

                //设置渲染全局状态
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context) =>
                {
                    //调用渲染指令绘制物体
                    context.cmd.DrawRendererList(passData.opaqueRendererListHandle);
                    context.cmd.DrawRendererList(passData.transparentRendererListHandle);
                });
            }
        }

    }
}