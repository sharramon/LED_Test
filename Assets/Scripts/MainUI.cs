using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainUI : MonoBehaviour
{
    [SerializeField] private TMP_Text zoom;
    [SerializeField] private int zoomMax = 2;
    [SerializeField] private int zoomMin = -2;
    private int _zoomInt = 0;
    
    public Action<int> onVideoButtonPressed;
    public Action<int> onZoomChanged;

    private void Start()
    {
        SetZoomText();
    }

    private int SetZoomText()
    {
        zoom.text = _zoomInt.ToString();
        return _zoomInt;
    }

    public int AddZoomInt(int addValue)
    {
        _zoomInt += addValue;
        if (_zoomInt > zoomMax) _zoomInt = zoomMax;
        if (_zoomInt < zoomMin) _zoomInt = zoomMin;
        
        SetZoomText();
        onZoomChanged?.Invoke(_zoomInt);
        return _zoomInt;
    }

    public void OnOceanButtonPressed()
    {
        onVideoButtonPressed?.Invoke(0);
    }

    public void OnFireButtonPressed()
    {
        onVideoButtonPressed?.Invoke(1);
    }

    public void OnZoomButtonPressed(int zoomValue)
    {
        AddZoomInt(zoomValue);
    }
}
