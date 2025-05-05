using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class XRRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Transform _raycastOrigin;
    [SerializeField] private LayerMask _raycastLayerMask;
    [SerializeField] private float _maxHitDistance = 11f;
    [SerializeField] private float _maxDistance = 10f;

    [Header("Grab Pivot")]
    [SerializeField] private GameObject _grabPoint;
    [SerializeField] private float _lerpSpeed = 10f;
    [SerializeField] private float _fadeDuration = 0.2f;

    [Header("Input")]
    [SerializeField] private OVRHand _hand;

    [Header("Line Renderer")]
    [SerializeField] private AnimationCurve _lineReachCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float _hoverExtendSpeed = 2f;

    [Header("Pull/Push Settings")]
    [SerializeField] private float pullTriggerDistance = 0.1f;
    [SerializeField] private float pushTriggerDistance = 0.1f;
    [SerializeField] private float minPullDistance = 0.5f;
    [SerializeField] private float maxPushDistance = 5f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float acceleration = 0.5f;

    private LineRenderer _lineRenderer;
    private enum RayState { Idle, Hovering, Grabbing }
    private RayState _currentState = RayState.Idle;

    private RaycastSelectable _currentSelectable;
    private Transform _selectedTransform;
    private bool _isPinching;
    private RaycastInfo _raycastInfo;

    private float _pinchTimer;
    private const float _pinchHoldTime = 0.1f;
    private float _hoverTime;

    // Grab state
    private float _initialGrabDistance;      // planar XZ radius
    private float _initialGrabY;             // locked height
    private float _initialHandDistanceXZ;    // planar hand-camera
    private float _pullTimer;
    private float _pushTimer;
    private Coroutine _fadeCoroutine;
    private Vector3 _initialGrabFlatDir;     // stored normalized XZ direction

    private struct RaycastInfo
    {
        public Ray ray;
        public RaycastHit hit;
        public bool didHit;
    }

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
        _raycastInfo.ray = new Ray(_raycastOrigin.position, _raycastOrigin.forward);
        _raycastInfo.didHit = Physics.Raycast(
            _raycastInfo.ray,
            out _raycastInfo.hit,
            _maxDistance,
            _raycastLayerMask
        );
        _isPinching = _hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        _pinchTimer = _isPinching ? _pinchTimer + Time.deltaTime : 0f;
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

    private void TransitionTo(RayState newState)
    {
        if (_currentState != newState && newState == RayState.Hovering)
            _hoverTime = 0f;
        _currentState = newState;
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
                TransitionTo(RayState.Hovering);
            }
        }
    }

    private void HandleHovering()
    {
        if (!_raycastInfo.didHit
            || _raycastInfo.hit.collider.GetComponent<RaycastSelectable>() != _currentSelectable
            || _raycastInfo.hit.distance > _maxHitDistance)
        {
            _currentSelectable?.OnHoverExit();
            _currentSelectable = null;
            TransitionTo(RayState.Idle);
            return;
        }

        if (_pinchTimer >= _pinchHoldTime)
        {
            _currentSelectable.OnSelect();
            _selectedTransform = _currentSelectable.transform;
            GrabSetup(_raycastInfo.hit);
            TransitionTo(RayState.Grabbing);
        }
    }

    private void GrabSetup(RaycastHit hit)
    {
        // Snap pivot to the exact hit point
        Vector3 hitPos = hit.point;
        _grabPoint.transform.position = hitPos;
        _initialGrabY = hitPos.y;

        // Compute planar offset & radius
        Vector3 rawOffset3D = hitPos - _raycastOrigin.position;
        Vector3 flatOffset = new Vector3(rawOffset3D.x, 0f, rawOffset3D.z);
        float flatDist = flatOffset.magnitude;
        _initialGrabDistance = flatDist;

        // Store planar direction, guard zero-length
        if (flatDist > 0.0001f)
            _initialGrabFlatDir = flatOffset / flatDist;
        else
        {
            Vector3 camFwdXZ = new Vector3(_raycastOrigin.forward.x, 0f, _raycastOrigin.forward.z);
            _initialGrabFlatDir = camFwdXZ.sqrMagnitude > 0.0001f
                ? camFwdXZ.normalized
                : Vector3.forward;
        }

        // Record hand-to-camera planar distance
        Vector3 camPos = Camera.main.transform.position;
        Vector2 camXZ = new Vector2(camPos.x, camPos.z);
        Vector2 handXZ = new Vector2(_hand.transform.position.x, _hand.transform.position.z);
        _initialHandDistanceXZ = Vector2.Distance(camXZ, handXZ);

        _pullTimer = _pushTimer = 0f;

        _selectedTransform.SetParent(_grabPoint.transform, true);

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeLineRenderer(0f));
    }

    private void HandleGrabbing()
{
    if (!_isPinching)
    {
        Release();
        TransitionTo(RayState.Idle);
        return;
    }

    // Compute camera, grab‑point, and hand distances
    Vector3 camPos   = Camera.main.transform.position;
    Vector3 grabPos  = _grabPoint.transform.position;
    Vector3 camFlat  = new Vector3(camPos.x, grabPos.y, camPos.z);

    Vector2 camXZ    = new Vector2(camPos.x, camPos.z);
    Vector2 handXZ   = new Vector2(_hand.transform.position.x, _hand.transform.position.z);
    float currentHandDist = Vector2.Distance(camXZ, handXZ);
    float delta      = _initialHandDistanceXZ - currentHandDist;

    float dt = Time.deltaTime;

    // Pull
    if (delta > pullTriggerDistance)
    {
        _pullTimer += dt;
        _pushTimer = 0f;

        float step = moveSpeed * dt + acceleration * dt * dt;
        Vector3 pullDir = (camFlat - grabPos).normalized;
        Vector3 newPos  = grabPos + pullDir * step;

        // Clamp to minimum pull distance
        float distFlat = Vector3.Distance(
            new Vector3(newPos.x, 0f,    newPos.z),
            new Vector3(camFlat.x, 0f, camFlat.z)
        );
        if (distFlat < minPullDistance)
            newPos = camFlat + pullDir * minPullDistance;

        _grabPoint.transform.position = newPos;

        // Update planar radius for later orbit
        _initialGrabDistance = Vector3.Distance(
            _raycastOrigin.position,
            new Vector3(newPos.x, _initialGrabY, newPos.z)
        );
    }
    // Push
    else if (delta < -pushTriggerDistance)
    {
        _pushTimer += dt;
        _pullTimer = 0f;

        float step = moveSpeed * dt + acceleration * dt * dt;
        Vector3 awayDir = (grabPos - camFlat).normalized;
        Vector3 newPos   = grabPos + awayDir * step;

        // Clamp to maximum push distance
        float distFlat2 = Vector3.Distance(
            new Vector3(newPos.x, 0f,    newPos.z),
            new Vector3(camFlat.x, 0f, camFlat.z)
        );
        if (distFlat2 > maxPushDistance)
            newPos = camFlat + awayDir * maxPushDistance;

        _grabPoint.transform.position = newPos;

        _initialGrabDistance = Vector3.Distance(
            _raycastOrigin.position,
            new Vector3(newPos.x, _initialGrabY, newPos.z)
        );
    }
    // Neither pulling nor pushing
    else
    {
        _pullTimer = _pushTimer = 0f;
    }

    // Fallback orbit: follow current hand direction around the circle
    Vector3 flatFwd = _raycastOrigin.forward;
    flatFwd.y = 0f;
    flatFwd.Normalize();

    Vector3 orbitPoint = _raycastOrigin.position + flatFwd * _initialGrabDistance;
    Vector3 targetPos  = new Vector3(orbitPoint.x, _initialGrabY, orbitPoint.z);

    _grabPoint.transform.position = Vector3.MoveTowards(
        _grabPoint.transform.position,
        targetPos,
        _lerpSpeed * dt
    );
}

    private void Release()
    {
        _currentSelectable?.OnUnselect();
        if (_raycastInfo.didHit
            && _raycastInfo.hit.collider.TryGetComponent<RaycastSelectable>(out var sel)
            && sel == _currentSelectable)
        {
            _currentSelectable.OnHoverEnter();
        }

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeLineRenderer(1f));

        if (_selectedTransform != null)
            _selectedTransform.SetParent(null, true);

        _selectedTransform = null;
    }

    private IEnumerator FadeLineRenderer(float targetAlpha)
    {
        Color startCol = _lineRenderer.startColor;
        Color endCol   = _lineRenderer.endColor;
        float initialAlpha = startCol.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / _fadeDuration;
            float a = Mathf.Lerp(initialAlpha, targetAlpha, t);
            _lineRenderer.startColor = new Color(startCol.r, startCol.g, startCol.b, a);
            _lineRenderer.endColor   = new Color(endCol.r,   endCol.g,   endCol.b,   a);
            yield return null;
        }

        _lineRenderer.startColor = new Color(startCol.r, startCol.g, startCol.b, targetAlpha);
        _lineRenderer.endColor   = new Color(endCol.r,   endCol.g,   endCol.b,   targetAlpha);
    }

    private void UpdateLineRenderer()
    {
        _lineRenderer.SetPosition(0, _raycastOrigin.position);
        Vector3 targetPos = _raycastOrigin.position + _raycastOrigin.forward * 0.1f;

        if (_currentState == RayState.Grabbing)
        {
            targetPos = _grabPoint.transform.position;
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
