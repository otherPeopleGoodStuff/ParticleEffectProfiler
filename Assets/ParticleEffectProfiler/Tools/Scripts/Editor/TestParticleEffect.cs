using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 给选中的特效添加脚本的
/// </summary>
[InitializeOnLoad]
public static class TestParticleEffect
{
    private const string RequestTestKey = "TestParticleEffectRquestTest";
    private static bool _hasPlayed;
    static bool isRestart = false;

    private static GameObject m_TargetGameObject;

    [MenuItem("GameObject/VFX/Profiling", false, 1)]
    private static void Test()
    {
        m_TargetGameObject = Selection.activeGameObject;
        var particleSystemRenderer = m_TargetGameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);

        if (particleSystemRenderer.Length == 0)
        {
            Debug.LogError("不是特效无法测试！");
            return;
        }

        EditorPrefs.SetBool(RequestTestKey, true);

        //已经在播放状态，使其重新开始
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            isRestart = true;
        }
        else
        {
            EditorApplication.isPlaying = true;
        }

        var particleProfilingComponents = m_TargetGameObject.GetComponentsInChildren<HParticleProfilingComponent>(true);

        if (particleProfilingComponents == null || particleProfilingComponents.Length == 0)
        {
            m_TargetGameObject.AddComponent<HParticleProfilingComponent>();
        }
    }

    static TestParticleEffect()
    {
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        // EditorApplication.playModeStateChanged += UpdatePlayModeChange;
    }

    private static void Update()
    {
        if (EditorPrefs.HasKey(RequestTestKey) && !_hasPlayed &&
            EditorApplication.isPlaying &&
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorPrefs.DeleteKey(RequestTestKey);
            _hasPlayed = true;
        }
    }

    private static void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (!EditorApplication.isPlaying)
        {
            _hasPlayed = false;
        }

        if (isRestart)
        {
            EditorApplication.isPlaying = true;
            isRestart = false;
        }

        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                Debug.Log("Entered PlayMode");
                SetupOverdrawRenderer();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                Debug.Log("Exiting PlayMode");
                // CleanParticleProfilingComponents();
                ResetRenderer();
                break;
            default:
                break;
        }
    }

    static void SetupOverdrawRenderer()
    {
        // Get overdraw renderer in
        var curRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        if (curRenderPipelineAsset is UniversalRenderPipelineAsset)
        {
            var curCamera = Camera.main;
            var universalAdditionCameraData = curCamera.GetUniversalAdditionalCameraData();
            universalAdditionCameraData.SetRenderer(1);
        }
    }

    static void ResetRenderer()
    {
        // Get overdraw renderer in
        var curRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        if (curRenderPipelineAsset is UniversalRenderPipelineAsset)
        {
            var curCamera = Camera.main;
            var universalAdditionCameraData = curCamera.GetUniversalAdditionalCameraData();
            universalAdditionCameraData.SetRenderer(0);
        }
    }

    static void CleanParticleProfilingComponents()
    {
        var components = Resources.FindObjectsOfTypeAll<HParticleProfilingComponent>();
        if (components == null || components.Length <= 0)
        {
            return;
        }

        var length = components.Length;
        for (int i = 0; i < length; i++)
        {
            GameObject.Destroy(components[i]);
        }
    }
}