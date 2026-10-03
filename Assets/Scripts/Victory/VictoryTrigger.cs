using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;

    private bool hasWon;

    private void OnTriggerEnter(Collider other)
    {
        if (hasWon)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasWon = true;

        victoryPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}