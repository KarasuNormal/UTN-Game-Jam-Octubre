using UnityEngine;

public class UIsounds : MonoBehaviour
{
    static UIsounds instance;
    public static UIsounds Instance => instance;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip soundClip;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySound()
    {
        source.PlayOneShot(soundClip);
    }
}
