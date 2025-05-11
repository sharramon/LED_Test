using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public enum SceneState
{
    MR,
    VR
}
public class LEDSceneManager : Singleton<LEDSceneManager>
{
    [Header("Scenes")]
    [SerializeField] private List<LEDSceneBase> scenes = new List<LEDSceneBase>();
    
    private SceneState currentState = SceneState.MR;
    public SceneState _currentState { get => currentState; }
    private void OnEnable()
    {
        SubscribeToUIEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromUIEvents();
    }

    private void SubscribeToUIEvents()
    {
        EventBus.Subscribe<int>(UIEventType.StateButton, OnStateButtonPressed);
    }

    private void UnsubscribeFromUIEvents()
    {
        EventBus.Unsubscribe<int>(UIEventType.StateButton, OnStateButtonPressed);
    }
    
    private void OnStateButtonPressed(int stateInd)
    {
        currentState = (SceneState)stateInd;
        TurnAllStatesOff();
        
        Debug.Log($"Turning scene state {currentState} with index {stateInd}");
        scenes[stateInd].SetObjectState(true);
    }

    private void TurnAllStatesOff()
    {
        foreach (var scene in scenes)
        {
            scene.SetObjectState(false);
        }
    }
}
