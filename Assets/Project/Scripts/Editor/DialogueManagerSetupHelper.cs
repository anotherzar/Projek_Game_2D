using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueManager))]
public class DialogueManagerSetupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DialogueManager script = (DialogueManager)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Fiks Audio Otomatis (Cari BGM & SFX)"))
        {
            SetupAudio(script);
        }
    }

    private void SetupAudio(DialogueManager script)
    {
        Debug.Log("Sedang mencari asset audio untuk DialogueManager...");

        // 1. Cari AudioSource untuk SFX di objek ini
        AudioSource source = script.GetComponent<AudioSource>();
        if (source == null)
        {
            source = script.gameObject.AddComponent<AudioSource>();
        }

        // Assign source SFX
        Undo.RecordObject(script, "Assign Audio Source");
        script.audioSource = source;

        // Paksa setting 2D
        script.audioSource.spatialBlend = 0f;
        script.audioSource.playOnAwake = false;

        // 2. Cari Audio Clips
        string bgmPath = "Assets/Project/Audio/BGM/Visual_Novel";
        string sfxPath = "Assets/Project/Audio/SFX/VN_Level_1";

        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>(bgmPath + "/BGM_Level1_VN.mp3");
        AudioClip retak = script.suaraRetakan;
        AudioClip buku = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Buku_Muncul_VN.mp3");
        AudioClip masuk = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Masuk_Dunia_Buku_VN.mp3");
        AudioClip portal = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Portal_Kebuka_VN.mp3");

        script.SetupAudioClips(bgm, retak, buku, masuk, portal);
        EditorUtility.SetDirty(script);

        Debug.Log("Selesai! BGM dan SFX telah dipasang otomatis. Musik sekarang akan nyambung lintas scene!");
    }
}
