using System.Collections;
using UnityEngine;
using Oculus.Interaction; // for OVRHand

[RequireComponent(typeof(LineRenderer))]
public class XRRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Transform _raycastOrigin;
    [SerializeField] private LayerMask _raycastLayerMask;
    [SerializeField] private float _maxHitDistance = 11f;

    [Header("Line Renderer")]
    [SerializeField] private AnimationCurve _lineReachCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float _hoverExtendSpeed = 2f;
    [SerializeField] private float _fadeDuration = 0.2f;

    [Header("Hand Tracking")]
    [SerializeField] private OVRHand _hand;

    private LineRenderer _lineRenderer;
    private RaycastInfo _raycastInfo;
    private float _pinchTimer;
    private float _hoverTime;

    private RayState _currentState = RayState.Idle;
    private RaycastSelectable _currentSelectable;
    private IDraggableRaycastable _currentDraggable;

    private const float _pinchHoldTime = 0.1f;

    private enum RayState { Idle, Hovering, Grabbing }
    private struct RaycastInfo { public Ray ray; public RaycastHit hit; public bool didHit; }

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        UpdateRaycast();
        HandleState();
        UpdateLineRenderer();
    }

    private void UpdateRaycast()
    {
        var ray = new Ray(_raycastOrigin.position, _raycastOrigin.forward);
        _raycastInfo.ray = ray;
        if (Physics.Raycast(ray, out RaycastHit hit, _maxHitDistance, _raycastLayerMask))
        {
            _raycastInfo.didHit = true;
            _raycastInfo.hit = hit;
        }
        else
        {
            _raycastInfo.didHit = false;
        }
    }

    private void HandleState()
    {
        switch (_currentState)
        {
            case RayState.Idle:     HandleIdle();     break;
            case RayState.Hovering: HandleHovering(); break;
            case RayState.Grabbing: HandleGrabbing(); break;
        }
    }

    private void HandleIdle()
    {
        if (_raycastInfo.didHit && _raycastInfo.hit.distance <= _maxHitDistance)
        {
            var sel = _raycastInfo.hit.collider.GetComponent<RaycastSelectable>();
            if (sel != null)
            {
                _currentSelectable = sel;
                _currentSelectable.OnHoverEnter();
                _currentState = RayState.Hovering;
                _pinchTimer = 0f;
                _hoverTime  = 0f;
            }
        }
    }

    private void HandleHovering()
    {
        bool stillHovering = _raycastInfo.didHit
                            && _raycastInfo.hit.collider.GetComponent<RaycastSelectable>() == _currentSelectable
                            && _raycastInfo.hit.distance <= _maxHitDistance;
        if (!stillHovering)
        {
            _currentSelectable?.OnHoverExit();
            _currentSelectable = null;
            _currentState = RayState.Idle;
            return;
        }

        bool isPinching = _hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (isPinching) _pinchTimer += Time.deltaTime;
        else            _pinchTimer  = 0f;

        if (_pinchTimer >= _pinchHoldTime)
        {
            _currentSelectable.OnSelect();
            if (_currentSelectable.TryGetComponent<IDraggableRaycastable>(out var draggable))
            {
                draggable.SetDragContext(_raycastOrigin, _hand);
                draggable.OnDragStart(_raycastInfo.ray, _raycastInfo.hit);
                _currentDraggable = draggable;
            }
            _currentState = RayState.Grabbing;
        }
    }

    private void HandleGrabbing()
    {
        bool isPinching = _hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (!isPinching)
        {
            _currentDraggable?.OnDragEnd();
            _currentDraggable = null;
            _currentSelectable.OnUnselect();
            _currentSelectable = null;
            _currentState = RayState.Idle;
            return;
        }

        _currentDraggable?.OnDrag(_raycastInfo.ray);
    }

    private void UpdateLineRenderer()
    {
        _lineRenderer.SetPosition(0, _raycastOrigin.position);
        Vector3 targetPos = _raycastOrigin.position + _raycastOrigin.forward * 0.1f;

        if (_currentState == RayState.Grabbing)
        {
            if (_currentDraggable is Component draggableComp && draggableComp.transform.parent != null)
            {
                targetPos = draggableComp.transform.parent.position;
            }
        }
        else if (_currentState == RayState.Hovering && _raycastInfo.didHit)
        {
            _hoverTime += Time.deltaTime * _hoverExtendSpeed;
            float t = Mathf.Clamp01(_hoverTime);
            targetPos = Vector3.Lerp(
                _raycastOrigin.position,
                _raycastInfo.hit.point,
                _lineReachCurve.Evaluate(t)
            );
        }

        _lineRenderer.SetPosition(1, targetPos);
    }
}
