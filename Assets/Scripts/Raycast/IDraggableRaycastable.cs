// IDraggableRaycastable.cs
using UnityEngine;
using Oculus.Interaction; // for OVRHand

public interface IDraggableRaycastable
{
    /// <summary>
    /// Inject the ray origin (camera or controller) and the hand reference.
    /// </summary>
    void SetDragContext(Transform raycastOrigin, OVRHand hand);

    void OnDragStart(Ray ray, RaycastHit hit);
    void OnDrag(Ray ray);
    void OnDragEnd();
}