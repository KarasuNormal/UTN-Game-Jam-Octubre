using UnityEngine;

public class LavaSound : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private Collider surfaceCollider;
    [SerializeField] private Transform listenerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //surfaceCollider = GetComponent<Collider>();
        AudioListener listener = FindAnyObjectByType<AudioListener>();
        if (listener != null)
        {
            listenerTransform = listener.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (listenerTransform == null || surfaceCollider == null) return;
        Vector3 closestPoint = surfaceCollider.ClosestPoint(listenerTransform.position);
        audioSource.transform.position = closestPoint;
    }
}
