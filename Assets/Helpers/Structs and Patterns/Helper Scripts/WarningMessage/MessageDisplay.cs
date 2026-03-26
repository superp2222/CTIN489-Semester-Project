using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
/// <summary>
/// This class allows you to display a simple message to players on screen using a static method.
/// It is also an example of how to use the provided state machine class (although the state machine is really overkill for this use case!)
/// </summary>
public class MessageDisplay : MonoBehaviour
{
    //
    private static MessageDisplay _instance;
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private Text text;
    [SerializeField] private CanvasGroup canvasGroup;
    private StateMachine<MessageDisplay> _stateMachine;
    private State<MessageDisplay> _showState = new ShowState();
    private State<MessageDisplay> _hideState = new HideState();
    private Vector3 _startPosition;
    private Vector3 _hiddenPosition => _startPosition + Vector3.down * 200f;
    
    public bool playSound = true;
    private const string messageSound = "cancel";
    
    public static void ShowMessage(string message)
    {
        if (_instance == null)
        {
            Debug.LogWarning("Cannot show message because there is no instance of MessageDisplay in the scene");
            return;
        }
        _instance.BeginShowingMessage(message);
    }

    private float _displayTimer = 0f;

    private void Awake()
    {
        _instance = this;
        TweenRunner.Test();
        _startPosition = text.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup.alpha = 0f;
        _stateMachine = new(this);
        _stateMachine.ChangeState(_hideState);
    }

    void BeginShowingMessage(string message)
    {
        if (text.text != message || _stateMachine.currentState != _showState)
        {
            text.text = message;
            _stateMachine.ChangeState(_showState);
        }

        if(playSound)
            AudioEffectManager.PlayAudio(messageSound);
        _displayTimer = messageDuration;
    }

    
    // Update is called once per frame
    void Update()
    {
        _stateMachine.Update();
    }

    private class HideState : State<MessageDisplay>
    {
        public override void Enter()
        {
            Tween.Position(owner.text.transform,owner._startPosition, owner._hiddenPosition, 0.5f)
                .SetEase(Tween.Easing.EaseOutBack).Play();
        }

        public override void Execute()
        {
            owner.canvasGroup.alpha = Mathf.MoveTowards(owner.canvasGroup.alpha, 0f, Time.deltaTime * 10);
        }
        
        public override void Exit()=>Debug.Log("exiting hide state!");
        
    }
    
    private class ShowState : State<MessageDisplay>
    {
        public override void Enter()
        {
            owner._displayTimer = owner.messageDuration;
            owner.canvasGroup.alpha = 1f;
            Tween.Position(owner.text.transform, owner._hiddenPosition, owner._startPosition, 0.5f)
                .SetEase(Tween.Easing.EaseOutBack).Play();
            
        }
        

        public override void Execute()
        {
            owner._displayTimer -= Time.deltaTime;
            if(owner._displayTimer <=0) owner._stateMachine.ChangeState(owner._hideState);
        }
        
        public override void Exit()=>Debug.Log("exiting show state!");
        
    }
    
}

