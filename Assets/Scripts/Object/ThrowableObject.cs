using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
    [SerializeField] private string bridgeTag = "BridgeGround";

    private Rigidbody rb;
    private bool wasThrown;
    private bool isLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetHeld()
    {
        wasThrown = false;
    }

    public void SetDropped()
    {
        wasThrown = false;
    }

    public void SetThrown()
    {
        wasThrown = true;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!wasThrown || isLocked)
            return;

        if (collision.gameObject.CompareTag(bridgeTag))
        {
            LockObject();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!wasThrown || isLocked)
            return;

        if (other.CompareTag(bridgeTag))
        {
            LockObject();
        }
    }

    private void LockObject()
    {
        isLocked = true;
        wasThrown = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}