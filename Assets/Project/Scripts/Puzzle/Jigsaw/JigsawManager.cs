using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JigsawManager : MonoBehaviour
{
    [Header("Scramble Settings")]
    [Tooltip("Acak posisi kepingan puzzle secara otomatis saat game dimulai?")]
    public bool scrambleAtStart = true;
    
    [Tooltip("Area Collider2D untuk merandom posisi kepingan puzzle. Jika kosong, akan merandom di sekitar JigsawManager.")]
    public Collider2D scrambleArea;
    
    [Tooltip("Radius horizontal jika tidak ada scrambleArea.")]
    public float scrambleRangeX = 6f;
    
    [Tooltip("Radius vertikal jika tidak ada scrambleArea.")]
    public float scrambleRangeY = 3f;

    [Header("UI & Feedback")]
    [Tooltip("UI Panel yang akan diaktifkan secara otomatis saat puzzle selesai.")]
    public GameObject winUIPanel;

    [Header("Scene Transition Settings")]
    [Tooltip("Nama scene berikutnya yang akan diload setelah puzzle selesai.")]
    public string nextSceneName = "Level2";
    [Tooltip("Durasi transisi fade (ke putih dan kembali ke transparan) dalam detik.")]
    public float fadeDuration = 1.0f;

    [Header("Audio Settings")]
    [Tooltip("Lagu latar belakang (BGM) untuk puzzle Jigsaw.")]
    public AudioClip bgmClip;
    [Tooltip("SFX yang dimainkan saat semua kepingan puzzle selesai disusun.")]
    public AudioClip winSFX;

    private JigsawPiece[] pieces;
    private bool puzzleCompleted = false;
    private AudioSource audioSource;

    private void Start()
    {
        // Mencari semua kepingan JigsawPiece yang ada di scene secara dinamis
        pieces = FindObjectsByType<JigsawPiece>(FindObjectsSortMode.None);

        if (scrambleAtStart)
        {
            ScramblePieces();
        }

        // Menyembunyikan Win UI di awal game
        if (winUIPanel != null)
        {
            winUIPanel.SetActive(false);
        }

        // Set up AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Set ke 2D sound
        }

        // Hentikan BGM dari scene sebelumnya jika ada AudioManager global untuk menghindari double BGM
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
        }

        // Putar BGM secara looping dengan fade in
        if (bgmClip != null)
        {
            audioSource.clip = bgmClip;
            audioSource.loop = true;
            audioSource.Play();
            StartCoroutine(FadeInBGM(fadeDuration, 0.8f));
        }
    }

    private IEnumerator FadeInBGM(float duration, float targetVolume)
    {
        if (audioSource == null || bgmClip == null) yield break;
        audioSource.volume = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    private void Update()
    {
        if (!puzzleCompleted)
        {
            CheckPuzzleComplete();
        }
    }

    private void ScramblePieces()
    {
        if (pieces == null || pieces.Length == 0) return;

        foreach (JigsawPiece piece in pieces)
        {
            if (piece == null || piece.isPlaced) continue;

            Vector3 randomPos;
            if (scrambleArea != null)
            {
                // Jika ada area collider, acak posisi di dalam batas collider tersebut
                Bounds bounds = scrambleArea.bounds;
                randomPos = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    piece.transform.position.z
                );
            }
            else
            {
                // Jika tidak ada collider, acak di sekitar posisi JigsawManager
                randomPos = transform.position + new Vector3(
                    Random.Range(-scrambleRangeX, scrambleRangeX),
                    Random.Range(-scrambleRangeY, scrambleRangeY),
                    0f
                );
            }

            // Atur posisi baru untuk kepingan
            piece.transform.position = randomPos;
        }
    }

    private void CheckPuzzleComplete()
    {
        if (pieces == null || pieces.Length == 0) return;

        foreach (JigsawPiece piece in pieces)
        {
            if (piece == null || !piece.isPlaced)
            {
                return; // Masih ada kepingan yang belum pas
            }
        }

        // Jika semua kepingan sudah di posisi masing-masing
        puzzleCompleted = true;
        Debug.Log("LOG_WIN");

        // Mainkan SFX kemenangan jika diisi
        if (winSFX != null && audioSource != null)
        {
            // PlayOneShot agar tidak memutus BGM sebelum pindah scene
            audioSource.PlayOneShot(winSFX);
        }

        // Aktifkan Win UI Panel jika ada
        if (winUIPanel != null)
        {
            winUIPanel.SetActive(true);
        }

        // Jalankan transisi scene otomatis setelah 2 detik
        StartCoroutine(AutoLoadNextSceneRoutine());
    }

    private IEnumerator FadeOutAllLoopingAudio(float duration)
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        if (sources == null || sources.Length == 0) yield break;

        // Simpan volume awal untuk semua AudioSource yang sedang play dan looping
        System.Collections.Generic.Dictionary<AudioSource, float> startVolumes = new System.Collections.Generic.Dictionary<AudioSource, float>();
        foreach (var src in sources)
        {
            if (src != null && src.isPlaying && src.loop)
            {
                startVolumes[src] = src.volume;
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            foreach (var kvp in startVolumes)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.volume = Mathf.Lerp(kvp.Value, 0f, t);
                }
            }
            yield return null;
        }

        foreach (var kvp in startVolumes)
        {
            if (kvp.Key != null)
            {
                kvp.Key.volume = 0f;
                kvp.Key.Stop();
            }
        }
    }

    private IEnumerator AutoLoadNextSceneRoutine()
    {
        // Tunggu 0.5 detik setelah menang (sesuai jeda)
        yield return new WaitForSecondsRealtime(0.5f);

        // Memudarkan semua BGM looping di scene
        StartCoroutine(FadeOutAllLoopingAudio(fadeDuration));
        
        // 1. Buat Canvas Transisi secara dinamis
        GameObject canvasGo = new GameObject("TransitionCanvas");
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer != -1) canvasGo.layer = uiLayer;

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999999; // Sangat tinggi agar di atas UI apa pun
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>(); // Agar memblokir klik/sentuhan saat transisi
        
        // Jangan dihancurkan saat ganti scene
        DontDestroyOnLoad(canvasGo);

        // 2. Buat Image putih di dalam Canvas
        GameObject imageGo = new GameObject("FadeImage");
        if (uiLayer != -1) imageGo.layer = uiLayer;
        imageGo.transform.SetParent(canvasGo.transform, false);
        Image fadeImage = imageGo.AddComponent<Image>();
        fadeImage.color = new Color(1f, 1f, 1f, 0f); // Transparan putih di awal

        // Atur rect transform agar menutupi seluruh layar
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;

        // 3. Tambahkan helper component ke Canvas yang tidak hancur saat ganti scene
        TransitionHelper helper = canvasGo.AddComponent<TransitionHelper>();
        helper.nextSceneName = nextSceneName;
        helper.fadeDuration = fadeDuration;
        helper.fadeImage = fadeImage;
        helper.StartTransition();
    }

    public void LoadNextScene()
    {
        StartCoroutine(AutoLoadNextSceneRoutine());
    }
}

// Helper Class untuk menjalankan Coroutine transisi lintas scene
public class TransitionHelper : MonoBehaviour
{
    public string nextSceneName;
    public float fadeDuration;
    public Image fadeImage;

    public void StartTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. Fade ke Putih (Alpha 0 -> 1)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        fadeImage.color = Color.white;

        // 2. Load Scene secara Asynchronous
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Fade dari Putih (Alpha 1 -> 0) di scene baru
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            fadeImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // 4. Hancurkan Canvas Transisi setelah selesai
        Destroy(gameObject);
    }
}