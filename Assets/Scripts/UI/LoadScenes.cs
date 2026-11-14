using UnityEngine;
using UnityEngine.SceneManagement;

public class loadScenes : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadScenes(string scene)
    {
        // Las pantallas de derrota/victoria/pausa dejan el tiempo en 0: se restablece al cambiar de escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }
    /*public void Pause()
    {
        Time.timeScale = 0;
    }
    public void Resume()
    {
        Time.timeScale = 1;
    }
    */
}
