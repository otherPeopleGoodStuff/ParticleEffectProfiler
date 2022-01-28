namespace UnityEngine.Rendering.Universal
{
    public class OverdrawRenderer: ScriptableRenderer
    {
        private OverdrawPass m_OverdrawPass;

        public OverdrawRenderer(ScriptableRendererData data) : base(data)
        {
            m_OverdrawPass = new OverdrawPass();
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            EnqueuePass(m_OverdrawPass);
        }
    }
}