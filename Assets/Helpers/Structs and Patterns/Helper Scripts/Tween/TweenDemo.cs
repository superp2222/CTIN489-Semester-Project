using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenDemo : MonoBehaviour
{
    public Transform tweenObject;

    public Transform start;
    public Transform end;

    public float duration = 0.2f;
    
    CanvasGroup canvasGroup;
    
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    //these helper methods let me map different kinds of easing to different UI buttons.
    public void TweenInQuad() => TweenPosition(Tween.Easing.EaseInQuadratic);
    public void TweenOutQuad() => TweenPosition(Tween.Easing.EaseOutQuadratic);
    public void TweenInOutQuad() => TweenPosition(Tween.Easing.EaseInOutQuadratic);
    public void TweenInBack() => TweenPosition(Tween.Easing.EaseInBack);
    public void TweenOutBack() => TweenPosition(Tween.Easing.EaseOutBack);
    public void TweenInOutBack() => TweenPosition(Tween.Easing.EaseInOutBack);

    public void TweenPosition(Tween.Easing easing)
    {
        canvasGroup.interactable = false;
        
        //tween.position sets up a tween of the position (there are also methods for rotation and scale
        
        Tween.Position(tweenObject,start.position,end.position,duration)
            .SetEase(easing)                                    //optionally set the easing type with SetEase
            .OnComplete(()=>canvasGroup.interactable = true)    //optionally add a callback with OnComplete
            .Play();                                            //finish by playing the tween!
        
        //here I make the canvasgroup not interactable when the tween starts,
        //then use OnComplete to make it interactable again
        //preventing the user from spamming the buttons
        
    }
    
    
}
