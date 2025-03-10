using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public class CameraData : ContextItem
    {
        public Camera Camera;
        public CullingResults CullingResults;

        public override void Reset()
        {
            Camera = null;
            CullingResults = default;
        }

    }
}