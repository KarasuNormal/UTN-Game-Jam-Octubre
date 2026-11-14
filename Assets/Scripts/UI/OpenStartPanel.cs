using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;

    [Header("Player Look / Movement Script to Disable")]
    [SerializeField] private MonoBehaviour playerLookScript;

    [Header("Volumen")]
    [Tooltip("Si el panel de pausa no tiene un VolumeSlider, se crea uno automaticamente.")]
    [SerializeField] private bool addVolumeSliderToPausePanel = true;
    [Tooltip("Sin panel de pausa (menu principal): el control de volumen se crea en este Canvas, en esta posicion.")]
    [SerializeField] private Vector2 menuVolumePosition = new Vector2(0f, -370f);

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
        {
            if (addVolumeSliderToPausePanel &&
                pausePanel.GetComponentInChildren<VolumeSlider>(true) == null)
            {
                VolumeSlider.Create(pausePanel.transform, new Vector2(0f, -400f), false);
            }

            pausePanel.SetActive(false);
        }
        else if (addVolumeSliderToPausePanel &&
                 GetComponent<Canvas>() != null &&
                 GetComponentInChildren<VolumeSlider>(true) == null)
        {
            // Menu principal: no hay panel de pausa, se agrega el volumen directo en el Canvas
            VolumeSlider volumeSlider = VolumeSlider.Create(transform, menuVolumePosition, true);

            // Justo despues del fondo, para que los paneles (por ejemplo Creditos) queden por encima
            if (transform.childCount > 1)
            {
                volumeSlider.transform.SetSiblingIndex(1);
            }
        }
    }

    private void Update()
    {
        // Sin panel de pausa (menu principal) Esc no hace nada: antes congelaba el tiempo
        // y al apretarlo de nuevo escondia el cursor del mouse
        if (pausePanel == null)
            return;

        // Si el juego termino (derrota/victoria pusieron el tiempo en 0) no se permite pausar/reanudar,
        // porque reanudar dejaba seguir jugando con la pantalla de derrota abierta
        if (!isPaused && Time.timeScale == 0f)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        if (playerLookScript != null)
        {
            playerLookScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (playerLookScript != null)
        {
            playerLookScript.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}