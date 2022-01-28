// #if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// class for data curve graph drawing
/// </summary>
public static class AnimationCurveUtils 
{
    public const int FPS = 30;

    /// <summary>
    /// Extension static method for AnimationCurve to record values into frames
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="value"></param>
    /// <param name="second"></param>
    public static void UpdateAnimationCurve(this AnimationCurve curve, int value, int second)
    {
        var targetFrameCount = second * FPS;
        
        if (curve.length > targetFrameCount)
        {
            for (int i = curve.length-1; i >= targetFrameCount; i--)
            {
                curve.RemoveKey(i);
            }
        }
        
        if (curve.length < targetFrameCount)
        {
            curve.AddKey(curve.length, value);
        }
    }

}
// #endif