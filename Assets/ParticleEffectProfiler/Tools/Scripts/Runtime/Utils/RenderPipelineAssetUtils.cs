using UnityEditor;
using UnityEngine.Rendering.Universal;

public static class RenderPipelineAssetUtils
{
    public static UniversalRenderPipelineAsset GetRenderPipelineAssetAtPath(string relativePath)
    {
        return AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(relativePath);
    }
}
