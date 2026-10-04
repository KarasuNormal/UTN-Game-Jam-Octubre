using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("Configuración del Sway")]
    [Tooltip("Qué tanto se inclina la gomera al mover el mouse.")]
    [SerializeField] private float swayMultiplier = 0.5f;

    [Tooltip("La velocidad a la que la gomera vuelve a su posición original.")]
    [SerializeField] private float smoothStep = 8f;

    [Tooltip("El ángulo máximo de inclinación para que no dé la vuelta completa.")]
    [SerializeField] private float maxSwayAngle = 4f;

    private Quaternion initialRotation;

    private void Start()
    {
        // Guardamos la rotación inicial que le pusiste en el editor (ej: mirando al frente)
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        // Leemos el movimiento crudo del mouse en este frame
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Calculamos la rotación inversa en los ejes X e Y (clamp limitando el ángulo máximo)
        float rotationX = Mathf.Clamp(-mouseDelta.y * swayMultiplier, -maxSwayAngle, maxSwayAngle);
        float rotationY = Mathf.Clamp(mouseDelta.x * swayMultiplier, -maxSwayAngle, maxSwayAngle);

        // Creamos la rotación objetivo combinando la inicial con el desfase
        Quaternion targetRotation = initialRotation * Quaternion.Euler(rotationX, rotationY, 0f);

        // Suavizamos el movimiento desde la rotación actual hacia la rotación objetivo
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothStep * Time.deltaTime);
    }
}