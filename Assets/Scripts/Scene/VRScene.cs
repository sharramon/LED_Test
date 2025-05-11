using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRScene : LEDSceneBase
{
    [SerializeField] private List<LEDVideo> videos = new List<LEDVideo>();
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private float baseMoveSpeed = 1f;
    private float moveSpeed;

    private void Start()
    {
        moveSpeed = baseMoveSpeed * 0.5f;
    }

    // internal state
    private bool _isPointing = false;
    
    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    
    void SubscribeEvents()
    {
        EventBus.Subscribe<int>  (UIEventType.VideoButton, ChangeLEDVideo);
        EventBus.Subscribe<float>(UIEventType.SpeedSlider, ChangeMoveSpeed);
    }

    void UnsubscribeEvents()
    {
        EventBus.Unsubscribe<int>  (UIEventType.VideoButton, ChangeLEDVideo);
        EventBus.Unsubscribe<float>(UIEventType.SpeedSlider, ChangeMoveSpeed);
    }
    
    void Update()
    {
        if (_isPointing)
            MoveForward();
    }
    
    public void ChangeLEDVideo(int videoIndex)
    {
        if(LEDSceneManager.Instance._currentState != SceneState.VR)
            return;
        
        if(videoIndex < 0 || videoIndex >= videos.Count)
            return;
        
        foreach (var video in videos)
        {
            if (video != null)
            {
                video.SetVideoClip(videoIndex);
            }
        }
    }
    
    public void OnPointStart()
    {
        _isPointing = true;
    }

    public void OnPointEnd()
    {
        _isPointing = false;
    }
    
    private void MoveForward()
    {
        // stop if UI is up or in MR mode
        if (gameUI.activeSelf || LEDSceneManager.Instance._currentState == SceneState.MR)
            return;

        // take the hand’s forward, zero out y, invert and normalize
        Vector3 dir = -rightHandAnchor.forward;
        dir.y = 0;
        dir.Normalize();

        // move the VRScene
        sceneObject.transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void ChangeMoveSpeed(float speedModifier)
    {
        moveSpeed = speedModifier * baseMoveSpeed;
    }
}
