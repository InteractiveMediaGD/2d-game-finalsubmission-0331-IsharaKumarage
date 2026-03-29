using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildGame : ScriptableObject
{
    [MenuItem("Tools/Assignment 02/📦 Build Windows EXE")]
    public static void BuildWindows()
    {
        string buildFolder = EditorUtility.SaveFolderPanel(
            "Choose Build Output Folder",
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "2D_Game_Assignment_Build");

        if (string.IsNullOrEmpty(buildFolder)) return;

        // Collect only scenes that exist for Assignment 02
        var sceneList = new System.Collections.Generic.List<string>();
        string[] wanted = {
            "Assets/Scenes/SplashScreen.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/GameOver.unity"
        };
        foreach (var s in wanted)
            if (File.Exists(Application.dataPath.Replace("Assets","") + s))
                sceneList.Add(s);

        if (sceneList.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No valid scenes found in Assets/Scenes!", "OK");
            return;
        }

        string exePath = Path.Combine(buildFolder, "2D_Game_Assignment.exe");

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes          = sceneList.ToArray(),
            locationPathName = exePath,
            target          = BuildTarget.StandaloneWindows64,
            options         = BuildOptions.None
        };

        EditorUtility.DisplayDialog("Building...",
            $"Building {sceneList.Count} scenes to:\n{exePath}\n\nThis may take a minute. Click OK to start.",
            "Build Now");

        var report = BuildPipeline.BuildPlayer(opts);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("✅ Build Successful!",
                $"Your game EXE was created at:\n\n{exePath}\n\n" +
                "Zip that folder and submit it!",
                "Open Folder");

            // Open the folder in Windows Explorer
            EditorUtility.RevealInFinder(buildFolder);
        }
        else
        {
            EditorUtility.DisplayDialog("❌ Build Failed",
                $"Build failed: {report.summary.result}\n\nCheck the Console for details.", "OK");
        }
    }
}
