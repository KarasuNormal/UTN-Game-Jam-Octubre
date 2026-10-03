using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private GameObject defeatPanel;

    private bool hasLost;

    private void OnTriggerEnter(Collider other)
    {
        if (hasLost)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasLost = true;

        defeatPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
