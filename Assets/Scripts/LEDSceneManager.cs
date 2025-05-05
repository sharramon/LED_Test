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
    [Header("MR")]
    [SerializeField] MainUI mainUI;
    [SerializeField] LEDVideo ledVideo;
    [SerializeField] private GameObject mrObjects;
    
    [Header("VR")]
    [SerializeField] VRUI VRUI;

    [SerializeField] private GameObject vrObjects;
    
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
        mainUI.onVideoButtonPressed += OnVideoButtonPressed;
        mainUI.onPictureButtonPressed += OnPictureButtonPressed;
        mainUI.onZoomChanged += OnSizeButtonPressed;
        mainUI.onVRButtonPressed += OnVRButtonPressed;
        VRUI.onMRButtonPressed += OnMRButtonPressed;
    }

    private void UnsubscribeFromUIEvents()
    {
        mainUI.onVideoButtonPressed -= OnVideoButtonPressed;
        mainUI.onPictureButtonPressed -= OnPictureButtonPressed;
        mainUI.onZoomChanged -= OnSizeButtonPressed;
        mainUI.onVRButtonPressed -= OnVRButtonPressed;
        VRUI.onMRButtonPressed -= OnMRButtonPressed;
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

    private void OnVRButtonPressed()
    {
        currentState = SceneState.VR;
        
        mrObjects.SetActive(false);
        mainUI.gameObject.SetActive(false);
        vrObjects.SetActive(true);
        VRUI.gameObject.SetActive(true);
    }

    private void OnMRButtonPressed()
    {
        currentState = SceneState.MR;
        
        mrObjects.SetActive(true);
        mainUI.gameObject.SetActive(true);
        vrObjects.SetActive(false);
        VRUI.gameObject.SetActive(false);
    }
}
