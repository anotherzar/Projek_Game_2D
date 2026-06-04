using UnityEngine;
using UnityEngine.InputSystem;

public class BookTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Pengaturan Pose Aren")]
    public GameObject arenWalk;  // Slot untuk Aren yang jalan (Kiri)
    public GameObject arenBaca;  // Slot untuk Aren yang duduk baca buku (Tengah)

    private bool playerDiAreaMeja = false;

    // Fungsi pas Aren masuk ke area meja
    void OnTriggerEnter2D(Collider2D other)
    {
        // Pastiin objek Aren Walk lo di Inspector Tag-nya udah "Player"
        if (other.CompareTag("Player"))
        {
            playerDiAreaMeja = true;
            // Update tips petunjuk
            if (dialogueManager != null) dialogueManager.SetPetunjukInteraksi(true);
            Debug.Log("Sistem: Aren masuk area meja perpus. Siap pencet E.");
        }
    }

    // Fungsi pas Aren keluar dari area meja
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDiAreaMeja = false;
            // Update tips petunjuk
            if (dialogueManager != null) dialogueManager.SetPetunjukInteraksi(false);
            Debug.Log("Sistem: Aren menjauh dari meja perpus.");
        }
    }

    void Update()
    {
        // Deteksi input tombol E pas player ada di dekat meja
        if (playerDiAreaMeja && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Sistem: Tombol E ditekan! Mengubah pose Aren...");

            // 1. Matikan Aren jalan (Kiri), hidupkan Aren baca buku (Tengah)
            if (arenWalk != null) arenWalk.SetActive(false);
            if (arenBaca != null) arenBaca.SetActive(true);

            // 2. Jalankan naskah dialog buku dari DialogueManager
            if (dialogueManager != null)
            {
                dialogueManager.StartBookDialogue();
            }

            // 3. Matikan status biar gak bisa di-spam pencet E berkali-kali
            playerDiAreaMeja = false;
        }
    }
}