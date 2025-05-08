using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRScene : MonoBehaviour
{
    [SerializeField] private LEDVideo ledVideo;
    [SerializeField] private GameObject mrScene;

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
        EventBus.Subscribe<float>(UIEventType.Slider, ChangeScreenSize);
        EventBus.Subscribe<int>  (UIEventType.VideoButton, ChangeLEDVideo);
    }

    void UnsubscribeEvents()
    {
        EventBus.Unsubscribe<float>(UIEventType.Slider, ChangeScreenSize);
        EventBus.Unsubscribe<int>  (UIEventType.VideoButton, ChangeLEDVideo);
    }
    
    public void ChangeScreenSize(float size)
    {
        if(ledVideo != null)
            ledVideo.ChangeScreenSizeNormalized(size);
    }

    public void ChangeLEDVideo(int videoIndex)
    {
        if(ledVideo != null && mrScene.activeInHierarchy)
        {
            ledVideo.SetVideoClip(videoIndex);
        }
    }
}
