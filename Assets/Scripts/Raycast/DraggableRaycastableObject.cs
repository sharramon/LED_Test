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
        var camPos  = Camera.main.transform.position;
        var grabPos = _grabPoint.position;
        var camFlat = new Vector3(camPos.x, grabPos.y, camPos.z);

        var camXZ   = new Vector2(camPos.x, camPos.z);
        var handXZ  = new Vector2(_hand.transform.position.x, _hand.transform.position.z);
        float currentHandDist = Vector2.Distance(camXZ, handXZ);
        float delta           = _initialHandDistanceXZ - currentHandDist;
        float dt              = Time.deltaTime;

        if (delta > pullTriggerDistance)
        {
            // Pull
            float step   = moveSpeed * dt + acceleration * dt * dt;
            Vector3 dir  = (camFlat - grabPos).normalized;
            Vector3 newP = grabPos + dir * step;

            float distFlat = Vector2.Distance(
                new Vector2(newP.x, newP.z),
                new Vector2(camFlat.x, camFlat.z)
            );
            if (distFlat < minPullDistance)
                newP = camFlat + dir * minPullDistance;

            _grabPoint.position       = newP;
            _initialGrabDistance      = Vector3.Distance(
                _raycastOrigin.position,
                new Vector3(newP.x, _initialGrabY, newP.z)
            );
        }
        else if (delta < -pushTriggerDistance)
        {
            // Push
            float step   = moveSpeed * dt + acceleration * dt * dt;
            Vector3 dir  = (grabPos - camFlat).normalized;
            Vector3 newP = grabPos + dir * step;

            float distFlat2 = Vector2.Distance(
                new Vector2(newP.x, newP.z),
                new Vector2(camFlat.x, camFlat.z)
            );
            if (distFlat2 > maxPushDistance)
                newP = camFlat + dir * maxPushDistance;

            _grabPoint.position  = newP;
            _initialGrabDistance = Vector3.Distance(
                _raycastOrigin.position,
                new Vector3(newP.x, _initialGrabY, newP.z)
            );
        }
        
        // Orbit
        var flatFwd = new Vector3(_raycastOrigin.forward.x, 0f, _raycastOrigin.forward.z).normalized;
        var orbitPt = _raycastOrigin.position + flatFwd * _initialGrabDistance;
        var target  = new Vector3(orbitPt.x, _initialGrabY, orbitPt.z);

        _grabPoint.position = Vector3.MoveTowards(
            _grabPoint.position,
            target,
            lerpSpeed * dt
        );
    }

    public void OnDragEnd()
    {
        transform.SetParent(null, true);
        if (_grabPoint != null)
            Destroy(_grabPoint.gameObject);
    }
}
