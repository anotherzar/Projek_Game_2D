// PetunjukManager.cs
// Bagian dari Fitur Main Menu
using UnityEngine;
using System.Collections;

public class PetunjukManager : MonoBehaviour
{
    public static PetunjukManager instance;
    private string teksPetunjuk = "";

    void Awake() { instance = this; }

    public void Tampilkan(string pesan, float durasi)
    {
        StartCoroutine(TampilkanTeks(pesan, durasi));
    }

    IEnumerator TampilkanTeks(string pesan, float durasi)
    {
        teksPetunjuk = pesan;
        if (durasi > 0)
        {
            yield return new WaitForSeconds(durasi);
            teksPetunjuk = "";
        }
    }

    void OnGUI()
    {
        if (teksPetunjuk != "")
        {
            GUI.skin.label.fontSize = 25;
            GUI.skin.label.normal.textColor = Color.white;
            // Posisi: 20px dari kiri, 20px dari atas
            GUI.Label(new Rect(20, 20, 500, 100), teksPetunjuk);
        }
    }
}