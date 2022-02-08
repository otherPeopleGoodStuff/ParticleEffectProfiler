using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public class OverdrawPass : ScriptableRenderPass
    {
        private const string k_ProfilerTag = "Overdraw Pass";
        private readonly ProfilingSampler k_ProfilingSampler = new ProfilingSampler(k_ProfilerTag);
        
        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        private Material m_overdrawMaterial;

        public OverdrawPass()
        {
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            // m_ShaderTagIdList.Add(new ShaderTagId("HRPForward"));

            ValidateMaterials();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_ProfilerTag);
            using (new ProfilingScope(cmd, k_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                ValidateMaterials();

                var drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, SortingCriteria.CommonOpaque);
                drawingSettings.overrideMaterial = m_overdrawMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

                drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, SortingCriteria.CommonTransparent);
                drawingSettings.overrideMaterial = m_overdrawMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;
                filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void ValidateMaterials()
        {
            if (m_overdrawMaterial == null)
            {
                m_overdrawMaterial = new Material(Shader.Find("Hidden/ParticleEffectProfiler/OverDraw"));
            }
        }
    }
}