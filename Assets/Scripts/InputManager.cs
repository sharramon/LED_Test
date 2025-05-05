using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private GameObject testObject;
    [SerializeField] private Transform LEDScreen;
    [SerializeField] private float speed;
    [SerializeField] private float distance;
    [SerializeField] private float rotationSpeed = 180f;
    public bool isLookAt = true;
    private Coroutine moveLEDScreen;
    private bool isMoving = false;

    private void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.One))
        {
            OnButtonPressed();
        }
    }

    private void OnButtonPressed()
    {
        if (LEDSceneManager.Instance._currentState == SceneState.VR)
            return;
        
        isMoving = !isMoving;
        
        if(moveLEDScreen != null)
            StopCoroutine(moveLEDScreen);
        
        if (isMoving == true)
        {
            moveLEDScreen = StartCoroutine(MoveLEDScreen());
        }
    }

    private IEnumerator MoveLEDScreen()
    {
        while (true)
        {
            Transform rightHand = cameraRig.rightHandAnchor;

            Vector3 forward = rightHand.forward;
            forward.y = 0;

            Vector3 targetPosition = rightHand.position + forward.normalized * distance;
            targetPosition.y = 0;

            LEDScreen.position = Vector3.MoveTowards(LEDScreen.position, targetPosition, speed * Time.deltaTime);
            
            Vector3 directionToHand = rightHand.position - LEDScreen.position;
            if (!isLookAt)
                directionToHand = -directionToHand;

            directionToHand.y = 0;

            if (directionToHand != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToHand);
                LEDScreen.rotation = Quaternion.RotateTowards(
                    LEDScreen.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            if (LEDSceneManager.Instance._currentState == SceneState.VR)
            {
                isMoving = false;
                break;
            }
            
            yield return null;
        }
    }
}
