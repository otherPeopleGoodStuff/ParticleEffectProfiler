#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// 给选中的特效添加脚本的
/// </summary>
[InitializeOnLoad]
public static class TestParticleEffect
{
    private const string RequestTestKey = "TestParticleEffectRquestTest";
    private static bool _hasPlayed;
    static bool isRestart = false;

    [MenuItem("GameObject/VFX/Profiling", false, 1)]
    private static void Test()
    {
        var go = Selection.activeGameObject;
        var particleSystemRenderer = go.GetComponentsInChildren<ParticleSystemRenderer>(true);

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

        var particleEffectScript = go.GetComponentsInChildren<HParticleEffectProfilingComponent>(true);
        if (particleEffectScript.Length == 0)
        {
            go.AddComponent<HParticleEffectProfilingComponent>();
        }
    }

    static TestParticleEffect()
    {
        EditorApplication.update += Update;
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
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

    private static void PlaymodeStateChanged()
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
    }
}

#endif