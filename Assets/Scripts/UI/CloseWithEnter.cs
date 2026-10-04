using UnityEngine;
using UnityEngine.InputSystem;

public class CloseWithEnter : MonoBehaviour
{
    [SerializeField] private GameObject panelToClose;

    private void Update()
    {
        if (!panelToClose.activeSelf)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            panelToClose.SetActive(false);
        }
    }
}