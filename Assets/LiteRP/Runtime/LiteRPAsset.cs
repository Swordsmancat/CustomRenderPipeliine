using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    [CreateAssetMenu(menuName = "Lite Rendering Pipeline/Lite Render Pipeline Asset")]
    public class LiteRPAsset : RenderPipelineAsset<LiteRenderPipline>
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipline();
        }
    }
 

}
