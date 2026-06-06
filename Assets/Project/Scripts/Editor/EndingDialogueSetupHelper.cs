using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EndingDialogueManager))]
public class EndingDialogueSetupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EndingDialogueManager script = (EndingDialogueManager)target;

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Fiks Audio ENDING (BGM & SFX)"))
        {
            SetupAudioEnding(script);
        }
        GUI.backgroundColor = Color.white;
    }

    private void SetupAudioEnding(EndingDialogueManager script)
    {
        Debug.Log("Sedang mencari asset audio untuk Ending Scene...");

        // 1. Setup AudioSource
        AudioSource source = script.GetComponent<AudioSource>();
        if (source == null) source = script.gameObject.AddComponent<AudioSource>();
        script.audioSource = source;
        script.audioSource.spatialBlend = 0f;
        script.audioSource.playOnAwake = false;

        // 2. Cari Audio Clips
        string bgmPath = "Assets/Project/Audio/BGM/Visual_Novel";
        string sfxPath = "Assets/Project/Audio/SFX/VN_Level_2";

        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>(bgmPath + "/BGM_Level2_VN.mp3");
        AudioClip s_muncul = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Shadowink_Muncul_VN.mp3");
        AudioClip s_hilang = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxPath + "/SFX_Shadowink_Hilang_VN.mp3");

        Undo.RecordObject(script, "Assign Ending Audio");
        script.SetupAudioClips(bgm, s_muncul, s_hilang);
        EditorUtility.SetDirty(script);

        Debug.Log("Berhasil! BGM dan SFX untuk Ending Scene telah dipasang. Musik akan nyambung dari Level 2!");
    }
}
