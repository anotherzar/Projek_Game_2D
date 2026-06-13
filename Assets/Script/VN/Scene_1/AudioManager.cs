using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource musicSource;

    void Awake()
    {
        // Sistem Singleton: Memastikan hanya ada satu AudioManager yang hidup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // JANGAN hancurkan objek ini saat ganti scene
            
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // Mode 2D
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip, float volume)
    {
        if (clip == null) return;
        
        // Jika musik yang diminta sudah berputar, jangan di-restart! (Biar nyambung)
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
        Debug.Log("[AudioManager] Memulai musik: " + clip.name);
    }

    public void StopBGM()
    {
        musicSource.Stop();
    }

    // Cek apakah sedang ada musik yang dimainkan
    public bool IsPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
}
