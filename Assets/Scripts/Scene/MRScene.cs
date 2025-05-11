using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRScene : LEDSceneBase
{
    [SerializeField] private LEDVideo ledVideo;

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
        EventBus.Subscribe<float>(UIEventType.SizeSlider, ChangeScreenSize);
        EventBus.Subscribe<int>  (UIEventType.VideoButton, ChangeLEDVideo);
    }

    void UnsubscribeEvents()
    {
        EventBus.Unsubscribe<float>(UIEventType.SizeSlider, ChangeScreenSize);
        EventBus.Unsubscribe<int>  (UIEventType.VideoButton, ChangeLEDVideo);
    }
    
    public void ChangeScreenSize(float size)
    {
        if(ledVideo != null)
            ledVideo.ChangeScreenSizeNormalized(size);
    }

    public void ChangeLEDVideo(int videoIndex)
    {
        if(LEDSceneManager.Instance._currentState != SceneState.MR)
            return;
        
        if(ledVideo != null)
        {
            ledVideo.SetVideoClip(videoIndex);
        }
    }
}
