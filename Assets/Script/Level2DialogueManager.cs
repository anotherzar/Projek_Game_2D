using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI; // for Image UI components

public class Level2DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Petunjuk Gambar References")]
    public GameObject petunjukSpaceImage;   // Gambar untuk petunjuk Space (saat dialog)
    public GameObject petunjukMoveImage;    // Gambar untuk petunjuk A/D (saat bergerak)
    public GameObject petunjukInteractImage;// Gambar untuk petunjuk E (saat interaksi)
    // --- New name image slots ---
    public Image namaImageAren; // Image with name "Aren"
    public Image namaImageLuma; // Image with name "Luma"
// Duplicate declarations removed
// Duplicate declarations removed
    public Image namaImageAnakKecil; // Image with name "Anak_Kecil"
    public Image namaImageShadowInk; // Image with name "Shadow_Ink"
    public Image namaImageChild; // Image with name "Child"

    [Header("Characters")]
    public GameObject arenRead;
    public GameObject arenIdle;
    public GameObject luma;
    public GameObject lostChild;

    [Header("Background")]
    public GameObject bgNormal;
    public GameObject bgFragments;

    [Header("Movement")]
    public PlayerMovement playerMovement;
    public LumaFollow lumaFollow;

    [Header("Intro Dialogue")]
    public DialogueLine[] introLines;

    [Header("Child Dialogue")]
    public DialogueLine[] childLines;

    [Header("Ending Dialogue")]
    public DialogueLine[] endingLines;

    private int index = 0;

    private enum DialogueState
    {
        Intro,
        ChildScene,
        Ending
    }

    private DialogueState currentState;

    void Awake()
    {
        // Fallback find images by name if not assigned
        if (namaImageAren == null) {
            var go = GameObject.Find("NamaBox_Aren");
            if (go != null) namaImageAren = go.GetComponent<Image>();
        }
        if (namaImageLuma == null) {
            var go = GameObject.Find("NamaBox_Luma");
            if (go != null) namaImageLuma = go.GetComponent<Image>();
        }
        if (namaImageAnakKecil == null) {
            var go = GameObject.Find("NamaBox_AnakKecil");
            if (go != null) namaImageAnakKecil = go.GetComponent<Image>();
        }
        if (namaImageShadowInk == null) {
            var go = GameObject.Find("NamaBox_ShadowInk");
            if (go != null) namaImageShadowInk = go.GetComponent<Image>();
        }
        if (namaImageChild == null) {
            var go = GameObject.Find("NamaBox_Child");
            if (go != null) namaImageChild = go.GetComponent<Image>();
        }
    }

    void Start()
    {
        currentState = DialogueState.Intro;

        dialoguePanel.SetActive(true);

        // Hide all name images at start
        if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
        if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
        if (namaImageAnakKecil != null) namaImageAnakKecil.gameObject.SetActive(false);
        if (namaImageShadowInk != null) namaImageShadowInk.gameObject.SetActive(false);
        if (namaImageChild != null) namaImageChild.gameObject.SetActive(false);

        bgNormal.SetActive(true);
        bgFragments.SetActive(false);

        arenRead.SetActive(true);
        arenIdle.SetActive(false);

        // ANAK KECIL HILANG DULU
        lostChild.SetActive(false);

        playerMovement.canMove = false;
        lumaFollow.canFollow = false;

        index = 0;

        DisplayLine(introLines[index]);
        UpdatePetunjuk();
    }

    void Update()
    {
        if (
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            NextDialogue();
        }
    }

    void NextDialogue()
    {
        index++;

        if (currentState == DialogueState.Intro)
        {
            if (index < introLines.Length)
            {
                DisplayLine(introLines[index]);
            }
            else
            {
                EndIntro();
            }
        }
        else if (currentState == DialogueState.ChildScene)
        {
            if (index < childLines.Length)
            {
                DisplayLine(childLines[index]);
            }
            else
            {
                StartEnding();
            }
        }
        else if (currentState == DialogueState.Ending)
        {
            if (index < endingLines.Length)
            {
                DisplayLine(endingLines[index]);
            }
            else
            {
                dialoguePanel.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.LoadScene("Puzzle_Memory");
            }
        }
        UpdatePetunjuk();
    }

    void EndIntro()
    {
        dialoguePanel.SetActive(false);

        arenRead.SetActive(false);
        arenIdle.SetActive(true);

        playerMovement.canMove = true;
        lumaFollow.canFollow = true;
        // Explicitly hide all name boxes after intro
        if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
        if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
        if (namaImageAnakKecil != null) namaImageAnakKecil.gameObject.SetActive(false);
        if (namaImageShadowInk != null) namaImageShadowInk.gameObject.SetActive(false);
        if (namaImageChild != null) namaImageChild.gameObject.SetActive(false);
        // After intro, hide any active name boxes
        UpdateNameImages("");
    }

    public void TriggerChildScene()
    {
        StartCoroutine(StartChildScene());
    }

    IEnumerator StartChildScene()
    {
        currentState = DialogueState.ChildScene;

        // STOP GERAK
        playerMovement.canMove = false;
        lumaFollow.canFollow = false;

        // MUNCULKAN ANAK KECIL
        lostChild.SetActive(true);

        // FADE IN
        lostChild.GetComponent<LostChildFade>().StartFade();

        yield return new WaitForSeconds(0.5f);

        // FLIP AREN
        arenIdle.transform.localScale = new Vector3(
            -Mathf.Abs(arenIdle.transform.localScale.x),
            arenIdle.transform.localScale.y,
            arenIdle.transform.localScale.z
        );

        // FLIP LUMA
        luma.transform.localScale = new Vector3(
            -Mathf.Abs(luma.transform.localScale.x),
            luma.transform.localScale.y,
            luma.transform.localScale.z
        );

        yield return new WaitForSeconds(1f);

        dialoguePanel.SetActive(true);

        index = 0;

        DisplayLine(childLines[index]);
        UpdatePetunjuk();
    }

    void StartEnding()
    {
        currentState = DialogueState.Ending;

        StartCoroutine(FadeBackground());

        index = 0;

        DisplayLine(endingLines[index]);
        UpdatePetunjuk();
    }

    IEnumerator FadeBackground()
    {
        SpriteRenderer normal =
            bgNormal.GetComponent<SpriteRenderer>();

        SpriteRenderer fragment =
            bgFragments.GetComponent<SpriteRenderer>();

        bgFragments.SetActive(true);

        Color normalColor = normal.color;
        Color fragmentColor = fragment.color;

        // AWAL TRANSPARAN
        fragmentColor.a = 0;
        fragment.color = fragmentColor;

        // EFEK GLITCH / KASAR
        for (int i = 0; i < 6; i++)
        {
            fragmentColor.a = Random.Range(0.2f, 0.8f);
            normalColor.a = Random.Range(0.3f, 1f);

            fragment.color = fragmentColor;
            normal.color = normalColor;

            yield return new WaitForSeconds(0.08f);
        }

        // AKHIR FULL FRAGMENT
        fragmentColor.a = 1f;
        fragment.color = fragmentColor;

        normalColor.a = 0f;
        normal.color = normalColor;

        bgNormal.SetActive(false);
    }

    void DisplayLine(DialogueLine line)
    {
        if (line.speaker == "")
        {
            dialogueText.text = line.text;
        }
        else
        {
            dialogueText.text = line.speaker + ": " + line.text;
        }
        UpdateNameImages(line.speaker);
    }

    void UpdateNameImages(string speaker)
    {
        // Hide all first
        if (namaImageAren != null) namaImageAren.gameObject.SetActive(false);
        if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(false);
        if (namaImageAnakKecil != null) namaImageAnakKecil.gameObject.SetActive(false);
        if (namaImageShadowInk != null) namaImageShadowInk.gameObject.SetActive(false);

        string spk = (speaker ?? "").Trim().ToLower();
        if (spk == "aren")
        {
            if (namaImageAren != null) namaImageAren.gameObject.SetActive(true);
        }
        else if (spk == "luma")
        {
            if (namaImageLuma != null) namaImageLuma.gameObject.SetActive(true);
        }
        else if (spk == "anak_kecil")
        {
            if (namaImageAnakKecil != null) namaImageAnakKecil.gameObject.SetActive(true);
        }
        else if (spk == "child")
        {
            if (namaImageChild != null) namaImageChild.gameObject.SetActive(true);
        }
        else if (spk == "shadow_ink")
        {
            if (namaImageShadowInk != null) namaImageShadowInk.gameObject.SetActive(true);
        }
    }

    // === SISTEM PETUNJUK/TIPS ===
    private bool playerDiAreaInteraksi = false;

    public void SetPetunjukInteraksi(bool diArea)
    {
        playerDiAreaInteraksi = diArea;
        UpdatePetunjuk();
    }

    void UpdatePetunjuk()
    {
        // 1. Matikan semua petunjuk gambar terlebih dahulu
        if (petunjukSpaceImage != null) petunjukSpaceImage.SetActive(false);
        if (petunjukMoveImage != null) petunjukMoveImage.SetActive(false);
        if (petunjukInteractImage != null) petunjukInteractImage.SetActive(false);

        // 2. Tentukan state petunjuk yang aktif
        bool dialogAktif = dialoguePanel != null && dialoguePanel.activeSelf;
        bool interactAktif = !dialogAktif && playerDiAreaInteraksi;
        bool moveAktif = !dialogAktif && !interactAktif && (playerMovement != null && playerMovement.canMove);

        // 3. Aktifkan petunjuk gambar sesuai state
        if (dialogAktif)
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
    }
}