using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;
    public bool lookAtTarget = true;
    public Vector3 rotationOffset;
    public float distanceFromTarget = 5f;

    public bool useBoundaries = false;
    public Vector2 minXAndMaxX;
    public Vector2 minXAndMaxY;
    public Vector2 minZAndMaxZ;

    private void Start()
    {
        rotationOffset = transform.rotation.eulerAngles - target.rotation.eulerAngles;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;

        if (lookAtTarget)
        {
            transform.LookAt(target);
            transform.rotation *= Quaternion.Euler(rotationOffset);
        }

        transform.position -= transform.forward * distanceFromTarget;

        if (useBoundaries)
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minXAndMaxX.x, minXAndMaxX.y);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minXAndMaxY.y, minXAndMaxY.y);
            clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZAndMaxZ.x, minZAndMaxZ.y);
            transform.position = clampedPosition;
        }
    }
}
