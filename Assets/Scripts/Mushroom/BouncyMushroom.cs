using StarterAssets;
using UnityEngine;

public class BouncyMushroom : MonoBehaviour
{
    [Header("Configuración de Salto")]
    [SerializeField] private float playerBounceHeight = 8f;
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] boingClips;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            var controller = other.GetComponentInParent<ThirdPersonController>();

            if (controller != null)
            {
                controller.ApplyBounce(playerBounceHeight);

                if (audioSource != null && boingClips != null && boingClips.Length > 0)
                {
                    int indiceAlAzar = Random.Range(0, boingClips.Length);

                    audioSource.PlayOneShot(boingClips[indiceAlAzar]);
                }
            }
        }
    }
}