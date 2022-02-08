// #if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 处理特效整体相关的数据
/// </summary>
public class GetParticleEffectData
{

    static int m_MaxDrawCall = 0;

    public static int GetRuntimeMemorySize(GameObject go, out int textureCount)
    {
        var textures = new List<Texture>();
        textureCount = 0;
        int sumSize = 0;

        var meshRendererlist = go.GetComponentsInChildren<ParticleSystemRenderer>(true);

        foreach (ParticleSystemRenderer item in meshRendererlist)
        {
            if (item.sharedMaterial)
            {
                Texture texture = item.sharedMaterial.mainTexture;
                if (texture && !textures.Contains(texture))
                {
                    textures.Add(texture);
                    textureCount++;
                    sumSize = sumSize + GetStorageMemorySize(texture);
                }
            }
        }
        return sumSize;
    }

    private static int GetStorageMemorySize(Texture texture)
    {
        return (int)InvokeInternalAPI("UnityEditor.TextureUtil", "GetStorageMemorySize", texture);
    }

    private static object InvokeInternalAPI(string type, string method, params object[] parameters)
    {
        var assembly = typeof(AssetDatabase).Assembly;
        var custom = assembly.GetType(type);
        var methodInfo = custom.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
        return methodInfo != null ? methodInfo.Invoke(null, parameters) : 0;
    }

    public static string GetGetRuntimeMemorySizeStr(GameObject go)
    {
        int maxTextureCount = 5;
        int maxMemorySize = 1000 * 1024;
        int textureCount;
        int memorySize = GetRuntimeMemorySize(go, out textureCount);
        string memorySizeStr = EditorUtility.FormatBytes(memorySize);
        string maxMemorySizeStr = EditorUtility.FormatBytes(maxMemorySize);

        if (maxMemorySize > memorySize)
            memorySizeStr = string.Format("<color=green>{0}</color>", memorySizeStr);
        else
            memorySizeStr = string.Format("<color=red>{0}</color>", memorySizeStr);

        return
            $"贴图所占用的内存：{memorySizeStr}   建议：<{maxMemorySizeStr}\n贴图数量：{FormatColorMax(textureCount, maxTextureCount)}     建议：<{maxTextureCount}";
    }

    public static string GetParticleSystemCount(GameObject go)
    {
        int max = 5;
        var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        return $"特效中所有粒子系统组件数量：{FormatColorMax(particleSystems.Length, max)}     建议：<{max}";
    }

    public static int GetOnlyParticleEffecDrawCall()
    {
        //因为Camera 实际上渲染了两次，一次用作取样，一次用作显示。 狂飙这里给出了详细的说明：https://networm.me/2019/07/28/unity-particle-effect-profiler/#drawcall-%E6%95%B0%E5%80%BC%E4%B8%BA%E4%BB%80%E4%B9%88%E6%AF%94%E5%AE%9E%E9%99%85%E5%A4%A7-2-%E5%80%8D
        int drawCall = UnityStats.batches / 2;
        if (m_MaxDrawCall<drawCall)
        {
            m_MaxDrawCall = drawCall;
        }
        return drawCall;
    }

    public static string GetOnlyParticleEffecDrawCallStr()
    {
        int max = 10;
        return
            $"DrawCall: {FormatColorMax(GetOnlyParticleEffecDrawCall(), max)}   最高：{FormatColorMax(m_MaxDrawCall, max)}   建议：<{max}";
    }

    public static string GetPixDrawAverageStr(HParticleProfilingComponent hParticleGo)
    {
        //index = 0：默认按高品质的算，这里你可以根本你们项目的品质进行修改。
        EffectEvlaData[] effectEvlaData = hParticleGo.GetEffectEvlaData();
        int pixDrawAverage = effectEvlaData[0].GetPixDrawAverage();
        return $"特效原填充像素点：{FormatColorValue(pixDrawAverage)}";
    }

    public static string GetPixActualDrawAverageStr(HParticleProfilingComponent hParticleGo)
    {
        EffectEvlaData[] effectEvlaData = hParticleGo.GetEffectEvlaData();
        int pixActualDrawAverage = effectEvlaData[0].GetPixActualDrawAverage();
        return $"特效实际填充像素点：{FormatColorValue(pixActualDrawAverage)}";
    }

    public static string GetPixRateStr(HParticleProfilingComponent hParticleGo)
    {
        int max = 4;
        EffectEvlaData[] effectEvlaData = hParticleGo.GetEffectEvlaData();
        int pixRate = effectEvlaData[0].GetPixRate();
        return $"平均每像素overdraw率：{FormatColorMax(pixRate, max)}   建议：<{max}";
    }

    public static string GetParticleCountStr(HParticleProfilingComponent hParticleGo)
    {
        int max = 50;
        return
            $"粒子数量：{FormatColorMax(hParticleGo.GetParticleCount(), max)}   最高：{FormatColorMax(hParticleGo.GetMaxParticleCount(), max)}   建议：<{max}";
    }

    public static string GetCullingSupportedString(GameObject go)
    {
        var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        string text = "";
        foreach (ParticleSystem item in particleSystems)
        {
            string str = CheckCulling(item);
            if (!string.IsNullOrEmpty(str))
            {
                text += item.gameObject.name + ":" + str + "\n\n";
            }
        }
        return text;
    }

    static string CheckCulling(ParticleSystem particleSystem)
    {
        string text = "";
        if (particleSystem.collision.enabled)
        {
            text += "\n勾选了 Collision";
        }

        if (particleSystem.emission.enabled)
        {
            if (particleSystem.emission.rateOverDistance.curveMultiplier != 0)
            {
                text += "\nEmission使用了Current(非线性运算)";
            }
        }

        if (particleSystem.externalForces.enabled)
        {
            text += "\n勾选了 External Forces";
        }

        if (particleSystem.forceOverLifetime.enabled)
        {
            if (GetIsRandomized(particleSystem.forceOverLifetime.x)
                || GetIsRandomized(particleSystem.forceOverLifetime.y)
                || GetIsRandomized(particleSystem.forceOverLifetime.z)
                || particleSystem.forceOverLifetime.randomized)
            {
                text += "\nForce Over Lifetime使用了Current(非线性运算)";
            }
        } 
        if (particleSystem.inheritVelocity.enabled)
        {
            if (GetIsRandomized(particleSystem.inheritVelocity.curve))
            {
                text += "\nInherit Velocity使用了Current(非线性运算)";
            }
        } 
        if (particleSystem.noise.enabled)
        {
            text += "\n勾选了 Noise";
        } 
        if (particleSystem.rotationBySpeed.enabled)
        {
            text += "\n勾选了 Rotation By Speed";
        }
        if (particleSystem.rotationOverLifetime.enabled)
        {
            if (GetIsRandomized(particleSystem.rotationOverLifetime.x)
                || GetIsRandomized(particleSystem.rotationOverLifetime.y)
                || GetIsRandomized(particleSystem.rotationOverLifetime.z))
            {
                text += "\nRotation Over Lifetime使用了Current(非线性运算)";
            }
        } 
        if (particleSystem.shape.enabled)
        {
            var shapeType = particleSystem.shape.shapeType;
            switch (shapeType)
            {
                case ParticleSystemShapeType.Cone:
                case ParticleSystemShapeType.ConeVolume:
#if UNITY_2017_1_OR_NEWER
                case ParticleSystemShapeType.Donut:
#endif
                case ParticleSystemShapeType.Circle:
                    if(particleSystem.shape.arcMode != ParticleSystemShapeMultiModeValue.Random)
                    {
                        text += "\nShape的Circle-Arc使用了Random模式";
                    }
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    if (particleSystem.shape.radiusMode != ParticleSystemShapeMultiModeValue.Random)
                    {
                        text += "\nShape的Edge-Radius使用了Random模式";
                    }
                    break;
                default:
                    break;
            }
        } 
        if (particleSystem.subEmitters.enabled)
        {
            text += "\n勾选了 SubEmitters";
        } 
        if (particleSystem.trails.enabled)
        {
            text += "\n勾选了 Trails";
        } 
        if (particleSystem.trigger.enabled)
        {
            text += "\n勾选了 Trigger";
        }
        if (particleSystem.velocityOverLifetime.enabled)
        {
            if (GetIsRandomized(particleSystem.velocityOverLifetime.x)
                || GetIsRandomized(particleSystem.velocityOverLifetime.y)
                || GetIsRandomized(particleSystem.velocityOverLifetime.z))
            {
                text += "\nVelocity Over Lifetime使用了Current(非线性运算)";
            }
        }
        if (particleSystem.limitVelocityOverLifetime.enabled)
        {
            text += "\n勾选了 Limit Velocity Over Lifetime";
        }
        if (particleSystem.main.simulationSpace != ParticleSystemSimulationSpace.Local)
        {
            text += "\nSimulationSpace 不等于 Local";
        }
        if (particleSystem.main.gravityModifierMultiplier != 0)
        {
            text += "\nGravityModifier 不等于0";
        }
        return text;
    }

    static bool GetIsRandomized(ParticleSystem.MinMaxCurve minMaxCurve)
    {
        bool flag = AnimationCurveSupportsProcedural(minMaxCurve.curveMax);

        bool result;
        if (minMaxCurve.mode != ParticleSystemCurveMode.TwoCurves && minMaxCurve.mode != ParticleSystemCurveMode.TwoConstants)
        {
            result = flag;
        }
        else
        {
            bool flag2 = AnimationCurveSupportsProcedural(minMaxCurve.curveMin);
            result = (flag && flag2);
        }

        return result;
    }

    static bool AnimationCurveSupportsProcedural(AnimationCurve curve)
    {
        //switch (AnimationUtility.IsValidPolynomialCurve(curve)) //保护级别，无法访问，靠
        //{
        //    case AnimationUtility.PolynomialValid.Valid:
        //        return true;
        //    case AnimationUtility.PolynomialValid.InvalidPreWrapMode:
        //        break;
        //    case AnimationUtility.PolynomialValid.InvalidPostWrapMode:
        //        break;
        //    case AnimationUtility.PolynomialValid.TooManySegments:
        //        break;
        //}
        return false; //只能默认返回false了
    }

    static string FormatColorValue(int value)
    {
        return $"<color=green>{value}</color>";
    }

    static string FormatColorMax(int value, int max)
    {
        if (max > value)
            return $"<color=green>{value}</color>";
        else
            return $"<color=red>{value}</color>";
    }
}
// #endif