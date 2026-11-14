using UnityEngine;

// Mueve la isla con un Rigidbody cinematico (en vez de mover el Transform a mano)
// para que la fisica sepa que la isla se mueve: asi los objetos apoyados encima
// no la atraviesan y el jugador puede ser "arrastrado" por ella (ver ThirdPersonController).
public class FloatingIsland : MonoBehaviour
{
    [SerializeField] private float minHeight = 0.2f;
    [SerializeField] private float maxHeight = 0.6f;

    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.8f;

    private Vector3 startPosition;

    private float height;
    private float speed;
    private float timeOffset;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        startPosition = transform.position;

        height = Random.Range(minHeight, maxHeight);
        speed = Random.Range(minSpeed, maxSpeed);
        timeOffset = Random.Range(0f, 10f);
    }

    private void FixedUpdate()
    {
        float yOffset = Mathf.Sin((Time.time + timeOffset) * speed) * height;

        rb.MovePosition(new Vector3(
            startPosition.x,
            startPosition.y + yOffset,
            startPosition.z
        ));
    }
}
