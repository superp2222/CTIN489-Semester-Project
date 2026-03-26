using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tween 
{

    public enum Mode{Position,Rotation,Scale}
    public enum Easing{EaseInQuadratic, EaseOutQuadratic, EaseInOutQuadratic, EaseInBack, EaseOutBack, EaseInOutBack}
    
    public static TweenHandle Position(Transform target, Vector3 start, Vector3 end, float duration)
    {
        return new TweenHandle()
        {
            target = target,
            duration = duration,
            startValue = start,
            endValue = end,
            tweenType = Mode.Position
        };
    }
    
    public static TweenHandle Scale(Transform target, Vector3 startScale, Vector3 endScale, float duration)
    {
        return new TweenHandle()
        {
            target = target,
            duration = duration,
            startValue = startScale,
            endValue = endScale,
            tweenType = Mode.Scale
        };

    }
    public static TweenHandle Rotate(Transform target, Vector3 startEulerAngles, Vector3 endEulerAngles, float duration)
    {
        return new TweenHandle()
        {
            target = target,
            duration = duration,
            startValue = startEulerAngles,
            endValue = endEulerAngles,
            tweenType = Mode.Rotation
        };

    }
}
