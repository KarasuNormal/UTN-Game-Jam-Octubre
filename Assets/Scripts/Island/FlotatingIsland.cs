using UnityEngine;

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

    private void Start()
    {
        startPosition = transform.position;

        height = Random.Range(minHeight, maxHeight);
        speed = Random.Range(minSpeed, maxSpeed);
        timeOffset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float yOffset = Mathf.Sin((Time.time + timeOffset) * speed) * height;

        transform.position = new Vector3(
            startPosition.x,
            startPosition.y + yOffset,
            startPosition.z
        );
    }
}