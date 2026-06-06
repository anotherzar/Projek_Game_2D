using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public static class AutoPlayHandler
{
    static AutoPlayHandler()
    {
        // Tunggu sampai Editor benar-benar siap
        EditorApplication.delayCall += () =>
        {
            // Cek apakah ada argumen -autoplay di terminal
            string[] args = System.Environment.GetCommandLineArgs();
            if (args.Contains("-autoplay"))
            {
                if (!EditorApplication.isPlaying)
                {
                    UnityEngine.Debug.Log("AutoPlay: Mendeteksi argumen -autoplay, menjalankan game...");
                    EditorApplication.isPlaying = true;
                }
            }
        };
    }
}
