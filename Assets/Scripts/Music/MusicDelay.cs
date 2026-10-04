using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicDelay : MonoBehaviour
{
    [Tooltip("Tiempo de silencio en segundos (2 minutos = 120)")]
    [SerializeField] private float tiempoDeSilencio = 120f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Iniciamos el ciclo infinito
        StartCoroutine(CicloDeMusica());
    }

    private IEnumerator CicloDeMusica()
    {
        while (true) // Se repite para siempre
        {
            audioSource.Play(); // Le da play a la canción

            // Espera exactamente lo que dura la pista de audio
            yield return new WaitForSeconds(audioSource.clip.length);

            // Espera los 2 minutos de silencio absolutos
            yield return new WaitForSeconds(tiempoDeSilencio);
        }
    }
}