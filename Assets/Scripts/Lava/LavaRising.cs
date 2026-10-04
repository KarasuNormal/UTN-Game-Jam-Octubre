using UnityEngine;

public class LavaRising : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 0.5f;
    [SerializeField] private float startDelay = 30f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < startDelay)
            return;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
    }
}