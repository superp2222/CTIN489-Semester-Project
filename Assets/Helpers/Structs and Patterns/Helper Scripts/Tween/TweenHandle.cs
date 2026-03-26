using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TweenHandle
{
   public Transform target;
   public float duration;
   public Vector3 startValue, endValue;

   public Tween.Mode tweenType;
  public Tween.Easing easingFunc;
   public Action CompleteCallback;

   public TweenHandle SetEase(Tween.Easing aFunc)
   {
       easingFunc = aFunc;
       return this;
   }

   public TweenHandle OnComplete(Action aCallback)
   {
       CompleteCallback = aCallback;
       return this;
   }

   public TweenHandle Play()
   {
       TweenRunner.Play(this);
       return this;
   }
}
