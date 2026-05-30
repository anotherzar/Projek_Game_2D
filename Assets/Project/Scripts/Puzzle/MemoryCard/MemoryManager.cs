using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("Container for the pre-placed/fixed prompt cards (e.g., Left side)")]
    public Transform fixedCardsContainer;
    
    [Tooltip("Container for the empty drop slots (e.g., Middle/Right side, aligned with fixed cards)")]
    public Transform slotsContainer;
    
    [Tooltip("Container where draggable cards are spawned and shuffled")]
    public Transform draggablePoolContainer;
    
    [Tooltip("A high-level Canvas transform to parent cards during dragging so they render on top of others")]
    public Transform dragParent;

    [Header("Prefabs")]
    [Tooltip("Prefab for the cards (used for both fixed prompts and draggable cards)")]
    public GameObject memoryCardPrefab;
    
    [Tooltip("Prefab for the empty target slot containing the MemorySlot script")]
    public GameObject slotPrefab;

    [Header("Data")]
    public List<CardData> availableCards;

    [Header("UI Feedback Panels")]
    public GameObject winPanel;

    private int totalPairs;
    private int matchedPairs;

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        InitializeMatchingGame();
    }

    void InitializeMatchingGame()
    {
        if (availableCards == null || availableCards.Count == 0) return;

        totalPairs = availableCards.Count;
        matchedPairs = 0;

        // 1. Spawn Fixed Prompt Cards & matching slots
        // We want the prompts and slots to be spawned in the same order so they align perfectly
        foreach (var data in availableCards)
        {
            // Spawn Fixed Card
            if (memoryCardPrefab != null && fixedCardsContainer != null)
            {
                GameObject fixedCardObj = Instantiate(memoryCardPrefab, fixedCardsContainer);
                MemoryCard card = fixedCardObj.GetComponent<MemoryCard>();
                if (card != null)
                {
                    // Setup as static, using fixedSprite
                    card.SetupStatic(data.cardID, data.fixedSprite);
                }
            }

            // Spawn Empty Slot
            if (slotPrefab != null && slotsContainer != null)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                MemorySlot slot = slotObj.GetComponent<MemorySlot>();
                if (slot != null)
                {
                    slot.Setup(data.cardID, this);
                }
            }
        }

        // 2. Prepare Draggable Cards
        List<CardData> draggableDeck = new List<CardData>(availableCards);
        Shuffle(draggableDeck);

        // Spawn Draggable Cards into the pool
        foreach (var data in draggableDeck)
        {
            if (memoryCardPrefab != null && draggablePoolContainer != null)
            {
                GameObject dragCardObj = Instantiate(memoryCardPrefab, draggablePoolContainer);
                MemoryCard card = dragCardObj.GetComponent<MemoryCard>();
                if (card != null)
                {
                    // Setup as draggable, using cardSprite
                    card.Setup(data.cardID, data.cardSprite, this, dragParent);
                }
            }
        }
    }

    void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void OnCardMatched(int cardID)
    {
        matchedPairs++;
        Debug.Log($"Card matched! ID: {cardID}. Total matched: {matchedPairs}/{totalPairs}");

        if (matchedPairs >= totalPairs)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        if (winPanel != null) winPanel.SetActive(true);
        Debug.Log("Puzzle Complete! All cards matched correctly.");

        // Jalankan transisi scene otomatis setelah 2 detik
        StartCoroutine(AutoLoadNextSceneRoutine());
    }

    private IEnumerator AutoLoadNextSceneRoutine()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level2_Scene2");
    }
}
