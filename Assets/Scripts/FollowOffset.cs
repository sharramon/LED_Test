using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionType
{
    Null,
    LookAt,
    LocalAxis,
    WorldAxis
}
public enum DirectionAxis
{
    X,
    Y,
    Z
}
public class FollowOffset : MonoBehaviour
{
    public Vector3 offset;
    public float maxFollowSpeed = 100f;
    public Transform followTransform;
    private Coroutine followCoroutine;

    public bool isXLocal = false;
    public bool isYLocal = false;
    public bool isZLocal = false;

    [Header("Look direction")]
    public DirectionType lookDirectionType = DirectionType.Null;
    public DirectionAxis lookDirectionAxis = DirectionAxis.X;
    //maybe implement an offset angle?
    public bool invert = false;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetFollowStats(Transform _followTransform, Vector3 _offSet, float _maxFollowSpeed, bool _isXLocal = false, bool _isYLocal = false, bool _isZLocal = false)
    {
        followTransform = _followTransform;
        offset = _offSet;
        maxFollowSpeed = _maxFollowSpeed;
        isXLocal = _isXLocal;
        isYLocal = _isYLocal;
        isZLocal = _isZLocal;
    }

    public void SnapToFollowPos()
    {
        Vector3 offsetRelativeToForward = GetAdjustedOffset();
        Vector3 targetPos = followTransform.transform.position + offsetRelativeToForward;
        this.transform.position = targetPos;
        AdjustDirection();
    }

    public void StartFollow()
    {
        SnapToFollowPos();
        followCoroutine = StartCoroutine(FollowObject());
    }

    public void StopFollow(float stopTime = 0.5f)
    {
        StartCoroutine(StopFollowObject(stopTime));
    }

    private void LookAtOrInvertTarget()
    {
        // Determine the direction to look towards or away from the target based on the invert flag
        Vector3 lookDirection = followTransform.position - transform.position;

        if (invert)
        {
            lookDirection = -lookDirection;
        }

        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private IEnumerator FollowObject()
    {
        while (this.gameObject.activeInHierarchy)
        {
            Vector3 offsetRelativeToForward = GetAdjustedOffset();
            Vector3 targetPos = followTransform.position + offsetRelativeToForward;
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, maxFollowSpeed * Time.deltaTime);

            AdjustDirection();

            yield return null;
        }
    }

    private IEnumerator StopFollowObject(float stopTime = 0.5f)
    {
        yield return new WaitForSeconds(stopTime);

        StopCoroutine(followCoroutine);
    }

    private Vector3 GetAdjustedOffset()
    {
        Vector3 adjustedOffset = Vector3.zero;

        if (isXLocal)
        {
            adjustedOffset += offset.x * followTransform.right;
        }
        else
        {
            adjustedOffset += new Vector3(offset.x, 0, 0);
        }

        if (isYLocal)
        {
            adjustedOffset += offset.y * followTransform.up;
        }
        else
        {
            adjustedOffset += new Vector3(0, offset.y, 0);
        }

        if (isZLocal)
        {
            adjustedOffset += offset.z * followTransform.forward;
        }
        else
        {
            adjustedOffset += new Vector3(0, 0, offset.z);
        }

        return adjustedOffset;
    }

    private void AdjustDirection()
    {
        switch(lookDirectionType)
        {
            case DirectionType.Null:
                break;
            case DirectionType.LookAt:
                LookAtOrInvertTarget();
                break;
            case DirectionType.LocalAxis:
                AdjustDirectionAxis(false);
                break;
            case DirectionType.WorldAxis:
                AdjustDirectionAxis(true);
                break;
        }
    }

    private void AdjustDirectionAxis(bool isGlobal)
    {
        if (followTransform == null)
            return;

        Vector3 newForward = Vector3.zero;

        // Determine the new forward direction based on the selected axis and whether to use world or local axes.
        switch (lookDirectionAxis)
        {
            case DirectionAxis.X:
                newForward = isGlobal ? Vector3.right : followTransform.right;
                break;
            case DirectionAxis.Y:
                newForward = isGlobal ? Vector3.up : followTransform.up;
                break;
            case DirectionAxis.Z:
                newForward = isGlobal ? Vector3.forward : followTransform.forward;
                break;
        }

        if (invert)
        {
            newForward = -newForward;
        }

        if (newForward != Vector3.zero)
        {
            transform.forward = newForward;
        }
    }
}
