using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // KUNCI: Buat ngontrol komponen Image langsung

public class DialogueManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Visual Effects")]
    public GameObject overlaySimbolPecah;

    [Header("Visual & Transition Effects (DIRECT IMAGE SYSTEM)")]
    public Image fadeImage;      // Slot untuk pasang langsung objek FadePanel_Final (bukan CanvasGroup)
    public float kecepatanFade = 1f;

    [Header("Character Target")]
    public GameObject arenPlayer;

    [Header("Dialogue Data Arrays")]
    public DialogueLine[] bookLines;
    public DialogueLine[] introLines;
    public DialogueLine[] dialogPanjangLuma;
    public DialogueLine[] dialogGerbangRusak;

    private PlayerMovement playerMovement;

    private int jenisDialogAktif = 1;
    private int index = 0;
    private bool dialogueActive = true;
    private bool playerInTriggerPuzzle = false;
    private bool isTransitioning = false;
    private bool sedangTungguLedakan = false;

    void Start()
    {
        index = 0;
        dialogueActive = true;
        jenisDialogAktif = 1;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (overlaySimbolPecah != null) overlaySimbolPecah.SetActive(false);

        // Paksa visual warna fade di awal bener-bener transparan (Alpha = 0)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        // AMANIN REFERENSI: Cari script pergerakan di objek utama atau anak-anaknya sekalian biar gak bocor
        if (arenPlayer != null)
        {
            playerMovement = arenPlayer.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                playerMovement = arenPlayer.GetComponentInChildren<PlayerMovement>();
            }
        }

        // Paksa lock di awal banget game dimulai
        SetMovement(false);
        RefreshDisplay();
    }

    void Update()
    {
        if (isTransitioning)
        {
            isTransitioning = false;
            return;
        }

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
            StartCoroutine(ProsesFadeLaluPindahScene("BookWorldScene"));
        }
    }

    public void NextLine()
    {
        index++;
        DialogueLine[] currentLines = GetActiveDialogueArray();

        if (index < currentLines.Length)
        {
            RefreshDisplay();
        }
        else
        {
            CloseDialogue();
        }
    }

    DialogueLine[] GetActiveDialogueArray()
    {
        if (jenisDialogAktif == 0) return bookLines;
        if (jenisDialogAktif == 1) return introLines;
        if (jenisDialogAktif == 2) return dialogPanjangLuma;
        if (jenisDialogAktif == 3) return dialogGerbangRusak;
        return introLines;
    }

    void RefreshDisplay()
    {
        DialogueLine[] currentLines = GetActiveDialogueArray();
        if (currentLines == null || currentLines.Length == 0) return;

        if (index < currentLines.Length && dialogueText != null)
        {
            string pembicara = !string.IsNullOrEmpty(currentLines[index].speaker) ? currentLines[index].speaker + ": " : "";
            dialogueText.text = pembicara + currentLines[index].text;
        }
    }

    void CloseDialogue()
    {
        dialogueActive = false;

        if (jenisDialogAktif == 1)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            jenisDialogAktif = 5;

            // UNLOCK: Narasi intro beres, player di kedua scene bebas jalan ke trigger masing-masing!
            SetMovement(true);
            Debug.Log("Sistem: Narasi awal selesai, Player di-UNLOCK.");
        }
        else if (jenisDialogAktif == 2)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (overlaySimbolPecah != null) overlaySimbolPecah.SetActive(true);
            sedangTungguLedakan = true;
        }
        else if (jenisDialogAktif == 3)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            jenisDialogAktif = 4;
            SetMovement(true);
        }
        else if (jenisDialogAktif == 0)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            dialogueActive = false;

            Debug.Log("Sistem: Dialog perpus selesai. Jalankan Jeda Input...");
            StartCoroutine(JedaDanMulaiFade());
        }
    }

    public void MemicuPertemuanLuma()
    {
        SetMovement(false);
        jenisDialogAktif = 2;
        index = 0;
        dialogueActive = true;
        isTransitioning = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshDisplay();
    }

    void MemicuGerbangMacet()
    {
        jenisDialogAktif = 3;
        index = 0;
        dialogueActive = true;
        isTransitioning = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshDisplay();
    }

    public void SetPlayerDiAreaPuzzle(bool diArea)
    {
        playerInTriggerPuzzle = diArea;
    }

    void SetMovement(bool bisaGerak)
    {
        if (playerMovement != null) playerMovement.canMove = bisaGerak;
    }

    public void StartBookDialogue()
    {
        index = 0;
        jenisDialogAktif = 0;
        dialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshDisplay();
        SetMovement(false);
    }

    IEnumerator JedaDanMulaiFade()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        StartCoroutine(ProsesFadeLaluPindahScene("BookWorldScene"));
    }

    IEnumerator ProsesFadeLaluPindahScene(string namaSceneTujuan)
    {
        SetMovement(false);

        if (fadeImage != null)
        {
            Color warnaSekarang = fadeImage.color;
            warnaSekarang.a = 0f;
            fadeImage.color = warnaSekarang;

            while (fadeImage.color.a < 1f)
            {
                warnaSekarang.a += Time.unscaledDeltaTime * kecepatanFade;
                fadeImage.color = warnaSekarang;
                yield return null;
            }

            warnaSekarang.a = 1f;
            fadeImage.color = warnaSekarang;
        }

        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(namaSceneTujuan);
    }
}