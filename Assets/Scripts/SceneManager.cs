using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SceneManager : Singleton<SceneManager>
{
    [SerializeField] MainUI mainUI;
    [SerializeField] LEDVideo ledVideo;
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
        mainUI.onVideoButtonPressed += OnVideoButtonPressed;
        mainUI.onZoomChanged += OnSizeButtonPressed;
    }

    private void UnsubscribeFromUIEvents()
    {
        mainUI.onVideoButtonPressed -= OnVideoButtonPressed;
        mainUI.onZoomChanged -= OnSizeButtonPressed;
    }

    public int GetZoomInt()
    {
        if(mainUI != null)
            return mainUI.GetZoom();
        
        return 0; //0 being default
    }

    private void OnVideoButtonPressed(int ind)
    {
        ledVideo.SetVideoClip(ind);
    }

    private void OnSizeButtonPressed(int ind)
    {
        ledVideo.ChangeScreenSize(ind);
    }

    private void OnPictureButtonPressed(int ind)
    {
        ledVideo.SetImage(ind);
    }
}
