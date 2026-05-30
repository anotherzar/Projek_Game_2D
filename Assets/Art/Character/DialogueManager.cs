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
    public GameObject petunjukPanel;
    public TextMeshProUGUI petunjukText;

    [Header("Visual Effects")]
    public GameObject overlaySimbolPecah;
    public int indexTriggerKedapKedip = 7;

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

    void Start()
    {
        index = 0;
        dialogueActive = true;
        jenisDialogAktif = 1;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (overlaySimbolPecah != null) overlaySimbolPecah.SetActive(false);
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
            string p = !string.IsNullOrEmpty(lines[index].speaker) ? lines[index].speaker + ": " : "";
            dialogueText.text = p + lines[index].text;

            if (petunjukPanel != null) { petunjukPanel.SetActive(true); petunjukText.text = "Tekan SPACE untuk lanjut"; }

            if (jenisDialogAktif == 2 && index == indexTriggerKedapKedip && overlaySimbolPecah != null)
                StartCoroutine(EfekKedapKedip(overlaySimbolPecah));
        }
    }

    void CloseDialogue()
    {
        dialogueActive = false;
        if (petunjukPanel != null) petunjukPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (jenisDialogAktif == 1) { jenisDialogAktif = 5; SetMovement(true); StartCoroutine(TampilkanPetunjukSementara("Gunakan A & D untuk berjalan!", 3f)); }
        else if (jenisDialogAktif == 2) { sedangTungguLedakan = true; }
        else if (jenisDialogAktif == 3) { jenisDialogAktif = 4; SetMovement(true); }
        else if (jenisDialogAktif == 0) { StartCoroutine(JedaDanMulaiFade()); }
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

    public void SetPlayerDiAreaPuzzle(bool diArea) { playerInTriggerPuzzle = diArea; }
    void SetMovement(bool bisaGerak) { if (playerMovement != null) playerMovement.canMove = bisaGerak; }

    DialogueLine[] GetActiveDialogueArray()
    {
        if (jenisDialogAktif == 0) return bookLines;
        if (jenisDialogAktif == 2) return dialogPanjangLuma;
        if (jenisDialogAktif == 3) return dialogGerbangRusak;
        return introLines;
    }

    IEnumerator TampilkanPetunjukSementara(string pesan, float durasi)
    {
        if (petunjukPanel != null) { petunjukPanel.SetActive(true); petunjukText.text = pesan; yield return new WaitForSeconds(durasi); petunjukPanel.SetActive(false); }
    }

    IEnumerator EfekKedapKedip(GameObject obj)
    {
        Image img = obj.GetComponent<Image>();
        if (img == null) yield break;
        obj.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            for (float a = 0; a <= 1; a += 0.1f) { img.color = new Color(img.color.r, img.color.g, img.color.b, a); yield return new WaitForSeconds(0.05f); }
            for (float a = 1; a >= 0; a -= 0.1f) { img.color = new Color(img.color.r, img.color.g, img.color.b, a); yield return new WaitForSeconds(0.05f); }
        }
    }

    IEnumerator JedaDanMulaiFade() { yield return new WaitForSecondsRealtime(0.2f); StartCoroutine(ProsesFadeLaluPindahScene("BookWorldScene")); }

    IEnumerator ProsesFadeLaluPindahScene(string n) { SetMovement(false); if (fadeImage != null) { Color w = fadeImage.color; w.a = 0; while (w.a < 1) { w.a += Time.unscaledDeltaTime * kecepatanFade; fadeImage.color = w; yield return null; } } yield return new WaitForSecondsRealtime(0.5f); SceneManager.LoadScene(n); }
}