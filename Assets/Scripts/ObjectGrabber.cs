using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform holdPoint;

    [Header("Pickup")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("Throw")]
    [SerializeField] private float throwForce = 12f;

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
    }

    private void TryPickup()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb == null)
                return;

            heldObject = rb;

          
            heldObject.isKinematic = true;
            heldObject.linearVelocity = Vector3.zero;
            heldObject.angularVelocity = Vector3.zero;

            // Guardamos y desactivamos sus colliders
            heldColliders = heldObject.GetComponentsInChildren<Collider>();

            foreach (Collider col in heldColliders)
            {
                col.enabled = false;
            }

            
            heldObject.transform.SetParent(holdPoint);

            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
        }
    }

    private void DropObject()
    {
        heldObject.transform.SetParent(null);

        heldObject.isKinematic = false;

        EnableColliders();

        heldObject = null;
    }

    private void ThrowObject()
    {
        Rigidbody objectToThrow = heldObject;

        objectToThrow.transform.SetParent(null);

        objectToThrow.isKinematic = false;

        EnableColliders();

        heldObject = null;

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
}