using System.Collections;
using UnityEngine;
using Oculus.Interaction; // for OVRMicrogestureEventSource, OVRHand

public class HandUI : MonoBehaviour
{
    [SerializeField] private GameObject m_UIObject;
    [SerializeField] private UIHandler uiHandler;
    
    [Header("UI Rotation")]
    [SerializeField] private float                    m_rotateDegrees = 120f;
    [SerializeField] private float                    m_rotateTime    = 0.5f;
    [SerializeField] private OVRMicrogestureEventSource m_gestureSource;

    // Coroutine handle for the current rotation (or null if none)
    private Coroutine _rotateCoroutine;

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
        // Pass the hand enum along with the recognized gesture
        m_gestureSource.GestureRecognizedEvent.AddListener(
            gesture => OnGestureRecognized(OVRPlugin.Hand.HandLeft, gesture)
        );
    }

    private void UnsubscribeEvents()
    {
        m_gestureSource.GestureRecognizedEvent.RemoveListener(
            gesture => OnGestureRecognized(OVRPlugin.Hand.HandLeft, gesture)
        );
    }

    private void OnGestureRecognized(OVRPlugin.Hand hand, OVRHand.MicrogestureType gesture)
    {
        switch (gesture)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                RotateMenu(false);
                break;
            case OVRHand.MicrogestureType.SwipeRight:
                RotateMenu(true);
                break;
            case OVRHand.MicrogestureType.ThumbTap:
                ToggleMenu();
                break;
        }
    }

    private void RotateMenu(bool isRight)
    {
        // Guard against null/inactive or already‑spinning
        if (m_UIObject == null || !m_UIObject.activeSelf || _rotateCoroutine != null)
            return;

        _rotateCoroutine = StartCoroutine(RotateCoroutine(isRight));
    }

    private IEnumerator RotateCoroutine(bool isRight)
    {
        Transform t = m_UIObject.transform;
        float elapsed = 0f;

        // Record starting rotation
        Quaternion startRot = t.localRotation;
        // Compute desired end rotation around the local X-axis
        Quaternion endRot = startRot * Quaternion.Euler(
            isRight ? -m_rotateDegrees : m_rotateDegrees,
            0f,
            0f
        );

        while (elapsed < m_rotateTime)
        {
            elapsed += Time.deltaTime;
            float tNorm = Mathf.Clamp01(elapsed / m_rotateTime);
            // Slerp for smooth shortest‐path interpolation
            t.localRotation = Quaternion.Slerp(startRot, endRot, tNorm);
            yield return null;
        }

        // Ensure exact final orientation
        t.localRotation = endRot;
        _rotateCoroutine = null;
    }

    private void ToggleMenu()
    {
        if (m_UIObject != null)
            m_UIObject.SetActive(!m_UIObject.activeSelf);
    }
}
