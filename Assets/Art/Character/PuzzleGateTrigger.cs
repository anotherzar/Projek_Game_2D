using UnityEngine;

public class PuzzleGateTrigger : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;

    private void Start()
    {
        // Cari DialogueManager secara otomatis jika lupa dihubungkan di Inspector
        if (dialogueManager == null)
        {
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogueManager != null)
            {
                dialogueManager.SetPlayerDiAreaPuzzle(true);
                Debug.Log("Sistem: Aren berada di dekat gerbang puzzle. Siap menekan E!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogueManager != null)
            {
                dialogueManager.SetPlayerDiAreaPuzzle(false);
                Debug.Log("Sistem: Aren menjauh dari gerbang puzzle.");
            }
        }
    }
}
