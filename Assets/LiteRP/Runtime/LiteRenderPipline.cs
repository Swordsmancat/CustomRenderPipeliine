using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipline : RenderPipeline
    {

        private readonly static ShaderTagId _shaderTagId = new ShaderTagId("SRPDefaultUnlit");

        /// <summary>
        /// 渲染图
        /// </summary>
        private RenderGraph _renderGraph = null;

        /// <summary>
        /// 渲染图记录器
        /// </summary>
        private LiteRenderGraphRecorder _liteRenderGraphRecorder = null;

        /// <summary>
        /// 上下文容器
        /// </summary>
        private ContextContainer _contextContainer = null;

        public LiteRenderPipline()
        {
            InitializeRenderGraph();
        }

        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化渲染图
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void InitializeRenderGraph()
        {
            RTHandles.Initialize(Screen.width, Screen.height);
            _renderGraph = new RenderGraph("LiteRPRenderGraph");
            _liteRenderGraphRecorder = new LiteRenderGraphRecorder();
            _contextContainer = new ContextContainer();
        }

        /// <summary>
        /// 清除渲染图
        /// </summary>
        private void CleanupRenderGraph()
        {
            _liteRenderGraphRecorder?.Dispose();
            _liteRenderGraphRecorder = null;
            _contextContainer?.Dispose();
            _contextContainer = null;
            _liteRenderGraphRecorder =null;
            _renderGraph?.Cleanup();
            _renderGraph = null;
        }

        /// <summary>
        /// 老版本
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cameras"></param>
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {

        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            //开始渲染上下文
            BeginContextRendering(context, cameras);


            //渲染相机
            for (int i = 0; i < cameras.Count; i++)
            {
                var camera = cameras[i];
                RenderCamera(context, camera);
            }

            //结束渲染图
            _renderGraph.EndFrame();

            //结束渲染上下文
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            //开始渲染相机
            BeginCameraRendering(context, camera);

            //准备FrameData
            if (!PrepareFrameData(context, camera))
            {
                return;
            }

            //为相机创建CommbandBuffer
            var cmd = CommandBufferPool.Get(camera.name);

            //记录并执行渲染图
            RecordAndExecuteRenderGraph(context,camera, cmd);

            //提交命令缓冲区
            context.ExecuteCommandBuffer(cmd);

            //释放命令缓冲区
            cmd.Clear();
            CommandBufferPool.Release(cmd);

            //提交渲染上下文
            context.Submit();

            //结束渲染相机
            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {

            //获取相机剔除参数，并进行剔除
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
            {
                return false;
            }
            var cullingResults = context.Cull(ref cullingParameters);
            var cameraData = _contextContainer.GetOrCreate<CameraData>();
            cameraData.Camera =camera;
            cameraData.CullingResults =cullingResults;
            return true;
        }

        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera cameras, CommandBuffer cmd)
        {
            var renderGraphParameters = new RenderGraphParameters()
            {
                executionName = cameras.name,
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount,
            };
            _renderGraph.BeginRecording(renderGraphParameters);

            //开启记录线
            _liteRenderGraphRecorder.RecordRenderGraph(_renderGraph, _contextContainer);

            _renderGraph.EndRecordingAndExecute();
        }
    }
}