using UnityEngine;
using UnityEngine.InputSystem; // Wajib ada buat ndeteksi eKey

public class LumaTrigger : MonoBehaviour
{
    private bool playerDiDalamArea = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kalau Aren masuk ke area Luma
        if (other.CompareTag("Player"))
        {
            playerDiDalamArea = true;
            // Update tips petunjuk
            DialogueManager manager = Object.FindFirstObjectByType<DialogueManager>();
            if (manager != null) manager.SetPetunjukInteraksi(true);
            Debug.Log("Sistem: Aren sudah dekat Luma. Silakan tekan E untuk bicara!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Kalau Aren ngejauh/keluar dari area Luma
        if (other.CompareTag("Player"))
        {
            playerDiDalamArea = false;
            // Update tips petunjuk
            DialogueManager manager = Object.FindFirstObjectByType<DialogueManager>();
            if (manager != null) manager.SetPetunjukInteraksi(false);
            Debug.Log("Sistem: Aren menjauh dari Luma.");
        }
    }

    private void Update()
    {
        // Cek tiap frame: Kalau Aren ada di dalam area DAN dia pencet tombol E
        if (playerDiDalamArea && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DialogueManager manager = Object.FindFirstObjectByType<DialogueManager>();

            if (manager != null)
            {
                Debug.Log("Sistem: Tombol E ditekan! Memulai dialog panjang Luma...");
                manager.MemicuPertemuanLuma();

                // Matikan trigger ini biar gak bisa dipencet E lagi setelah obrolan mulai
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Sistem ERROR: DialogueManager gak ketemu di scene!");
            }
        }
    }
}