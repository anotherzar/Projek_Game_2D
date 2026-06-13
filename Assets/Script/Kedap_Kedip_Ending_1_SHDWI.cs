using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Kedap_Kedip_Ending_1_SHDWI : MonoBehaviour
{
    [Header("Pengaturan Kedap-Kedip")]
    [Tooltip("Berapa kali kedap-kedip di awal")]
    public int jumlahKedip = 5;

    [Tooltip("Kecepatan kedap-kedip (detik)")]
    public float kecepatanKedip = 0.15f;

    [Header("Efek Besar ke Tengah")]
    [Tooltip("Ukuran akhir saat objek membesar (relatif dari ukuran awal)")]
    public float targetScale = 3f;

    [Tooltip("Durasi animasi membesar & bergerak ke tengah (detik)")]
    public float durasiAnimasi = 0.8f;

    [Tooltip("Gunakan SmoothStep agar animasi lebih natural")]
    public bool useSmoothStep = true;

    [Header("Fade Out Objek")]
    [Tooltip("Durasi fade out objek setelah scale selesai (detik)")]
    public float durasiFadeOut = 0.5f;

    [Header("Background Abu-Abu")]
    [Tooltip("(Opsi 1) SpriteRenderer background yang akan diubah warnanya ke abu-abu")]
    public SpriteRenderer backgroundSprite;

    [Tooltip("(Opsi 2) UI Image background yang akan diubah warnanya ke abu-abu")]
    public Image backgroundImage;

    [Tooltip("(Opsi 3) Jika tidak ada sprite/image, ubah warna Camera background (pastikan Clear Flags = Solid Color)")]
    public Camera targetCamera;

    [Tooltip("Warna abu-abu tujuan")]
    public Color warnaAbuAbu = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Tooltip("Durasi fade background ke abu-abu (detik), 0 = ikut durasiAnimasi")]
    public float durasiGantiBackground = 0f;

    [Header("SFX")]
    [Tooltip("Suara yang dimainkan saat karakter mulai membesar (assign SFX_Jigsaw_Lengkap)")]
    public AudioClip sfxScale;
    [Tooltip("Volume SFX")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    private AudioSource audioSource;

    [Header("Text To Be Continued")]
    [Tooltip("TextMeshPro UI untuk teks 'To Be Continued' — buat Text (TMP) di Canvas lalu assign ke sini")]
    public TextMeshProUGUI toBeContinuedText;

    [Tooltip("Teks yang diketik")]
    public string isiTeks = "To Be Continued...";

    [Tooltip("Kecepatan mengetik per karakter (detik)")]
    public float kecepatanKetik = 0.07f;

    [Tooltip("Jeda setelah objek hilang sebelum teks muncul (detik)")]
    public float jedaSebelumTeks = 0.4f;

    [Tooltip("Warna teks")]
    public Color warnaTeks = Color.white;

    [Tooltip("Ukuran font teks")]
    public float ukuranFont = 64f;

    // Private
    private SpriteRenderer spriteRendererObjek;
    private CanvasGroup canvasGroup;

    private Vector3 posisiAwal;
    private Vector3 skalaAwal;

    private Color warnaBgSpriteAwal;
    private Color warnaBgImageAwal;
    private Color warnaBgCameraAwal;

    void Awake()
    {
        // Sembunyikan teks dari frame pertama sebelum apapun berjalan
        if (toBeContinuedText != null)
        {
            toBeContinuedText.text = "";
            toBeContinuedText.gameObject.SetActive(false);
            Color c = toBeContinuedText.color;
            c.a = 0f;
            toBeContinuedText.color = c;
        }
    }

    void Start()
    {
        spriteRendererObjek = GetComponent<SpriteRenderer>();
        canvasGroup = GetComponent<CanvasGroup>();

        posisiAwal = transform.position;
        skalaAwal = transform.localScale;

        // Setup AudioSource untuk SFX
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        // Simpan warna awal background
        if (backgroundSprite != null)
            warnaBgSpriteAwal = backgroundSprite.color;

        if (backgroundImage != null)
            warnaBgImageAwal = backgroundImage.color;

        if (targetCamera == null)
            targetCamera = Camera.main;
        if (targetCamera != null)
            warnaBgCameraAwal = targetCamera.backgroundColor;

        // Setup teks — pastikan sudah tersembunyi (sudah dilakukan di Awake)
        if (toBeContinuedText != null)
        {
            toBeContinuedText.text = "";
            toBeContinuedText.fontSize = ukuranFont;
            toBeContinuedText.fontStyle = FontStyles.Bold;
            // Pastikan text dipaksa 1 baris
            toBeContinuedText.enableWordWrapping = false;
            toBeContinuedText.overflowMode = TextOverflowModes.Overflow;
            
            Color c = warnaTeks;
            c.a = 0f;
            toBeContinuedText.color = c;
            toBeContinuedText.gameObject.SetActive(false);
        }

        StartCoroutine(UrutanEfek());
    }

    IEnumerator UrutanEfek()
    {
        // === TAHAP 1: KEDAP-KEDIP ===
        for (int i = 0; i < jumlahKedip; i++)
        {
            SetVisibility(false);
            yield return new WaitForSeconds(kecepatanKedip);

            SetVisibility(true);
            yield return new WaitForSeconds(kecepatanKedip);
        }

        // Pastikan terlihat di akhir kedap-kedip
        SetVisibility(true);

        // Diam 2 detik sebelum animasi membesar dimulai
        yield return new WaitForSeconds(2f);

        // === TAHAP 2: BESAR KE TENGAH + BG ABU-ABU (bersamaan) ===
        // Mainkan SFX tepat saat scale dimulai
        if (sfxScale != null && audioSource != null)
            audioSource.PlayOneShot(sfxScale, sfxVolume);

        Vector3 posisiTengah = GetPosisiTengahLayar();
        Vector3 skalaTarget = skalaAwal * targetScale;

        float durasiActualBg = durasiGantiBackground > 0f ? durasiGantiBackground : durasiAnimasi;
        float elapsed = 0f;
        float elapsedMax = Mathf.Max(durasiAnimasi, durasiActualBg);

        while (elapsed < elapsedMax)
        {
            elapsed += Time.deltaTime;

            float rawTObj = Mathf.Clamp01(elapsed / durasiAnimasi);
            float tObj = useSmoothStep ? Mathf.SmoothStep(0f, 1f, rawTObj) : rawTObj;
            float tBg = Mathf.Clamp01(elapsed / durasiActualBg);

            transform.position = Vector3.Lerp(posisiAwal, posisiTengah, tObj);
            transform.localScale = Vector3.Lerp(skalaAwal, skalaTarget, tObj);

            Color lerpedColor;

            if (backgroundSprite != null)
            {
                lerpedColor = Color.Lerp(warnaBgSpriteAwal, warnaAbuAbu, tBg);
                lerpedColor.a = 1f;
                backgroundSprite.color = lerpedColor;
            }
            if (backgroundImage != null)
            {
                lerpedColor = Color.Lerp(warnaBgImageAwal, warnaAbuAbu, tBg);
                lerpedColor.a = 1f;
                backgroundImage.color = lerpedColor;
            }
            if (targetCamera != null && backgroundSprite == null && backgroundImage == null)
            {
                lerpedColor = Color.Lerp(warnaBgCameraAwal, warnaAbuAbu, tBg);
                lerpedColor.a = 1f;
                targetCamera.backgroundColor = lerpedColor;
            }

            yield return null;
        }

        // Nilai akhir pasti
        transform.position = posisiTengah;
        transform.localScale = skalaTarget;

        Color finalColor = warnaAbuAbu;
        finalColor.a = 1f;
        if (backgroundSprite != null) backgroundSprite.color = finalColor;
        if (backgroundImage != null) backgroundImage.color = finalColor;
        if (targetCamera != null && backgroundSprite == null && backgroundImage == null)
            targetCamera.backgroundColor = finalColor;

        // === TAHAP 3: FADE OUT OBJEK ===
        yield return StartCoroutine(FadeOutObjek());

        // Jeda singkat setelah objek hilang
        yield return new WaitForSeconds(jedaSebelumTeks);

        // === TAHAP 4: EFEK NGETIK "To Be Continued..." ===
        yield return StartCoroutine(EfekNgetik());
    }

    IEnumerator FadeOutObjek()
    {
        float elapsed = 0f;

        while (elapsed < durasiFadeOut)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / durasiFadeOut);

            if (spriteRendererObjek != null)
            {
                Color c = spriteRendererObjek.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRendererObjek.color = c;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        // Matikan visibility saja, jangan SetActive(false) karena akan menghentikan script ini!
        SetVisibility(false);
    }

    IEnumerator EfekNgetik()
    {
        if (toBeContinuedText == null) yield break;

        toBeContinuedText.gameObject.SetActive(true);
        toBeContinuedText.text = "";

        // Fade in teks perlahan di karakter pertama
        Color c = warnaTeks;
        c.a = 0f;
        toBeContinuedText.color = c;

        // Fade in teks
        float fadeElapsed = 0f;
        float fadeDur = 0.3f;
        while (fadeElapsed < fadeDur)
        {
            fadeElapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(fadeElapsed / fadeDur);
            toBeContinuedText.color = c;
            yield return null;
        }
        c.a = 1f;
        toBeContinuedText.color = c;

        // Ketik satu per satu
        for (int i = 0; i <= isiTeks.Length; i++)
        {
            toBeContinuedText.text = isiTeks.Substring(0, i);
            yield return new WaitForSeconds(kecepatanKetik);
        }
    }

    Vector3 GetPosisiTengahLayar()
    {
        if (targetCamera == null) return Vector3.zero;

        Vector3 tengah = new Vector3(0.5f, 0.5f,
            Mathf.Abs(targetCamera.transform.position.z - transform.position.z));

        Vector3 worldPos = targetCamera.ViewportToWorldPoint(tengah);
        worldPos.z = transform.position.z;
        return worldPos;
    }

    void SetVisibility(bool visible)
    {
        if (spriteRendererObjek != null)
            spriteRendererObjek.enabled = visible;

        if (canvasGroup != null)
            canvasGroup.alpha = visible ? 1f : 0f;
    }

    void Update()
    {

    }
}
