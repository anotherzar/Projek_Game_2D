using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class MemoryCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("UI Elements")]
    public GameObject cardBack;
    public GameObject cardFront;
    public Image frontImage;
    public Button button;

    [Header("Card Properties")]
    public int cardID;
    public bool isPlaced = false;

    [Header("SFX Settings")]
    public AudioClip clickSFX;
    public AudioClip dragSFX;
    public AudioClip snapSFX;

    private MemoryManager manager;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    private Vector2 originalPosition;
    private Transform originalParent;
    private Transform tempDragParent;
    private AudioSource audioSource;

    public void Setup(int id, Sprite sprite, MemoryManager mgr, Transform dragParent)
    {
        cardID = id;
        if (frontImage != null)
            frontImage.sprite = sprite;
        manager = mgr;
        tempDragParent = dragParent;
        isPlaced = false;
        
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        ShowFront();
        
        if (button != null)
        {
            button.interactable = false;
        }
    }

    public void SetupStatic(int id, Sprite sprite)
    {
        cardID = id;
        if (frontImage != null)
            frontImage.sprite = sprite;
        isPlaced = true; // Disable dragging
        
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
        
        ShowFront();
        
        if (button != null)
        {
            button.interactable = false;
        }
    }

    public void ShowFront()
    {
        if (cardBack != null) cardBack.SetActive(false);
        if (cardFront != null) cardFront.SetActive(true);
    }

    public void ShowBack()
    {
        if (cardBack != null) cardBack.SetActive(true);
        if (cardFront != null) cardFront.SetActive(false);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlaced) return;
        if (clickSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSFX);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (dragSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(dragSFX);
        }

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        if (tempDragParent != null)
        {
            transform.SetParent(tempDragParent);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (canvas != null && rectTransform != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
        }

        if (!isPlaced)
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
    }

    public void SnapTo(Transform targetSlot)
    {
        isPlaced = true;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (snapSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(snapSFX);
        }

        transform.SetParent(targetSlot);
        
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        StartCoroutine(SmoothSnap(Vector2.zero));
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        
        Vector2 currentPos = rectTransform.anchoredPosition;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, originalPosition, elapsed / duration);
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }

    private IEnumerator SmoothSnap(Vector2 targetPos)
    {
        Vector2 currentPos = rectTransform.anchoredPosition;
        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, targetPos, elapsed / duration);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (clickSFX == null)
            clickSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Project/Audio/SFX/Puzzle_Card/kartu diklik.mp3");
        if (dragSFX == null)
            dragSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Project/Audio/SFX/Puzzle_Card/kartu digeser.mp3");
        if (snapSFX == null)
            snapSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Project/Audio/SFX/Puzzle_Card/kartu dipasang.mp3");
    }
#endif
}
