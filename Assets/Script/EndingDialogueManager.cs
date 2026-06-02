using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndingDialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("White Flash")]
    public SpriteRenderer whiteFlash;

    [Header("Dark Effect")]
    public SpriteRenderer darkOverlay;

    [Header("Characters")]
    public GameObject aren;
    public GameObject luma;

    public GameObject lostChildSad;
    public GameObject lostChildHappy;

    [Header("Shadow")]
    public ShadowInkController shadowInk;

    [Header("Background")]
    public GameObject bgCards;
    public GameObject bgLight;

    [Header("Dialogue")]
    public DialogueLine[] introLines;
    public DialogueLine[] memoryLines;
    public DialogueLine[] shadowLines;

    private int index = 0;

    private enum DialogueState
    {
        Intro,
        Memory,
        Shadow
    }

    private DialogueState currentState;

    // =========================
    // UNITY
    // =========================

    void Start()
    {
        InitializeScene();

        StartIntroScene();
    }

    void Update()
    {
        HandleInput();
    }

    // =========================
    // INITIALIZE
    // =========================

    void InitializeScene()
    {
        dialoguePanel.SetActive(true);

        SetupBackground();

        SetupCharacters();

        SetupEffects();
    }

    void SetupBackground()
    {
        bgCards.SetActive(true);

        bgLight.SetActive(false);
    }

    void SetupCharacters()
    {
        lostChildSad.SetActive(true);

        lostChildHappy.SetActive(false);

        shadowInk.gameObject.SetActive(false);
    }

    void SetupEffects()
    {
        // WHITE FLASH

        whiteFlash.gameObject.SetActive(false);

        Color flashColor = whiteFlash.color;

        flashColor.a = 0f;

        whiteFlash.color = flashColor;

        // DARK OVERLAY

        if (darkOverlay != null)
        {
            Color darkColor = darkOverlay.color;

            darkColor.a = 0f;

            darkOverlay.color = darkColor;
        }
    }

    // =========================
    // INPUT
    // =========================

    void HandleInput()
    {
        if (
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            NextDialogue();
        }
    }

    // =========================
    // NEXT DIALOGUE
    // =========================

    void NextDialogue()
    {
        index++;

        switch (currentState)
        {
            case DialogueState.Intro:
                HandleIntroDialogue();
                break;

            case DialogueState.Memory:
                HandleMemoryDialogue();
                break;

            case DialogueState.Shadow:
                HandleShadowDialogue();
                break;
        }
    }

    // =========================
    // INTRO
    // =========================

    void StartIntroScene()
    {
        currentState = DialogueState.Intro;

        index = 0;

        DisplayLine(introLines[index]);
    }

    void HandleIntroDialogue()
    {
        if (index >= introLines.Length)
        {
            StartMemoryScene();

            return;
        }

        DisplayLine(introLines[index]);

        TriggerIntroEvents();
    }

    void TriggerIntroEvents()
    {
        switch (index)
        {
            case 2:

                StartCoroutine(ChangeSceneTransition());

                break;
        }
    }

    // =========================
    // MEMORY
    // =========================

    void StartMemoryScene()
    {
        currentState = DialogueState.Memory;

        index = 0;

        DisplayLine(memoryLines[index]);
    }

    void HandleMemoryDialogue()
    {
        if (index >= memoryLines.Length)
        {
            StartShadowScene();

            return;
        }

        DisplayLine(memoryLines[index]);

        TriggerMemoryEvents();
    }

    void TriggerMemoryEvents()
    {
        switch (index)
        {
            case 3:

                StartCoroutine(FadeOutChild());

                break;
        }
    }

    // =========================
    // SHADOW
    // =========================

    void StartShadowScene()
    {
        currentState = DialogueState.Shadow;

        index = -1;

        NextDialogue();
    }

    void HandleShadowDialogue()
    {
        if (index >= shadowLines.Length)
        {
            EndDialogue();

            return;
        }

        DisplayLine(shadowLines[index]);

        TriggerShadowEvents();
    }

    void TriggerShadowEvents()
    {
        switch (index)
        {
            // Namun tiba-tiba tinta hitam...

            case 0:

                shadowInk.Appear();

                if (darkOverlay != null)
                {
                    StartCoroutine(DarkenBackground());
                }

                break;

            // Suara berat terdengar...

            case 1:

                shadowInk.ChangeAlpha(0.45f);

                break;

            // "Kalian hanya memperlambat..."

            case 2:

                shadowInk.ChangeAlpha(0.8f);

                break;

            // "Apa itu?!"

            case 4:

                shadowInk.StartMoving();

                break;

            // "Dia mulai mendekat."

            case 6:

                shadowInk.FadeOut();

                break;
        }
    }

    // =========================
    // END
    // =========================

    IEnumerator EndAfterFade()
    {
        yield return new WaitForSeconds(2f);

        EndDialogue();
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        Debug.Log("Ending selesai.");
    }

    // =========================
    // WHITE FLASH
    // =========================

    IEnumerator ChangeSceneTransition()
    {
        whiteFlash.gameObject.SetActive(true);

        SpriteRenderer flash = whiteFlash;

        Color color = flash.color;

        color.a = 0f;

        flash.color = color;

        float duration = 1.5f;

        float timer = 0f;

        // FADE IN

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Pow(timer / duration, 1.5f)
                );

            color.a =
                Mathf.Lerp(
                    0f,
                    1f,
                    t
                );

            flash.color = color;

            yield return null;
        }

        // SWITCH BG + CHILD

        bgCards.SetActive(false);

        bgLight.SetActive(true);

        lostChildSad.SetActive(false);

        lostChildHappy.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        timer = 0f;

        // FADE OUT

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    timer / duration
                );

            color.a =
                Mathf.Lerp(
                    1f,
                    0f,
                    t
                );

            flash.color = color;

            yield return null;
        }

        color.a = 0f;

        flash.color = color;

        whiteFlash.gameObject.SetActive(false);
    }

    // =========================
    // CHILD
    // =========================

    IEnumerator FadeOutChild()
    {
        SpriteRenderer child =
            lostChildHappy.GetComponent<SpriteRenderer>();

        Color color = child.color;

        while (color.a > 0)
        {
            color.a -= Time.deltaTime;

            child.color = color;

            yield return null;
        }

        lostChildHappy.SetActive(false);
    }

    // =========================
    // DARK OVERLAY
    // =========================

    IEnumerator DarkenBackground()
    {
        Color color = darkOverlay.color;

        while (color.a < 0.35f)
        {
            color.a += Time.deltaTime * 0.2f;

            darkOverlay.color = color;

            yield return null;
        }
    }

    // =========================
    // DISPLAY
    // =========================

    void DisplayLine(DialogueLine line)
    {
        if (line.speaker == "")
        {
            dialogueText.text = line.text;
        }
        else
        {
            dialogueText.text =
                line.speaker + ": " + line.text;
        }
    }
}