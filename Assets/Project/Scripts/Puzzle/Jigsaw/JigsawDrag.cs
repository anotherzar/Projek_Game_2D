using UnityEngine;
using UnityEngine.EventSystems;

public class JigsawDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Scale Settings")]
    [Tooltip("Scale factor when the piece is not being dragged.")]
    public float idleScaleFactor = 0.7f;
    [Tooltip("Scale factor when the piece is being dragged.")]
    public float dragScaleFactor = 1.0f;
    [Tooltip("How fast the piece transitions between scales.")]
    public float scaleSpeed = 12f;

    [Header("SFX Settings")]
    [Tooltip("SFX played when the piece is picked up / dragged.")]
    public AudioClip dragSFX;
    [Tooltip("SFX played when the piece successfully snaps to its target point.")]
    public AudioClip snapSFX;

    private Camera mainCamera;
    private JigsawPiece piece;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Vector3 offset;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isDragging = false;
    private AudioSource audioSource;

    private void Start()
    {
        mainCamera = Camera.main;
        piece = GetComponent<JigsawPiece>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        // Automatically ensure the Main Camera has a Physics 2D Raycaster so that 2D click/drag events work!
        if (mainCamera != null && mainCamera.GetComponent<Physics2DRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }

        if (piece == null)
        {
            Debug.LogError($"JigsawPiece component missing on {gameObject.name}!", this);
            return;
        }

        // Set up AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Set to 2D sound for UI/puzzle SFX
        }

        originalScale = transform.localScale;

        if (piece.isPlaced)
        {
            targetScale = originalScale;
            transform.localScale = targetScale;
            enabled = false; // Disable script if already placed
        }
        else
        {
            targetScale = originalScale * idleScaleFactor;
            transform.localScale = targetScale;
        }
    }

    private void Update()
    {
        if (piece == null) return;

        if (piece.isPlaced)
        {
            targetScale = originalScale;
        }
        else if (isDragging)
        {
            targetScale = originalScale * dragScaleFactor;
        }
        else
        {
            targetScale = originalScale * idleScaleFactor;
        }

        // Smoothly interpolate the scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece == null || piece.isPlaced)
            return;

        isDragging = true;

        // Play drag SFX
        if (dragSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(dragSFX);
        }

        // Bring the piece to the front while dragging for premium visual feel
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder + 100;
        }

        Vector3 mouseWorldPos = GetWorldPos(eventData.position);
        offset = transform.position - mouseWorldPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (piece == null || piece.isPlaced)
            return;

        Vector3 mouseWorldPos = GetWorldPos(eventData.position);
        
        // Retain current Z coordinate
        mouseWorldPos.z = transform.position.z;
        transform.position = mouseWorldPos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (piece == null || piece.isPlaced)
            return;

        isDragging = false;

        // Restore original sorting order
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        CheckSnap();
    }

    private Vector3 GetWorldPos(Vector2 screenPos)
    {
        if (mainCamera == null)
            return Vector3.zero;

        // Calculate the correct Z depth distance from the camera
        float zDistance = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        Vector3 mousePos3D = new Vector3(screenPos.x, screenPos.y, zDistance);
        
        return mainCamera.ScreenToWorldPoint(mousePos3D);
    }

    private void CheckSnap()
    {
        if (piece.targetPoint == null)
        {
            Debug.LogWarning($"TargetPoint is not assigned on JigsawPiece for {gameObject.name}!", this);
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            piece.targetPoint.position
        );

        if (distance <= piece.snapDistance)
        {
            transform.position = piece.targetPoint.position;
            transform.localScale = originalScale; // Force exact original scale on snapping
            piece.isPlaced = true;

            // Play snap SFX
            if (snapSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(snapSFX);
            }
            
            // Disable this script to prevent further dragging of snapped pieces
            enabled = false;
        }
    }
}