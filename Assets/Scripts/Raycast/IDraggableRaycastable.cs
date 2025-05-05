// IDraggableRaycastable.cs
using UnityEngine;

public interface IDraggableRaycastable
{
    /// <summary>
    /// Called once, immediately when the user begins pinching this object.
    /// </summary>
    void OnDragStart(Ray ray, RaycastHit hit);

    /// <summary>
    /// Called every frame while the user is pinching.
    /// </summary>
    void OnDrag(Ray ray);

    /// <summary>
    /// Called once, when the user releases the pinch.
    /// </summary>
    void OnDragEnd();
}