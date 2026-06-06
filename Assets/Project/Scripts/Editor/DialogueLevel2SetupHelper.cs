using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level2DialogueManager))]
public class DialogueLevel2SetupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Level2DialogueManager script = (Level2DialogueManager)target;

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Fiks Audio LEVEL 2 (BGM & Shadowink SFX)"))
        {
            SetupAudioLevel2(script);
        }
        GUI.backgroundColor = Color.white;
    }

    private void SetupAudioLevel2(Level2DialogueManager script)
    {
        Debug.Log("Sedang mencari asset audio untuk Level 2...");

        // 1. Setup AudioSource
        AudioSource source = script.GetComponent<AudioSource>();
        if (source == null) source = script.gameObject.AddComponent<AudioSource>();
        script.audioSource = source;
        script.audioSource.spatialBlend = 0f;
        script.audioSource.playOnAwake = false;

        // 2. Cari Audio Clips Level 2
        string bgmPath = "Assets/Project/Audio/BGM/Visual_Novel";
        string sfxPath = "Assets/Project/Audio/SFX/VN_Level_2";

        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>(bgmPath + "/BGM_Level2_VN.mp3");
        AudioClip s_muncul = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Shadowink_Muncul_VN.mp3");
        AudioClip s_hilang = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Shadowink_Hilang_VN.mp3");
        AudioClip s_terbang = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_BukuTerbang_VN.mp3");

        Undo.RecordObject(script, "Assign Level 2 Audio");
        script.SetupAudioClips(bgm, s_muncul, s_hilang, s_terbang);
        EditorUtility.SetDirty(script);

        Debug.Log("Berhasil! BGM Level 2, Shadowink SFX, dan SFX Buku Terbang telah dipasang.");
    }
}
