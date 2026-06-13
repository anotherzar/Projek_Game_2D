using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryManager : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("Container for the pre-placed/fixed prompt cards (e.g., Left side)")]
    public Transform fixedCardsContainer;
    
    [Tooltip("Container for the empty drop slots (e.g., Middle/Right side, aligned with fixed cards)")]
    public Transform slotsContainer;
    
    [Tooltip("Container where draggable cards are spawned and shuffled")]
    public Transform draggablePoolContainer;
    
    [Tooltip("A high-level Canvas transform to parent cards during dragging so they render on top of others")]
    public Transform dragParent;

    [Header("Prefabs")]
    [Tooltip("Prefab for the cards (used for both fixed prompts and draggable cards)")]
    public GameObject memoryCardPrefab;
    
    [Tooltip("Prefab for the empty target slot containing the MemorySlot script")]
    public GameObject slotPrefab;

    [Header("Data")]
    public List<CardData> availableCards;

    [Header("UI Feedback Panels")]
    public GameObject winPanel;

    [Header("Audio Settings")]
    [Tooltip("Lagu latar belakang (BGM) untuk puzzle Memory.")]
    public AudioClip bgmClip;
    [Tooltip("Durasi transisi fade dalam detik.")]
    public float fadeDuration = 1.0f;

    [Header("Scene Transition Settings")]
    [Tooltip("Nama scene berikutnya yang akan diload setelah puzzle selesai.")]
    public string nextSceneName = "Level2_Page2";

    private int totalPairs;
    private int matchedPairs;
    private AudioSource audioSource;

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);

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

        InitializeMatchingGame();
    }

    void InitializeMatchingGame()
    {
        if (availableCards == null || availableCards.Count == 0) return;

        totalPairs = availableCards.Count;
        matchedPairs = 0;

        // 1. Spawn Fixed Prompt Cards & matching slots
        // We want the prompts and slots to be spawned in the same order so they align perfectly
        foreach (var data in availableCards)
        {
            // Spawn Fixed Card
            if (memoryCardPrefab != null && fixedCardsContainer != null)
            {
                GameObject fixedCardObj = Instantiate(memoryCardPrefab, fixedCardsContainer);
                MemoryCard card = fixedCardObj.GetComponent<MemoryCard>();
                if (card != null)
                {
                    // Setup as static, using fixedSprite
                    card.SetupStatic(data.cardID, data.fixedSprite);
                }
            }

            // Spawn Empty Slot
            if (slotPrefab != null && slotsContainer != null)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                MemorySlot slot = slotObj.GetComponent<MemorySlot>();
                if (slot != null)
                {
                    slot.Setup(data.cardID, this);
                }
            }
        }

        // 2. Prepare Draggable Cards
        List<CardData> draggableDeck = new List<CardData>(availableCards);
        Shuffle(draggableDeck);

        // Spawn Draggable Cards into the pool
        foreach (var data in draggableDeck)
        {
            if (memoryCardPrefab != null && draggablePoolContainer != null)
            {
                GameObject dragCardObj = Instantiate(memoryCardPrefab, draggablePoolContainer);
                MemoryCard card = dragCardObj.GetComponent<MemoryCard>();
                if (card != null)
                {
                    // Setup as draggable, using cardSprite
                    card.Setup(data.cardID, data.cardSprite, this, dragParent);
                }
            }
        }
    }

    void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void OnCardMatched(int cardID)
    {
        matchedPairs++;
        Debug.Log($"Card matched! ID: {cardID}. Total matched: {matchedPairs}/{totalPairs}");

        if (matchedPairs >= totalPairs)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        if (winPanel != null) winPanel.SetActive(true);
        Debug.Log("Puzzle Complete! All cards matched correctly.");

        // Jalankan transisi scene otomatis setelah 2 detik
        StartCoroutine(AutoLoadNextSceneRoutine());
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

    private IEnumerator FadeOutAllLoopingAudio(float duration)
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        if (sources == null || sources.Length == 0) yield break;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (bgmClip == null)
        {
            bgmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Project/Audio/BGM/Jigsaw/BGM_Jigsaw.mp3");
        }
    }
#endif
}
