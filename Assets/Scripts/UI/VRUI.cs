using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUI : MonoBehaviour
{
    public Action onMRButtonPressed;

    public void OnMRButtonPressed()
    {
        onMRButtonPressed?.Invoke();
    }
}
