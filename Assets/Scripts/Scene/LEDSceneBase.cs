using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEDSceneBase : MonoBehaviour
{
    [SerializeField] protected GameObject sceneObject;

    public void SetObjectState(bool isOn)
    {
        sceneObject.SetActive(isOn);
    }
}
