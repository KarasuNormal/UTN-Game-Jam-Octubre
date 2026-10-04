using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableObject : MonoBehaviour
{
    [SerializeField] private string bridgeTag = "BridgeGround";
	private string playerTag = "Player";
	
    private float bobbingAmplitude = 0.1f;
    private float bobbingSpeed = 2f;

    private Rigidbody rb;
    private bool wasThrown;
    private bool isLocked;
    private bool canBePlaced;
	private bool hasHabitant = false;
	
	Vector3 startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
	
    void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
		if (isLocked && !hasHabitant)
		{
            float newY = startPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;
            transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
		}
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
		if (other.CompareTag(playerTag))
        {
            hasHabitant = true;
        }
        if (!wasThrown || isLocked)
            return;

        if (other.CompareTag(bridgeTag))
        {
            canBePlaced = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
		if (other.CompareTag(playerTag))
        {
            hasHabitant = false;
        }
        if (isLocked)
            return;

        if (other.CompareTag(bridgeTag))
        {
            canBePlaced = false;
        }
    }

    private void LockObject()
    {
		startPos = transform.localPosition;
        isLocked = true;
        wasThrown = false;
        canBePlaced = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}