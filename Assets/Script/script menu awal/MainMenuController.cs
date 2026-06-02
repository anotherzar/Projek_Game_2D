// MainMenuController.cs
// Created by Rafli - Fitur Main Menu
using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk memanipulasi perpindahan scene

public class MainMenuController : MonoBehaviour
{
    // Fungsi untuk memulai game (dipasang di tombol START JOURNEY)
    public void PlayGame()
    {
        // Memanggil scene perantara bernama "tes" secara langsung menggunakan teks
        // Pastikan ejaan nama scene di dalam tanda petik sama persis dengan nama file scene lo!
        SceneManager.LoadScene("tes");

        Debug.Log("Tombol Start ditekan. Berhasil pindah ke scene perantara (tes).");
    }

    // Fungsi untuk keluar dari game (dipasang di tombol QUIT GAME)
    public void ExitGame()
    {
        // Menutup aplikasi game (hanya aktif jika game sudah di-build menjadi file .exe/.apk)
        Application.Quit();

        // Memunculkan log di Unity Editor untuk memastikan tombol berfungsi saat testing
        Debug.Log("Tombol Quit ditekan. Sistem meminta game untuk ditutup.");
    }
}