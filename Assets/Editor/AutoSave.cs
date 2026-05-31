using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    [InitializeOnLoad]
    public static class AutoSave
    {
        private const string PrefsPrefix = "UnityAutoSave_";
        private const string EnabledKey = PrefsPrefix + "Enabled";
        private const string IntervalKey = PrefsPrefix + "IntervalMinutes";
        private const string LogKey = PrefsPrefix + "LogEnabled";

        private static bool s_Enabled = true;
        private static int s_IntervalMinutes = 5;
        private static bool s_LogEnabled = false; // Default to false as requested

        private static double s_NextSaveTime;

        static AutoSave()
        {
            LoadSettings();
            ResetTimer();
            EditorApplication.update += OnUpdate;
        }

        public static bool Enabled
        {
            get => s_Enabled;
            set
            {
                if (s_Enabled != value)
                {
                    s_Enabled = value;
                    EditorPrefs.SetBool(EnabledKey, value);
                    ResetTimer();
                }
            }
        }

        public static int IntervalMinutes
        {
            get => s_IntervalMinutes;
            set
            {
                int val = Mathf.Clamp(value, 1, 60);
                if (s_IntervalMinutes != val)
                {
                    s_IntervalMinutes = val;
                    EditorPrefs.SetInt(IntervalKey, val);
                    ResetTimer();
                }
            }
        }

        public static bool LogEnabled
        {
            get => s_LogEnabled;
            set
            {
                if (s_LogEnabled != value)
                {
                    s_LogEnabled = value;
                    EditorPrefs.SetBool(LogKey, value);
                }
            }
        }

        public static void ResetTimer()
        {
            s_NextSaveTime = EditorApplication.timeSinceStartup + (s_IntervalMinutes * 60);
        }

        private static void LoadSettings()
        {
            s_Enabled = EditorPrefs.GetBool(EnabledKey, true);
            s_IntervalMinutes = EditorPrefs.GetInt(IntervalKey, 5);
            s_LogEnabled = EditorPrefs.GetBool(LogKey, false);
        }

        private static void OnUpdate()
        {
            if (!s_Enabled) return;

            // Do not save when playing or compiling
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                ResetTimer();
                return;
            }

            if (EditorApplication.timeSinceStartup >= s_NextSaveTime)
            {
                ExecuteAutoSave();
                ResetTimer();
            }
        }

        public static void ExecuteAutoSave()
        {
            bool anySceneDirty = false;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    anySceneDirty = true;
                    break;
                }
            }

            if (anySceneDirty)
            {
                if (s_LogEnabled)
                {
                    Debug.Log($"[AutoSave] Saving dirty scenes at {DateTime.Now:HH:mm:ss}...");
                }
                EditorSceneManager.SaveOpenScenes();
            }

            // Save dirty assets
            AssetDatabase.SaveAssets();

            if (s_LogEnabled && anySceneDirty)
            {
                Debug.Log("[AutoSave] Auto-save complete.");
            }
        }
    }

    public class AutoSaveSettingsWindow : EditorWindow
    {
        [MenuItem("Tools/AutoSave Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoSaveSettingsWindow>("AutoSave Settings");
            window.minSize = new Vector2(300, 150);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity AutoSave Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            AutoSave.Enabled = EditorGUILayout.Toggle("Enable AutoSave", AutoSave.Enabled);
            AutoSave.IntervalMinutes = EditorGUILayout.IntSlider("Interval (Minutes)", AutoSave.IntervalMinutes, 1, 60);
            AutoSave.LogEnabled = EditorGUILayout.Toggle("Enable Console Logs", AutoSave.LogEnabled);

            EditorGUILayout.Space();
            if (GUILayout.Button("Save Now", GUILayout.Height(30)))
            {
                AutoSave.ExecuteAutoSave();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Autosave operates when the editor is idle (not in Play mode).", EditorStyles.miniLabel);
        }
    }
}
