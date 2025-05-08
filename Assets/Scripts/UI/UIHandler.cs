using UnityEngine;

public enum StateButton
{
    MR,
    VR
}
public class UIHandler : MonoBehaviour
{
    public void OnStateButtonPressed(int stateIndex)
        => EventBus.Publish(UIEventType.StateButton, stateIndex);

    public void OnVidButtonPressed(int vidIndex)
        => EventBus.Publish(UIEventType.VideoButton, vidIndex);

    public void OnSliderUpdated(float sliderValue)
        => EventBus.Publish(UIEventType.Slider, sliderValue);
}