using StarterAssets;
using UnityEngine;

public class BouncyMushroom : MonoBehaviour
{
    [Header("Configuración de Salto")]
    [SerializeField] private float playerBounceHeight = 8f;
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioSource boingSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            var controller = other.GetComponentInParent<ThirdPersonController>();

            if (controller != null)
            {
                controller.ApplyBounce(playerBounceHeight);

                if (boingSound != null)
                {
                    boingSound.Play();
                }
            }
        }
    }
}