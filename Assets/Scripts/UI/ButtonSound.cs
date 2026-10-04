using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    [SerializeField] private Button button;
    private GameObject instance;
    private UIsounds soundManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = GameObject.Find("UIsoundManager");
        soundManager = instance.GetComponent<UIsounds>();
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }
    public void PlaySound()
    {
        soundManager.PlaySound();
    }
}
