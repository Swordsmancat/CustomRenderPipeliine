using UnityEngine;
using UnityEngine.Rendering;

public class SetLitRP : MonoBehaviour
{
    public RenderPipelineAsset CurrentPipLineAsset;

    private void OnEnable()
    {
        GraphicsSettings.defaultRenderPipeline = CurrentPipLineAsset;
    }

    private void OnValidate()
    {
        GraphicsSettings.defaultRenderPipeline = CurrentPipLineAsset;
    }
}
