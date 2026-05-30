#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoSave
{
    private static double saveDelayTime = -1;
    private const double DelayAfterModification = 1.5; // Menunggu 1.5 detik setelah kamu selesai beraktivitas baru melakukan save (mencegah lag saat drag objek)

    static AutoSave()
    {
        // 1. Auto-save sebelum masuk ke Play Mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        
        // 2. Deteksi modifikasi properti objek (misal geser objek, ubah nilai di inspector)
        Undo.postprocessModifications += OnPostprocessModifications;
        
        // 3. Deteksi saat pencet Ctrl+Z atau Ctrl+Y (Undo/Redo)
        Undo.undoRedoPerformed += OnUndoRedoPerformed;

        // 4. Update berkala untuk menjalankan timer save debouncing
        EditorApplication.update += OnUpdate;
    }

    private static void OnUpdate()
    {
        // Jika timer aktif dan waktu tunggu sudah habis, jalankan Save!
        if (saveDelayTime > 0 && EditorApplication.timeSinceStartup >= saveDelayTime)
        {
            saveDelayTime = -1; // Reset timer
            SaveActiveScene();
        }
    }

    private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
    {
        TriggerPendingSave();
        return modifications;
    }

    private static void OnUndoRedoPerformed()
    {
        TriggerPendingSave();
    }

    private static void TriggerPendingSave()
    {
        if (EditorApplication.isPlaying) return;

        // Teknik Debouncing:
        // Setiap ada perubahan baru, waktu tunggu 1.5 detik di-reset kembali ke awal.
        // Jadi Unity baru akan men-save jika kamu sudah diam/selesai melakukan modifikasi selama 1.5 detik.
        // Ini sangat penting supaya Unity kamu TIDAK LAG saat sedang menggeser-geser objek di Scene view!
        saveDelayTime = EditorApplication.timeSinceStartup + DelayAfterModification;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            saveDelayTime = -1; // Batalkan timer pending
            SaveActiveScene();
        }
    }

    private static void SaveActiveScene()
    {
        if (EditorApplication.isPlaying) return;

        var activeScene = EditorSceneManager.GetActiveScene();
        
        // Hanya save jika ada perubahan (Dirty) pada scene aktif
        if (activeScene.isDirty)
        {
            Debug.Log($"<color=#00FF00>[AutoSave]</color> Menyimpan otomatis karena ada perubahan pada scene: '{activeScene.name}' pada {System.DateTime.Now:HH:mm:ss}");
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
