// #if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 主要用于计算overdraw像素
/// </summary>
public class EffectOverdrawEvaluator
{
    public Camera _camera;
    SingleEffectEvla singleEffectEvla;

    public float time = 0;
    
    // @TODO: 512x512 can be the best size?
    int rtSizeWidth = 512;
    int rtSizeHeight = 512;
    RenderTexture m_RenderTexture;

    private UniversalRenderPipelineAsset m_CurrentRenderPipelineAsset;

    public EffectOverdrawEvaluator(Camera camera)
    {
        _camera = camera;
        
        m_CurrentRenderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (m_CurrentRenderPipelineAsset == null)
        {
            // @TODO: Does "renderPipelineAsset == null" mean using built-in graphics render pipeline
            _camera.SetReplacementShader(Shader.Find("Hidden/HRP/ParticleEffectProfiler/OverDraw"), "");
        }

        m_RenderTexture = new RenderTexture(rtSizeWidth, rtSizeHeight, 0, RenderTextureFormat.ARGB32);
        singleEffectEvla = new SingleEffectEvla(1);
    }

    public void Update()
    {
        time += Time.deltaTime;
        RecordOverDrawData(singleEffectEvla);
    }

    public EffectEvlaData[] GetEffectEvlaData()
    {
        return singleEffectEvla.GetEffectEvlaDatas();
    }

    #region overdraw

    public void RecordOverDrawData(SingleEffectEvla singleEffectEvla)
    {
        int pixTotal = 0;
        int pixActualDraw = 0;

        GetCameraOverDrawData(out pixTotal, out pixActualDraw);

        // 往数据+1
        singleEffectEvla.UpdateOneData(pixTotal, pixActualDraw);
    }

    public void GetCameraOverDrawData(out int pixTotal, out int pixActualDraw)
    {
        //记录当前激活的渲染纹理
        RenderTexture activeTextrue = RenderTexture.active;

        //渲染指定范围的rt，并记录范围内所有rgb像素值
        _camera.targetTexture = m_RenderTexture;
        _camera.Render();
        RenderTexture.active = m_RenderTexture;
        Texture2D texture = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
        GetOverDrawData(texture, out pixTotal, out pixActualDraw);

        //恢复之前激活的渲染纹理
        RenderTexture.active = activeTextrue;
        Texture2D.DestroyImmediate(texture);
        m_RenderTexture.Release();
        _camera.targetTexture = null;
    }

    public void GetOverDrawData(Texture2D texture, out int pixTotal, out int pixActualDraw)
    {
        var texw = texture.width;
        var texh = texture.height;

        var pixels = texture.GetPixels();

        int index = 0;

        pixTotal = 0;
        pixActualDraw = 0;

        for (var y = 0; y < texh; y++)
        {
            for (var x = 0; x < texw; x++)
            {
                float r = pixels[index].r;
                float g = pixels[index].g;
                float b = pixels[index].b;

                bool isEmptyPix = IsEmptyPix(r, g, b);
                if (!isEmptyPix)
                {
                    pixTotal++;
                }

                int drawThisPixTimes = DrawPixTimes(r, g, b);
                pixActualDraw += drawThisPixTimes;

                index++;
            }
        }
    }

    //计算单像素的绘制次数
    public int DrawPixTimes(float r, float g, float b)
    {
        switch (PlayerSettings.colorSpace)
        {
            case ColorSpace.Gamma:
                return DrawPixTimesForGamma(g);
            case ColorSpace.Linear:
                return DrawPixTimesForLinear(g);
            default:
                return DrawPixTimesForGamma(g);
        }
    }

    //Linear空间下的计算方式
    private int DrawPixTimesForLinear(float g)
    {
        //在OverDraw.Shader中像素每渲染一次，g值就会叠加0.04
        return Convert.ToInt32(g / 0.04);
    }


    //Gamma空间下的计算方式 by Blaze火神
    private int DrawPixTimesForGamma(float g)
    {
        //对g值进行Gamma校正
        float GLo = g / 12.92f;
        float GHi = Mathf.Pow((g + 0.055f) / 1.055f, 2.4f);
        float res = (g <= 0.04045f) ? GLo : GHi;

        return Convert.ToInt32(res / 0.04);
    }

    public bool IsEmptyPix(float r, float g, float b)
    {
        return r == 0 && g == 0 && b == 0;
    }

    #endregion
}