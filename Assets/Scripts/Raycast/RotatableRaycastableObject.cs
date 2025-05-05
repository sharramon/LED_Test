using UnityEngine;
using Oculus.Interaction; // for OVRHand

[RequireComponent(typeof(RaycastSelectable))]
public class RotatableRaycastableObject : RaycastableObject, IDraggableRaycastable
{
    // injected context
    private Transform _raycastOrigin;
    private OVRHand    _hand;

    // initial ray-based forward and right vectors (XZ plane)
    private Vector3 _initialForward;
    private Vector3 _initialRight;
    // initial lateral position of the hand relative to forward
    private float   _initialSideways;

    [Header("Rotation Settings")]
    [Tooltip("Degrees per second to spin the object around its center")]
    [SerializeField] private float spinSpeed      = 90f;
    [Tooltip("Normalized lateral movement threshold (0-1) before rotation starts")]
    [SerializeField] private float bufferDistance = 0.2f;

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
        // record forward vector of the ray on XZ plane
        Vector3 fwd = ray.direction;
        _initialForward = new Vector3(fwd.x, 0f, fwd.z).normalized;
        // compute perpendicular right vector
        _initialRight   = Vector3.Cross(Vector3.up, _initialForward);

        // record initial hand lateral alignment
        Vector3 camPos  = _raycastOrigin.position;
        Vector3 handPos = _hand.transform.position;
        Vector3 offset  = handPos - camPos;
        offset.y        = 0f;
        if (offset.sqrMagnitude > 1e-6f)
        {
            Vector3 handDir = offset.normalized;
            _initialSideways = Vector3.Dot(handDir, _initialRight);
        }
        else
        {
            _initialSideways = 0f;
        }
    }

    public void OnDrag(Ray ray)
    {
        // calculate current hand lateral position
        Vector3 camPos  = _raycastOrigin.position;
        Vector3 handPos = _hand.transform.position;
        Vector3 offset  = handPos - camPos;
        offset.y        = 0f;
        if (offset.sqrMagnitude < 1e-6f) return;

        Vector3 handDir = offset.normalized;
        float currentSideways = Vector3.Dot(handDir, _initialRight);
        float deltaSideways   = currentSideways - _initialSideways;

        // only rotate if movement exceeds buffer threshold
        if (Mathf.Abs(deltaSideways) > bufferDistance)
        {
            float direction = Mathf.Sign(deltaSideways);
            float angle     = spinSpeed * direction * Time.deltaTime;
            transform.Rotate(Vector3.up, angle, Space.World);
        }
    }

    public void OnDragEnd()
    {
        // nothing to clean up
    }
}
