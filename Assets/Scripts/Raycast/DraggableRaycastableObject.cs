using UnityEngine;
using Oculus.Interaction; // for OVRHand

[RequireComponent(typeof(RaycastSelectable))]
public class DraggableRaycastableObject : RaycastableObject, IDraggableRaycastable
{
    // injected context
    private Transform _raycastOrigin;
    private OVRHand    _hand;

    // internal state
    private Transform _grabPoint;
    private float     _initialGrabDistance;
    private float     _initialGrabY;
    private float     _initialHandDistanceXZ;
    private Vector3   _initialGrabFlatDir;

    [Header("Pull/Push Settings")]
    [SerializeField] private float pullTriggerDistance = 0.1f;
    [SerializeField] private float pushTriggerDistance = 0.1f;
    [SerializeField] private float minPullDistance     = 0.5f;
    [SerializeField] private float maxPushDistance     = 5f;
    [SerializeField] private float moveSpeed           = 1f;
    [SerializeField] private float acceleration        = 0.5f;
    [SerializeField] private float lerpSpeed           = 10f;
    
    Transform _originalParent;
    /// <summary>
    /// Called by XRRaycaster to inject its ray origin and hand reference.
    /// </summary>
    public void SetDragContext(Transform raycastOrigin, OVRHand hand)
    {
        _raycastOrigin = raycastOrigin;
        _hand          = hand;
    }

    public void OnDragStart(Ray ray, RaycastHit hit)
    {
        // remember where we lived before grab
        _originalParent = transform.parent;
        
        _grabPoint = new GameObject($"{name}_GrabPoint").transform;
        _grabPoint.position = hit.point;
        transform.SetParent(_grabPoint, true);

        _initialGrabY = hit.point.y;

        var rawOffset  = hit.point - _raycastOrigin.position;
        var flatOffset = new Vector3(rawOffset.x, 0f, rawOffset.z);
        _initialGrabDistance = flatOffset.magnitude;
        _initialGrabFlatDir  = flatOffset.magnitude > 0.0001f
            ? flatOffset.normalized
            : new Vector3(_raycastOrigin.forward.x, 0f, _raycastOrigin.forward.z).normalized;

        var camXZ  = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        var handXZ = new Vector2(_hand.transform.position.x,          _hand.transform.position.z);
        _initialHandDistanceXZ = Vector2.Distance(camXZ, handXZ);
    }

    public void OnDrag(Ray ray)
    {
        // cache positions
        Vector3 camPos   = Camera.main.transform.position;
        Vector3 grabPos  = _grabPoint.position;
        Vector3 camFlat  = new Vector3(camPos.x, grabPos.y, camPos.z);

        // compute how much the hand moved since grab
        var camXZ         = new Vector2(camPos.x, camPos.z);
        var handXZ        = new Vector2(_hand.transform.position.x, _hand.transform.position.z);
        float currentHand = Vector2.Distance(camXZ, handXZ);
        float delta       = _initialHandDistanceXZ - currentHand;
        float dt          = Time.deltaTime;

        // adjust your desired radius
        float step             = moveSpeed * dt + acceleration * dt * dt;
        float desiredDistance  = _initialGrabDistance;

        if (delta >  pullTriggerDistance)
            desiredDistance = Mathf.Max(_initialGrabDistance - step, minPullDistance);
        else if (delta < -pushTriggerDistance)
            desiredDistance = Mathf.Min(_initialGrabDistance + step, maxPushDistance);

        _initialGrabDistance = desiredDistance;

        // always orbit at that radius around camera forward
        Vector3 flatFwd     = new Vector3(_raycastOrigin.forward.x, 0f, _raycastOrigin.forward.z).normalized;
        Vector3 orbitCenter = _raycastOrigin.position + flatFwd * _initialGrabDistance;
        Vector3 targetPos   = new Vector3(orbitCenter.x, _initialGrabY, orbitCenter.z);

        _grabPoint.position = Vector3.MoveTowards(
            _grabPoint.position,
            targetPos,
            lerpSpeed * dt
        );
    }

    public void OnDragEnd()
    {
        // put it back where it was (if _originalParent is null, that's fine)
        transform.SetParent(_originalParent, true);
        if (_grabPoint != null)
            Destroy(_grabPoint.gameObject);
    }
}
