// Coded by VN Team - Connected to Puzzle Memory
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class Level2DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

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

    void Start()
    {
        currentState = DialogueState.Intro;

        dialoguePanel.SetActive(true);

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
    }

    void EndIntro()
    {
        dialoguePanel.SetActive(false);

        arenRead.SetActive(false);
        arenIdle.SetActive(true);

        playerMovement.canMove = true;
        lumaFollow.canFollow = true;
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
    }

    void StartEnding()
    {
        currentState = DialogueState.Ending;

        StartCoroutine(FadeBackground());

        index = 0;

        DisplayLine(endingLines[index]);
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
            dialogueText.text =
                line.speaker + ": " + line.text;
        }
    }
}