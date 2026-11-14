using UnityEngine;

// Volumen general del juego. Usa AudioListener.volume, que afecta a TODOS los sonidos
// (musica, efectos y UI), y lo guarda en PlayerPrefs para que se mantenga entre partidas.
public static class VolumeSettings
{
    private const string VolumeKey = "MasterVolume";
    private const float DefaultVolume = 1f;

    public static float Volume => AudioListener.volume;

    // Se aplica el volumen guardado antes de cargar la primera escena,
    // sin necesidad de poner nada en las escenas.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSavedVolume()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, DefaultVolume));
    }

    public static void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
