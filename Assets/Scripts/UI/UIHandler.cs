using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public enum StateButton {
    MR,
    VR
} 
public class UIHandler : MonoBehaviour
{
    public Action<int> onStateButtonPressed;
    public Action<int> onVideoButtonPressed;
    public Action<float> onSliderValueChanged;
    
    public void OnStateButtonPressed(int stateIndex)
    {
        onStateButtonPressed?.Invoke(stateIndex);
    }

    public void OnVidButtonPressed(int vidIndex)
    {
        onVideoButtonPressed?.Invoke(vidIndex);   
    }

    public void OnSliderUpdated(float sliderValue)
    {
        onSliderValueChanged?.Invoke(sliderValue);
    }
}
