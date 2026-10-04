using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableObject : MonoBehaviour
{
    [SerializeField] private string bridgeTag = "BridgeGround";

    private Rigidbody rb;
    private bool wasThrown;
    private bool isLocked;
    private bool canBePlaced;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!wasThrown || isLocked || !canBePlaced)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)
        {
            LockObject();
        }
    }

    public void SetHeld()
    {
        wasThrown = false;
        canBePlaced = false;
    }

    public void SetDropped()
    {
        wasThrown = false;
        canBePlaced = false;
    }

    public void SetThrown()
    {
        wasThrown = true;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!wasThrown || isLocked)
            return;

        if (other.CompareTag(bridgeTag))
        {
            canBePlaced = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isLocked)
            return;

        if (other.CompareTag(bridgeTag))
        {
            canBePlaced = false;
        }
    }

    private void LockObject()
    {
        isLocked = true;
        wasThrown = false;
        canBePlaced = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}