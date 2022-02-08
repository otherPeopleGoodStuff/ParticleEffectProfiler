using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace UnityEngine.Rendering.Universal
{
    [Serializable, ReloadGroup, ExcludeFromPreset]
    public class OverdrawRendererData : ScriptableRendererData
    {
#if UNITY_EDITOR
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateHOverdrawRendererAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<OverdrawRendererData>();
                AssetDatabase.CreateAsset(instance, pathName);
                ResourceReloader.ReloadAllNullIn(instance, UniversalRenderPipelineAsset.packagePath);
                Selection.activeObject = instance;
            }
        }

        [MenuItem("Assets/Create/Rendering/Overdraw Renderer", priority = CoreUtils.assetCreateMenuPriority2)]
        static void CreateHOverdrawRendererData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateHOverdrawRendererAsset>(), "OverdrawRenderer.asset", null, null);
        }
#endif

        protected override ScriptableRenderer Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ResourceReloader.TryReloadAllNullIn(this, UniversalRenderPipelineAsset.packagePath);
            }
#endif
            return new OverdrawRenderer(this);
        }
    }
}