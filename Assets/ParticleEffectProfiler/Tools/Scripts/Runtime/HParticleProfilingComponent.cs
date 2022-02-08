using System.Reflection;
using UnityEngine;

/// <summary>
/// Component for particle effect profiling
/// </summary>
public class HParticleProfilingComponent : MonoBehaviour
{
    public AnimationCurve particleCountAnimationCurve = new AnimationCurve();
    public AnimationCurve drawCallAnimationCurve = new AnimationCurve();
    public AnimationCurve overdrawAnimationCurve = new AnimationCurve();
    
    [Range(1,10)]
    public int effectDuration = 3;

    EffectOverdrawEvaluator m_EffectOverdrawEvaluator;
    ParticleSystem[] m_ParticleSystems;
    MethodInfo m_CalculateEffectUIDataMethod;
    int m_ParticleCount = 0;
    int m_MaxParticleCount = 0;

    void Awake()
    {
        Debug.Log("Particle vfx profiling starts.");
        Application.targetFrameRate = AnimationCurveUtils.FPS;
        m_EffectOverdrawEvaluator = new EffectOverdrawEvaluator(Camera.main);
    }

    void Start()
    {
        m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
#if UNITY_2017_1_OR_NEWER
        m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.Instance | BindingFlags.NonPublic);
#else
        m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CountSubEmitterParticles", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
    }
    
    void LateUpdate()
    {
        RecordParticleCount();
        m_EffectOverdrawEvaluator?.Update();

        UpdateParticleCountCurve();
        UpdateDrawCallCurve();
        UpdateOverdrawCurve();
    }

    public EffectEvlaData[] GetEffectEvlaData()
    {
        return m_EffectOverdrawEvaluator?.GetEffectEvlaData();
    }

    public void RecordParticleCount()
    {
        if (m_CalculateEffectUIDataMethod == null)
        {
            return;
        }
        
        m_ParticleCount = 0;
        foreach (var ps in m_ParticleSystems)
        {
            int count = 0;
            if (ps == null)
            {
                continue;
            }
#if UNITY_2017_1_OR_NEWER
            object[] invokeArgs = { count, 0.0f, Mathf.Infinity };
            m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
            count = (int)invokeArgs[0];
#else
            object[] invokeArgs = { count };
            m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
            count = (int)invokeArgs[0];
            count += ps.particleCount;
#endif
            m_ParticleCount += count;
        }
        if (m_MaxParticleCount < m_ParticleCount)
        {
            m_MaxParticleCount = m_ParticleCount;
        }
    }

    public int GetParticleCount()
    {
        return m_ParticleCount;
    }
    public int GetMaxParticleCount()
    {
        return m_MaxParticleCount;
    }

    void UpdateParticleCountCurve()
    {
        particleCountAnimationCurve.UpdateAnimationCurve(m_ParticleCount,  effectDuration);
    }
    void UpdateDrawCallCurve()
    {
        var drawCall = GetParticleEffectData.GetOnlyParticleEffecDrawCall();
        drawCallAnimationCurve.UpdateAnimationCurve(drawCall, effectDuration);
    }

    void UpdateOverdrawCurve()
    {
        EffectEvlaData[] effectEvlaData = this.GetEffectEvlaData();
        overdrawAnimationCurve.UpdateAnimationCurve(effectEvlaData[0].GetPixRate(), effectDuration);
    }
}