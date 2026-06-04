using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    // Two separate image boxes that already contain the character name inside the sprite
    public Image namaImageAren; // Image with name "Aren"
    public Image namaImageLuma; // Image with name "Luma"

    [Header("Petunjuk Gambar References")]
    public GameObject petunjukSpaceImage;   // Gambar untuk petunjuk Space (saat dialog)
    public GameObject petunjukMoveImage;    // Gambar untuk petunjuk A/D (saat bergerak)
    public GameObject petunjukInteractImage;// Gambar untuk petunjuk E (saat interaksi)

    [Header("Legacy UI Reference (Optional)")]
    public GameObject petunjukPanel;
    public TextMeshProUGUI petunjukText;

    [Header("Visual Effects")]
    public GameObject overlaySimbolPecah;
    public int indexTriggerKedapKedip = 7;
    public AudioSource audioSource;
    public AudioClip suaraRetakan;

    [Header("Transitions")]
    public Image fadeImage;
    public float kecepatanFade = 1f;
    public GameObject arenPlayer;

    [Header("Dialogue Data")]
    public DialogueLine[] bookLines;
    public DialogueLine[] introLines;
    public DialogueLine[] dialogPanjangLuma;
    public DialogueLine[] dialogGerbangRusak;

    private PlayerMovement playerMovement;
    private int jenisDialogAktif = 1;
    private int index = 0;
    private bool dialogueActive = true;
    private bool playerInTriggerPuzzle = false;
    private bool sedangTungguLedakan = false;
    private bool playerDiAreaInteraksi = false;

    void Awake()
    {
        // Fallback: try to find the name image objects by name if they were not assigned in the inspector
        if (namaImageAren == null)
        {
            var go = GameObject.Find("NamaBox_Aren");
            if (go != null) namaImageAren = go.GetComponent<Image>();
        }
        if (namaImageLuma == null)
        {
            var go = GameObject.Find("NamaBox_Luma");
            if (go != null) namaImageLuma = go.GetComponent<Image>();
        }
    }

    void Start()
    {
        index = 0;
        dialogueActive = true;
        jenisDialogAktif = 1;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (overlaySimbolPecah != null) overlaySimbolPecah.SetActive(false);
        // Hide both name images at start
        if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
        if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
        if (petunjukPanel != null) petunjukPanel.SetActive(false);

        if (fadeImage == null)
        {
            GameObject fadeObj = GameObject.Find("FadePanel_baru") ?? GameObject.Find("FadePanel") ?? GameObject.Find("FadeImage");
            if (fadeObj != null)
            {
                fadeImage = fadeObj.GetComponent<UnityEngine.UI.Image>();
            }
        }
        if (fadeImage != null) { Color c = fadeImage.color; c.a = 0f; fadeImage.color = c; }

        if (arenPlayer != null)
        {
            playerMovement = arenPlayer.GetComponent<PlayerMovement>() ?? arenPlayer.GetComponentInChildren<PlayerMovement>();
        }
        SetMovement(false);
        RefreshDisplay();
        UpdatePetunjuk();
    }

    void Update()
    {
        if (sedangTungguLedakan && (Keyboard.current.spaceKey.wasPressedThisFrame || Pointer.current.press.wasPressedThisFrame))
        {
            sedangTungguLedakan = false;
            MemicuGerbangMacet();
            return;
        }

        if (dialogueActive && (Keyboard.current.spaceKey.wasPressedThisFrame || Pointer.current.press.wasPressedThisFrame))
        {
            NextLine();
        }

        if (jenisDialogAktif == 4 && playerInTriggerPuzzle && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(ProsesFadeLaluPindahScene("Puzzle_Jigsaw"));
        }
    }

    public void NextLine()
    {
        index++;
        DialogueLine[] lines = GetActiveDialogueArray();
        if (index < lines.Length) RefreshDisplay();
        else CloseDialogue();
    }

    void RefreshDisplay()
    {
        DialogueLine[] lines = GetActiveDialogueArray();
        if (lines == null || lines.Length == 0) return;

        if (index < lines.Length && dialogueText != null)
        {
            // Separate speaker name and dialogue text
            if (!string.IsNullOrEmpty(lines[index].speaker))
            {
                // Normalise speaker string (trim & lower case)
                string spk = lines[index].speaker.Trim().ToLower();
                Debug.Log("[DialogueManager] speaker normalized: '" + spk + "'");

                if (spk == "aren")
                {
                    // Show Aren name image on top
                    if (namaImageAren != null) {
                        namaImageAren.gameObject.SetActive(true);
                        namaImageAren.transform.SetAsLastSibling();
                    }
                    // Hide Luma image
                    if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
                }
                else if (spk == "luma")
                {
                    // Show Luma name image on top
                    if (namaImageLuma != null) {
                        namaImageLuma.gameObject.SetActive(true);
                        namaImageLuma.transform.SetAsLastSibling();
                    }
                    // Hide Aren image
                    if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
                }
                else
                {
                    // Unknown speaker – hide both
                    if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
                    if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
                }
            }
            else
            {
                // Hide both name images when no speaker
                if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
                if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
            }
            // Set dialogue text without speaker prefix
            dialogueText.text = lines[index].text;

            UpdatePetunjuk();

            if (jenisDialogAktif == 2 && index == indexTriggerKedapKedip && overlaySimbolPecah != null)
            {
                if (audioSource != null && suaraRetakan != null) audioSource.PlayOneShot(suaraRetakan);
                StartCoroutine(EfekKedapKedip(overlaySimbolPecah));
            }
        }
    }

    void CloseDialogue()
    {
        dialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        // Ensure both name images are hidden when dialogue closes
        if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
        if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);

        if (jenisDialogAktif == 1) { jenisDialogAktif = 5; SetMovement(true); }
        else if (jenisDialogAktif == 2) { sedangTungguLedakan = true; }
        else if (jenisDialogAktif == 3) { jenisDialogAktif = 4; SetMovement(true); }
        else if (jenisDialogAktif == 0) { StartCoroutine(JedaDanMulaiFade()); }

        UpdatePetunjuk();
    }

    public void MemicuPertemuanLuma() { SetMovement(false); jenisDialogAktif = 2; index = 0; dialogueActive = true; if (dialoguePanel != null) dialoguePanel.SetActive(true); RefreshDisplay(); }

    public void StartBookDialogue()
    {
        SetMovement(false);
        jenisDialogAktif = 0;
        index = 0;
        dialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshDisplay();
    }

    void MemicuGerbangMacet() { jenisDialogAktif = 3; index = 0; dialogueActive = true; if (dialoguePanel != null) dialoguePanel.SetActive(true); RefreshDisplay(); }

    public void SetPlayerDiAreaPuzzle(bool diArea) { playerInTriggerPuzzle = diArea; UpdatePetunjuk(); }
    void SetMovement(bool bisaGerak) { if (playerMovement != null) playerMovement.canMove = bisaGerak; }

    // === SISTEM PETUNJUK/TIPS ===

    /// <summary>
    /// Dipanggil oleh trigger scripts saat player masuk area interaksi (Luma, Book, Puzzle)
    /// </summary>
    public void SetPetunjukInteraksi(bool diArea)
    {
        playerDiAreaInteraksi = diArea;
        UpdatePetunjuk();
    }

    /// <summary>
    /// Update teks petunjuk berdasarkan state saat ini
    /// </summary>
    void UpdatePetunjuk()
    {
        // 1. Matikan semua petunjuk gambar terlebih dahulu
        if (petunjukSpaceImage != null) petunjukSpaceImage.SetActive(false);
        if (petunjukMoveImage != null) petunjukMoveImage.SetActive(false);
        if (petunjukInteractImage != null) petunjukInteractImage.SetActive(false);

        // 2. Tentukan state petunjuk yang aktif
        bool spaceAktif = dialogueActive;
        bool interactAktif = !dialogueActive && (playerDiAreaInteraksi || playerInTriggerPuzzle);
        bool moveAktif = !dialogueActive && !interactAktif && (playerMovement != null && playerMovement.canMove);

        // 3. Aktifkan petunjuk gambar sesuai state
        if (spaceAktif)
        {
            if (petunjukSpaceImage != null) petunjukSpaceImage.SetActive(true);
        }
        else if (interactAktif)
        {
            if (petunjukInteractImage != null) petunjukInteractImage.SetActive(true);
        }
        else if (moveAktif)
        {
            if (petunjukMoveImage != null) petunjukMoveImage.SetActive(true);
        }

        // 4. Logika fallback legacy text-based petunjuk (agar tidak merusak setup lama)
        if (petunjukPanel != null)
        {
            if (spaceAktif)
            {
                petunjukPanel.SetActive(true);
                if (petunjukText != null) petunjukText.text = "Tekan SPACE untuk melanjutkan dialog";
            }
            else if (interactAktif)
            {
                petunjukPanel.SetActive(true);
                if (petunjukText != null) petunjukText.text = "Tekan E untuk interaksi";
            }
            else if (moveAktif)
            {
                petunjukPanel.SetActive(true);
                if (petunjukText != null) petunjukText.text = "Jalankan player dengan A / D";
            }
            else
            {
                petunjukPanel.SetActive(false);
            }
        }
    }

    DialogueLine[] GetActiveDialogueArray()
    {
        if (jenisDialogAktif == 0) return bookLines;
        if (jenisDialogAktif == 2) return dialogPanjangLuma;
        if (jenisDialogAktif == 3) return dialogGerbangRusak;
        return introLines;
    }

    IEnumerator TampilkanPetunjukSementara(string pesan, float durasi)
    {
        if (petunjukPanel != null) { petunjukPanel.SetActive(true); petunjukText.text = pesan; yield return new WaitForSeconds(durasi); UpdatePetunjuk(); }
    }

    IEnumerator EfekKedapKedip(GameObject obj)
    {
        Image img = obj.GetComponent<Image>();
        if (img == null) yield break;

        // Mulai dari invisible (alpha 0), lalu aktifkan
        obj.SetActive(true);
        Color c = img.color;
        img.color = new Color(c.r, c.g, c.b, 0f);

        for (int i = 0; i < 3; i++)
        {
            // Fade In
            for (float a = 0f; a <= 1f; a += 0.1f)
            {
                img.color = new Color(c.r, c.g, c.b, a);
                yield return new WaitForSeconds(0.03f);
            }
            img.color = new Color(c.r, c.g, c.b, 1f);

            // Fade Out
            for (float a = 1f; a >= 0f; a -= 0.1f)
            {
                img.color = new Color(c.r, c.g, c.b, a);
                yield return new WaitForSeconds(0.03f);
            }
            img.color = new Color(c.r, c.g, c.b, 0f);
        }

        // Setelah kedap-kedip selesai, simbol tetap terlihat
        img.color = new Color(c.r, c.g, c.b, 1f);
    }

    IEnumerator JedaDanMulaiFade() { yield return new WaitForSecondsRealtime(0.2f); StartCoroutine(ProsesFadeLaluPindahScene("BookWorldScene")); }

    IEnumerator ProsesFadeLaluPindahScene(string n) { SetMovement(false); if (fadeImage != null) { Color w = fadeImage.color; w.a = 0; while (w.a < 1) { w.a += Time.unscaledDeltaTime * kecepatanFade; fadeImage.color = w; yield return null; } } yield return new WaitForSecondsRealtime(0.5f); SceneManager.LoadScene(n); }
}