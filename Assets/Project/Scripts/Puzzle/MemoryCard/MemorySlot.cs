using UnityEngine;
using UnityEngine.EventSystems;

public class MemorySlot : MonoBehaviour, IDropHandler
{
    public int expectedCardID;
    public bool isFilled = false;
    
    [Header("Visual Feedback (Optional)")]
    public UnityEngine.UI.Image slotImage;
    public Sprite normalSprite;
    public Sprite highlightSprite; // When hovering with drag (optional)

    private MemoryManager manager;

    public void Setup(int id, MemoryManager mgr)
    {
        expectedCardID = id;
        manager = mgr;
        isFilled = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isFilled) return;

        // Find the dragged card
        GameObject draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        MemoryCard dragCard = draggedObj.GetComponent<MemoryCard>();
        if (dragCard != null)
        {
            if (dragCard.cardID == expectedCardID)
            {
                // Correct match!
                isFilled = true;
                dragCard.SnapTo(transform);
                
                if (manager != null)
                {
                    manager.OnCardMatched(expectedCardID);
                }
            }
            else
            {
                // Incorrect match
                Debug.Log($"Incorrect card! Card ID: {dragCard.cardID}, Expected ID: {expectedCardID}");
            }
        }
    }
}
