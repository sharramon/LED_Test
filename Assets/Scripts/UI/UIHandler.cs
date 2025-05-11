using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum StateButton
{
    MR,
    VR
}
public class UIHandler : MonoBehaviour
{
    [SerializeField] private SlideOnPoke _onSizeSlider;
    [SerializeField] private SlideOnPoke _onSpeedSlider;
    [SerializeField] private List<GameObject> m_stateUI = new List<GameObject>();

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        _onSizeSlider.onSliderValueChanged += OnSizeSliderUpdated;
        _onSpeedSlider.onSliderValueChanged += OnSpeedSliderUpdated;
    }

    private void UnsubscribeEvents()
    {
        _onSizeSlider.onSliderValueChanged -= OnSizeSliderUpdated;
        _onSpeedSlider.onSliderValueChanged -= OnSpeedSliderUpdated;
    }
    
    //0 : MR
    //1 : VR
    public void OnStateButtonPressed(int stateIndex)
    {
        // early out if called scene is the same
        var newState = (SceneState)stateIndex;
        if (newState == LEDSceneManager.Instance._currentState)
            return;

        // otherwise fire the event and update UI
        EventBus.Publish(UIEventType.StateButton, stateIndex);
        SetStateUI(stateIndex);
    }

    public void OnVidButtonPressed(int vidIndex)
        => EventBus.Publish(UIEventType.VideoButton, vidIndex);

    public void OnSizeSliderUpdated(float sliderValue)
        => EventBus.Publish(UIEventType.SizeSlider, sliderValue);

    private void SetStateUI(int uiIndex)
    {
        if(m_stateUI.Count <= uiIndex)
            return;
        
        foreach (var stateUI in m_stateUI)
        {
            stateUI.SetActive(false);
        }
        
        m_stateUI[uiIndex].SetActive(true);
    }
    
    public void OnSpeedSliderUpdated(float sliderValue)
        => EventBus.Publish(UIEventType.SpeedSlider, sliderValue);
}