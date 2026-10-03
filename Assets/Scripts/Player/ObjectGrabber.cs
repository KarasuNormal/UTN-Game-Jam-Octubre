using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabber : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float pickupRadius = 1.2f;
    [SerializeField] private float pickupHeight = 1f;
    [SerializeField] private float pickupForwardOffset = 0.8f;
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int lineSegments = 30;
    [SerializeField] private float timeStep = 0.05f;

    private Rigidbody heldObject;
    private Collider[] heldColliders;

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && heldObject != null)
        {
            ThrowObject();
        }

        if (heldObject != null)
        {
            DrawTrajectory();
        }
        else if (trajectoryLine != null && trajectoryLine.enabled)
        {
            trajectoryLine.enabled = false;
        }
    }

    private void DrawTrajectory()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = lineSegments;

        Vector3 startPosition = holdPoint.position;
        Vector3 startVelocity = (cameraTransform.forward * throwForce) / heldObject.mass;

        Vector3 currentPosition = startPosition;
        trajectoryLine.SetPosition(0, currentPosition);

        for (int i = 1; i < lineSegments; i++)
        {
            float timeOffset = i * timeStep;

            Vector3 gravityOffset = 0.5f * Physics.gravity * Mathf.Pow(timeOffset, 2);
            Vector3 nextPosition = startPosition + startVelocity * timeOffset + gravityOffset;

            if (Physics.Linecast(currentPosition, nextPosition, out RaycastHit hit))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                break;
            }

            trajectoryLine.SetPosition(i, nextPosition);
            currentPosition = nextPosition;
        }
    }

    private void TryPickup()
    {
        Vector3 detectionPosition =
            transform.position +
            Vector3.up * pickupHeight +
            transform.forward * pickupForwardOffset;

        Collider[] hits = Physics.OverlapSphere(
            detectionPosition,
            pickupRadius,
            pickupLayer
        );

        if (hits.Length == 0)
            return;

        Rigidbody closestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            ThrowableObject throwable = hit.GetComponentInParent<ThrowableObject>();

            if (throwable != null && throwable.IsLocked())
                continue;

            Rigidbody rb = hit.GetComponentInParent<Rigidbody>();

            if (rb == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                rb.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = rb;
            }
        }

        if (closestObject == null)
            return;

        heldObject = closestObject;

        ThrowableObject selectedThrowable =
            heldObject.GetComponent<ThrowableObject>();

        if (selectedThrowable != null)
            selectedThrowable.SetHeld();

        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.isKinematic = true;

        heldColliders = heldObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in heldColliders)
        {
            col.enabled = false;
        }

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    private void DropObject()
    {
        ThrowableObject throwable = heldObject.GetComponent<ThrowableObject>();

        if (throwable != null)
            throwable.SetDropped();

        heldObject.transform.SetParent(null);
        heldObject.isKinematic = false;

        EnableColliders();

        heldObject = null;
    }

    private void ThrowObject()
    {
        Rigidbody objectToThrow = heldObject;
        ThrowableObject throwable =
            objectToThrow.GetComponent<ThrowableObject>();

        objectToThrow.transform.SetParent(null);
        objectToThrow.isKinematic = false;

        EnableColliders();

        heldObject = null;

        if (throwable != null)
            throwable.SetThrown();

        objectToThrow.AddForce(
            cameraTransform.forward * throwForce,
            ForceMode.Impulse
        );
    }

    private void EnableColliders()
    {
        if (heldColliders == null)
            return;

        foreach (Collider col in heldColliders)
        {
            col.enabled = true;
        }

        heldColliders = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 detectionPosition =
            transform.position +
            Vector3.up * pickupHeight +
            transform.forward * pickupForwardOffset;

        Gizmos.DrawWireSphere(
            detectionPosition,
            pickupRadius
        );
    }
}