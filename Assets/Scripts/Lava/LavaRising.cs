using UnityEngine;

public class LavaRising : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 0.5f;

    private void Update()
    {
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
    }
}
