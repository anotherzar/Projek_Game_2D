using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFlow : MonoBehaviour
{
    [Header("UI Reference")]
    public Image fadeImage; // Tarik FadePanel_baru ke sini

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    public float stayBlackTime = 0.5f;

    [Header("Background Objects")]
    public GameObject libraryBG;
    public GameObject bookWorldBG;

    private void Start()
    {
        if (fadeImage != null)
        {
            // Jangan matikan SetActive, tapi buat transparan & matikan blokir klik
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;

            // Biar gak ngeblok input DialogueManager pas awal game
            fadeImage.raycastTarget = false;
        }
    }

    public void EnterBook()
    {
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Aryo, objek Fade Image di Inspector masih KOSONG!");
            yield break;
        }

        // --- PROSES FADE IN ---
        fadeImage.raycastTarget = true; // Sekarang kita blokir input pas layar item
        float timer = 0;
        Color tempColor = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = tempColor;
            yield return null;
        }
        tempColor.a = 1;
        fadeImage.color = tempColor;

        yield return new WaitForSeconds(stayBlackTime);

        // --- GANTI BACKGROUND ---
        if (libraryBG != null) libraryBG.SetActive(false);
        if (bookWorldBG != null) bookWorldBG.SetActive(true);

        yield return new WaitForSeconds(stayBlackTime);

        // --- PROSES FADE OUT ---
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(1 - (timer / fadeDuration));
            fadeImage.color = tempColor;
            yield return null;
        }
        tempColor.a = 0;
        fadeImage.color = tempColor;

        // MATIKAN blokir input lagi biar bisa klik dialog selanjutnya
        fadeImage.raycastTarget = false;
    }
}