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
    public float MaxThrowForce => maxThrowForce;
    [SerializeField] private float throwChargeInterval = 0.1f;

    [Header("UI Settings")]
    [SerializeField] private Image forceBar;
    [SerializeField] private Image forceBarFrame;

    [Header("Pickup")]
    [SerializeField] private LayerMask pickupLayer;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 shrunkenScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int lineSegments = 30;
    [SerializeField] private float timeStep = 0.05f;

    [Header("Rotation & Magic Float")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatSpeed = 2f;

    public bool IsRotatingObject { get; private set; }

    private Rigidbody heldObject;
    private Collider[] heldColliders;
    private ThrowableObject heldThrowableComponent;

    [SerializeField] private float currentThrowForce;
    public float CurrentThrowForce => currentThrowForce;

    private float throwChargeTimer;
    private bool isChargingThrow;
    private float magicFloatTimer;

    private void Start()
    {
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);

            if (forceBarFrame != null)
            {
                forceBarFrame.gameObject.SetActive(false);
            }
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
            IsRotatingObject = Keyboard.current.rKey.isPressed;

            HandleFloatingEffect();

            if (IsRotatingObject)
            {
                HandleRotation();
            }

            if (!IsRotatingObject)
            {
                HandleThrowInput();
            }
            else if (isChargingThrow)
            {
                CancelThrowCharge();
            }

            DrawTrajectory();
        }
        else
        {
            IsRotatingObject = false;

            if (trajectoryLine != null && trajectoryLine.enabled)
            {
                trajectoryLine.enabled = false;
            }
        }

        if (heldObject != null && isChargingThrow && forceBar != null)
        {
            forceBar.fillAmount = currentThrowForce;
        }
    }

    private void HandleRotation()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        heldObject.transform.Rotate(
            cameraTransform.up,
            -mouseDelta.x * rotationSpeed * Time.deltaTime,
            Space.World
        );

        heldObject.transform.Rotate(
            cameraTransform.right,
            mouseDelta.y * rotationSpeed * Time.deltaTime,
            Space.World
        );
    }

    private void HandleFloatingEffect()
    {
        magicFloatTimer += Time.deltaTime;

        float offsetY =
            Mathf.Sin(magicFloatTimer * floatSpeed) * floatAmplitude;

        heldObject.transform.position =
            holdPoint.position + new Vector3(0f, offsetY, 0f);
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

        if (
            Mouse.current.leftButton.wasReleasedThisFrame &&
            isChargingThrow
        )
        {
            ThrowObject();
        }
    }

    private void StartThrowCharge()
    {
        isChargingThrow = true;
        currentThrowForce = minThrowForce;
        throwChargeTimer = 0f;

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(true);

            if (forceBarFrame != null)
            {
                forceBarFrame.gameObject.SetActive(true);
            }

            forceBar.fillAmount = currentThrowForce;
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
            {
                currentThrowForce = maxThrowForce;
            }

            if (forceBar != null)
            {
                forceBar.fillAmount = currentThrowForce;
            }
        }
    }

    private void CancelThrowCharge()
    {
        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);

            if (forceBarFrame != null)
            {
                forceBarFrame.gameObject.SetActive(false);
            }
        }
    }

    private void DrawTrajectory()
    {
        if (heldObject == null)
            return;

        if (trajectoryLine == null)
            return;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = lineSegments;

        Vector3 startPosition = holdPoint.position;

        Vector3 startVelocity =
            (cameraTransform.forward * currentThrowForce) /
            heldObject.mass;

        Vector3 currentPosition = startPosition;

        trajectoryLine.SetPosition(
            0,
            currentPosition
        );

        for (int i = 1; i < lineSegments; i++)
        {
            float timeOffset = i * timeStep;

            Vector3 gravityOffset =
                0.5f *
                Physics.gravity *
                Mathf.Pow(timeOffset, 2);

            Vector3 nextPosition =
                startPosition +
                startVelocity * timeOffset +
                gravityOffset;

            if (
                Physics.Linecast(
                    currentPosition,
                    nextPosition,
                    out RaycastHit hit
                )
            )
            {
                trajectoryLine.positionCount = i + 1;

                trajectoryLine.SetPosition(
                    i,
                    hit.point
                );

                break;
            }

            trajectoryLine.SetPosition(
                i,
                nextPosition
            );

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
            ThrowableObject throwable =
                hit.GetComponentInParent<ThrowableObject>();

            if (
                throwable != null &&
                throwable.IsLocked()
            )
            {
                continue;
            }

            Rigidbody rb =
                hit.GetComponentInParent<Rigidbody>();

            if (rb == null)
                continue;

            float distance =
                Vector3.Distance(
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

        heldThrowableComponent =
            heldObject.GetComponent<ThrowableObject>();

        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.isKinematic = true;

        if (heldThrowableComponent != null)
        {
            heldThrowableComponent.ShrinkScale(
                shrunkenScale
            );
        }
        else
        {
            heldObject.transform.localScale =
                shrunkenScale;
        }

        heldColliders =
            heldObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in heldColliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        currentThrowForce = minThrowForce;
        isChargingThrow = false;
        magicFloatTimer = 0f;
    }

    private void DropObject()
    {
        if (heldObject != null)
        {
            if (heldThrowableComponent != null)
            {
                heldThrowableComponent.RestoreScale();
            }
            else
            {
                heldObject.transform.localScale =
                    Vector3.one;
            }

            heldObject.isKinematic = false;
        }

        EnableColliders();

        heldObject = null;
        heldThrowableComponent = null;

        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);

            if (forceBarFrame != null)
            {
                forceBarFrame.gameObject.SetActive(false);
            }
        }
    }

    private void ThrowObject()
    {
        Rigidbody objectToThrow = heldObject;

        if (objectToThrow != null)
        {
            if (heldThrowableComponent != null)
            {
                heldThrowableComponent.SetThrown();
            }
            else
            {
                objectToThrow.transform.localScale =
                    Vector3.one;
            }

            objectToThrow.isKinematic = false;

            EnableColliders();

            heldObject = null;
            heldThrowableComponent = null;

            objectToThrow.AddForce(
                cameraTransform.forward * currentThrowForce,
                ForceMode.Impulse
            );
        }

        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);

            if (forceBarFrame != null)
            {
                forceBarFrame.gameObject.SetActive(false);
            }
        }
    }

    private void EnableColliders()
    {
        if (heldColliders == null)
            return;

        foreach (Collider col in heldColliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
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