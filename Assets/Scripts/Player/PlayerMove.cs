using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private GameObject VRScene;
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private float moveSpeed = 1f;

    // internal state
    private bool _isPointing = false;

    public void OnPointStart()
    {
        _isPointing = true;
    }

    public void OnPointEnd()
    {
        _isPointing = false;
    }

    void Update()
    {
        if (_isPointing)
            MoveForward();
    }

    private void MoveForward()
    {
        // stop if UI is up or in MR mode
        if (gameUI.activeSelf || LEDSceneManager.Instance._currentState == SceneState.MR)
            return;

        // take the hand’s forward, zero out y, invert and normalize
        Vector3 dir = -rightHandAnchor.forward;
        dir.y = 0;
        dir.Normalize();

        // move the VRScene
        VRScene.transform.position += dir * moveSpeed * Time.deltaTime;
    }
}