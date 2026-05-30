using UnityEngine;
using System.Collections;

public class JigsawManager : MonoBehaviour
{
    [Header("Scramble Settings")]
    [Tooltip("Acak posisi kepingan puzzle secara otomatis saat game dimulai?")]
    public bool scrambleAtStart = true;
    
    [Tooltip("Area Collider2D untuk merandom posisi kepingan puzzle. Jika kosong, akan merandom di sekitar JigsawManager.")]
    public Collider2D scrambleArea;
    
    [Tooltip("Radius horizontal jika tidak ada scrambleArea.")]
    public float scrambleRangeX = 6f;
    
    [Tooltip("Radius vertikal jika tidak ada scrambleArea.")]
    public float scrambleRangeY = 3f;

    [Header("UI & Feedback")]
    [Tooltip("UI Panel yang akan diaktifkan secara otomatis saat puzzle selesai.")]
    public GameObject winUIPanel;

    private JigsawPiece[] pieces;
    private bool puzzleCompleted = false;

    private void Start()
    {
        // Mencari semua kepingan JigsawPiece yang ada di scene secara dinamis
        pieces = FindObjectsByType<JigsawPiece>(FindObjectsSortMode.None);

        if (scrambleAtStart)
        {
            ScramblePieces();
        }

        // Menyembunyikan Win UI di awal game
        if (winUIPanel != null)
        {
            winUIPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!puzzleCompleted)
        {
            CheckPuzzleComplete();
        }
    }

    private void ScramblePieces()
    {
        if (pieces == null || pieces.Length == 0) return;

        foreach (JigsawPiece piece in pieces)
        {
            if (piece == null || piece.isPlaced) continue;

            Vector3 randomPos;
            if (scrambleArea != null)
            {
                // Jika ada area collider, acak posisi di dalam batas collider tersebut
                Bounds bounds = scrambleArea.bounds;
                randomPos = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    piece.transform.position.z
                );
            }
            else
            {
                // Jika tidak ada collider, acak di sekitar posisi JigsawManager
                randomPos = transform.position + new Vector3(
                    Random.Range(-scrambleRangeX, scrambleRangeX),
                    Random.Range(-scrambleRangeY, scrambleRangeY),
                    0f
                );
            }

            // Atur posisi baru untuk kepingan
            piece.transform.position = randomPos;
        }
    }

    private void CheckPuzzleComplete()
    {
        if (pieces == null || pieces.Length == 0) return;

        foreach (JigsawPiece piece in pieces)
        {
            if (piece == null || !piece.isPlaced)
            {
                return; // Masih ada kepingan yang belum pas
            }
        }

        // Jika semua kepingan sudah di posisi masing-masing
        puzzleCompleted = true;
        Debug.Log("LOG_WIN");

        // Aktifkan Win UI Panel jika ada
        if (winUIPanel != null)
        {
            winUIPanel.SetActive(true);
        }

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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
    }
}