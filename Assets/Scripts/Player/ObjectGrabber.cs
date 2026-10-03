using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectGrabber : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float pickupRadius = 1.2f;
    [SerializeField] private float pickupHeight = 1f;
    [SerializeField] private float pickupForwardOffset = 0.8f;

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float throwChargeInterval = 0.1f;

    [Header("UI Settings")]
    [SerializeField] private Slider throwPowerSlider;

    [Header("Pickup")]
    [SerializeField] private LayerMask pickupLayer;

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int lineSegments = 30;
    [SerializeField] private float timeStep = 0.05f;

    private Rigidbody heldObject;
    private Collider[] heldColliders;

    private float currentThrowForce;
    private float throwChargeTimer;
    private bool isChargingThrow;

    private void Start()
    {
        if (throwPowerSlider != null)
        {
            throwPowerSlider.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else if (!isChargingThrow)
            {
                DropObject();
            }
        }

        if (heldObject != null)
        {
            HandleThrowInput();

            if (heldObject != null)
            {
                DrawTrajectory();
            }
        }
        else if (trajectoryLine != null && trajectoryLine.enabled)
        {
            trajectoryLine.enabled = false;
        }

        if (heldObject != null && isChargingThrow && throwPowerSlider != null)
        {
            throwPowerSlider.value = currentThrowForce;
        }
    }

    private void HandleThrowInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartThrowCharge();
        }

        if (isChargingThrow)
        {
            ChargeThrow();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isChargingThrow)
        {
            ThrowObject();
        }
    }

    private void StartThrowCharge()
    {
        isChargingThrow = true;
        currentThrowForce = minThrowForce;
        throwChargeTimer = 0f;

        if (throwPowerSlider != null)
        {
            throwPowerSlider.gameObject.SetActive(true);
            throwPowerSlider.minValue = minThrowForce;
            throwPowerSlider.maxValue = maxThrowForce;
            throwPowerSlider.value = currentThrowForce;
        }
    }

    private void ChargeThrow()
    {
        throwChargeTimer += Time.deltaTime;

        if (throwChargeTimer < throwChargeInterval)
            return;

        throwChargeTimer -= throwChargeInterval;

        if (currentThrowForce < maxThrowForce)
        {
            currentThrowForce += 1f;

            if (currentThrowForce > maxThrowForce)
                currentThrowForce = maxThrowForce;

            if (throwPowerSlider != null)
            {
                throwPowerSlider.value = currentThrowForce;
            }
        }
    }

    private void DrawTrajectory()
    {
        if (heldObject == null) return;
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = lineSegments;

        Vector3 startPosition = holdPoint.position;
        Vector3 startVelocity = (cameraTransform.forward * currentThrowForce) / heldObject.mass;

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
            if (!hit.CompareTag("Pickup"))
                continue;

            // Uso de método no genérico con typeof
            ThrowableObject throwable = hit.transform.GetComponentInParent(typeof(ThrowableObject)) as ThrowableObject;

            if (throwable != null && throwable.IsLocked())
                continue;

            Rigidbody rb = hit.transform.GetComponentInParent(typeof(Rigidbody)) as Rigidbody;

            if (rb == null)
                continue;

            float distance = Vector3.Distance(transform.position, rb.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = rb;
            }
        }

        if (closestObject == null)
            return;

        heldObject = closestObject;

        // Uso de método no genérico con typeof
        ThrowableObject selectedThrowable = heldObject.gameObject.GetComponent(typeof(ThrowableObject)) as ThrowableObject;

        if (selectedThrowable != null)
            selectedThrowable.SetHeld();

        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.isKinematic = true;

        // Obtener colliders hijos de forma no genérica
        Component[] rawColliders = heldObject.gameObject.GetComponentsInChildren(typeof(Collider));
        heldColliders = new Collider[rawColliders.Length];
        for (int i = 0; i < rawColliders.Length; i++)
        {
            heldColliders[i] = rawColliders[i] as Collider;
        }

        foreach (Collider col in heldColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        currentThrowForce = minThrowForce;
        isChargingThrow = false;
    }

    private void DropObject()
    {
        if (heldObject != null)
        {
            ThrowableObject throwable = heldObject.gameObject.GetComponent(typeof(ThrowableObject)) as ThrowableObject;

            if (throwable != null)
                throwable.SetDropped();

            heldObject.transform.SetParent(null);
            heldObject.isKinematic = false;
        }

        EnableColliders();
        heldObject = null;

        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        if (throwPowerSlider != null)
            throwPowerSlider.gameObject.SetActive(false);
    }

    private void ThrowObject()
    {
        Rigidbody objectToThrow = heldObject;

        if (objectToThrow != null)
        {
            ThrowableObject throwable = objectToThrow.gameObject.GetComponent(typeof(ThrowableObject)) as ThrowableObject;

            objectToThrow.transform.SetParent(null);
            objectToThrow.isKinematic = false;

            EnableColliders();

            heldObject = null;

            if (throwable != null)
                throwable.SetThrown();

            objectToThrow.AddForce(
                cameraTransform.forward * currentThrowForce,
                ForceMode.Impulse
            );
        }

        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        if (throwPowerSlider != null)
            throwPowerSlider.gameObject.SetActive(false);
    }

    private void EnableColliders()
    {
        if (heldColliders == null)
            return;

        foreach (Collider col in heldColliders)
        {
            if (col != null)
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