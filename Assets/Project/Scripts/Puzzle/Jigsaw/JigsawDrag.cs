// Coded by Fazar - Puzzle Level 1
using UnityEngine;
using UnityEngine.EventSystems;

public class JigsawDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera mainCamera;
    private JigsawPiece piece;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Vector3 offset;

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
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece == null || piece.isPlaced)
            return;

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
            piece.isPlaced = true;
            
            // Disable this script to prevent further dragging of snapped pieces
            enabled = false;
        }
    }
}