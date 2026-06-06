using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(MainMenuController))]
public class MainMenuSetupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MainMenuController script = (MainMenuController)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Fiks Audio Otomatis (Cari Clip)"))
        {
            SetupAudio(script);
        }
    }

    private void SetupAudio(MainMenuController script)
    {
        Debug.Log("Sedang melakukan pencarian audio asset untuk Main Menu...");

        // Jalur folder audio
        string sfxFolderPath = "Assets/Project/Audio/SFX/Menu";
        string bgmFolderPath = "Assets/Project/Audio/BGM/Visual_Novel";

        // Cari AudioClip
        AudioClip click = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxFolderPath + "/SFX_ButtonClick_MainMenu_VN.mp3");
        AudioClip start = AssetDatabase.LoadAssetAtPath<AudioClip>(sfxFolderPath + "/SFX_Game_Mulai_VN.mp3");
        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>(bgmFolderPath + "/BGM_Main_Menu_VN.mp3");

        Undo.RecordObject(script, "Auto Setup Main Menu Audio");
        script.SetupAudioClips(click, start, bgm);
        EditorUtility.SetDirty(script);
        
        Debug.Log("Berhasil! SFX dan BGM Menu telah dipasang otomatis.");
        
        // Setup AudioSource
        AudioSource source = script.GetComponent<AudioSource>();
        if (source != null)
        {
            source.spatialBlend = 0f;
            source.playOnAwake = false;
            EditorUtility.SetDirty(source);
        }
    }
}
