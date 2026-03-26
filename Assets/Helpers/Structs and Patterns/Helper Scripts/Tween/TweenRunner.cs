using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenRunner : Singleton<TweenRunner>
{

    public static void Test()
    {
        float[] testValues = new []{0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 0.10f, 1f};
        string str = "testing easing function";
        foreach (float testValue in testValues)
        {
            str += "\n" + testValue + ": " + EaseOutBack(testValue).ToString();
        }

        Debug.Log(str);
    }
    
    public static void Play(TweenHandle handle)
    {
        Instance.BeginPlayRoutine(handle);
    }


    private void BeginPlayRoutine(TweenHandle handle)
    {
        StartCoroutine(PlayRoutine(handle));
    }
    
    IEnumerator PlayRoutine(TweenHandle handle)
    {
        //This switches on the easing function enum, to pick the right easing function
        //Func is a kind of delegate, so we are basically assigning a function to a variable to use later
        Func<float, float> easingFunc = handle.easingFunc switch
        {
            Tween.Easing.EaseInQuadratic => EaseInQuad,
            Tween.Easing.EaseOutQuadratic => EaseOutQuad,
            Tween.Easing.EaseInOutQuadratic => EaseInOutQuad,
            Tween.Easing.EaseInBack => EaseInBack,
            Tween.Easing.EaseOutBack => EaseOutBack,
            Tween.Easing.EaseInOutBack => EaseInOutBack,
            _ => EaseInOutQuad
        };

        Debug.Log(handle.easingFunc);
        //iterate while time <= duration
        float time = 0;
        while (time <= handle.duration)
        {
            //get the current time, then calculate the easing value (based on the easingFunc we picked above)
            time += Time.deltaTime;
            float interpolatedPosition = Mathf.Clamp01( time / handle.duration);
            float eased = easingFunc(interpolatedPosition);
            //depending on the tweening mode, interpolate a property of the target according to the eased value
            if (handle.tweenType == Tween.Mode.Position)
                handle.target.position = Vector3.LerpUnclamped(handle.startValue, handle.endValue, eased);
            else if (handle.tweenType == Tween.Mode.Rotation)
                handle.target.rotation = Quaternion.SlerpUnclamped(Quaternion.Euler(handle.startValue), Quaternion.Euler(handle.endValue), eased);
            else if (handle.tweenType == Tween.Mode.Scale)
                handle.target.localScale = Vector3.LerpUnclamped(handle.startValue, handle.endValue, eased);
            
            //wait for the next update
            yield return null;
        }
        
        //when we're done, snap to the final value just in case.
        if (handle.tweenType == Tween.Mode.Position)
            handle.target.position = handle.endValue;
        if (handle.tweenType == Tween.Mode.Rotation)
            handle.target.eulerAngles = handle.endValue;
        if(handle.tweenType == Tween.Mode.Scale)
            handle.target.localScale = handle.endValue;
        
        //Invoke CompleteCallback. The ? means skip the method call if there's no CompleteCallback assigned (if it is null)
        handle.CompleteCallback?.Invoke();

    }

    //Easing functions - see https://easings.net/ for more of these
    
    static float EaseInQuad(float t)
    {
        return t * t;
    }

    static float EaseOutQuad(float t)
    {
        return 1-(1-t) *(1-t);
    }

    static float EaseInOutQuad(float t)
    {
        return 
            t < 0.5f 
            ? 2 * t * t
            : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }

    static float EaseInBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    static float EaseInOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;

        return x < 0.5f
            ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
            : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        
    }
}
