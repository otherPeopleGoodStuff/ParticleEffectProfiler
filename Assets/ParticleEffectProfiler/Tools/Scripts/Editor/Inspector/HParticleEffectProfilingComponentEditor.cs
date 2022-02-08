using UnityEditor;
using UnityEngine;

/// <summary>
/// 将特效的性能数据显示到Scene
/// </summary>
[CustomEditor(typeof(HParticleProfilingComponent))] 
public class HParticleEffectProfilingComponentEditor : Editor {

    string[] m_Label = new string[20];

    void OnSceneGUI()
    {
        HParticleProfilingComponent mHParticleProfilingComponent = (HParticleProfilingComponent)target;

        int index = 0;
        m_Label[index] = GetParticleEffectData.GetGetRuntimeMemorySizeStr(mHParticleProfilingComponent.gameObject);
        m_Label[++index] = GetParticleEffectData.GetParticleSystemCount(mHParticleProfilingComponent.gameObject);

        if (EditorApplication.isPlaying)
        {
            m_Label[++index] = GetParticleEffectData.GetOnlyParticleEffecDrawCallStr();
            m_Label[++index] = GetParticleEffectData.GetParticleCountStr(mHParticleProfilingComponent);
            m_Label[++index] = GetParticleEffectData.GetPixDrawAverageStr(mHParticleProfilingComponent);
            m_Label[++index] = GetParticleEffectData.GetPixActualDrawAverageStr(mHParticleProfilingComponent);
            m_Label[++index] = GetParticleEffectData.GetPixRateStr(mHParticleProfilingComponent);
        }

        ShowUI(); 
    }

    void ShowUI()
    {
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        GUIStyle style = new GUIStyle
        {
            richText = true,
            fontStyle = FontStyle.Bold
        };

        for (int i = 0; i < m_Label.Length; i++)
		{
            if (!string.IsNullOrEmpty(m_Label[i]))
	        {
		        GUILayout.Label(m_Label[i], style);
	        }
		}

        GUILayout.EndArea();

        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        HParticleProfilingComponent particleProfilingComponent = (HParticleProfilingComponent)target;

        string autoCullingTips = GetParticleEffectData.GetCullingSupportedString(particleProfilingComponent.gameObject);
        if (!string.IsNullOrEmpty(autoCullingTips))
        {
            GUILayout.Label("ParticleSystem 以下选项会导致无法自动剔除：", EditorStyles.whiteLargeLabel);
            GUILayout.Label(autoCullingTips);
        }
    }
}