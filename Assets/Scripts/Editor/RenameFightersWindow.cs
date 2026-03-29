using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class RenameFightersWindow : EditorWindow
{
    string playerName = "BROKEN WARRIOR";
    string enemyName = "THE DEMON KING";

    [MenuItem("Tools/Broken Warrior/📛 Change Fighter Names")]
    public static void ShowWindow()
    {
        GetWindow<RenameFightersWindow>("Change Names").Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Change Fighter Names (Updates All Levels)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        playerName = EditorGUILayout.TextField("Player Name (Left):", playerName);
        enemyName = EditorGUILayout.TextField("Enemy Name (Right):", enemyName);

        GUILayout.Space(20);

        if (GUILayout.Button("Update All 3 Levels", GUILayout.Height(40)))
        {
            UpdateNamesInAllLevels(playerName, enemyName);
        }
    }

    static void UpdateNamesInAllLevels(string pName, string eName)
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        string[] levels = {
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity"
        };

        foreach (string lvl in levels)
        {
            if (!System.IO.File.Exists(Application.dataPath.Replace("Assets", "") + lvl)) continue;

            var scene = EditorSceneManager.OpenScene(lvl, OpenSceneMode.Single);
            
            // Find player name text
            GameObject p1Name = GameObject.Find("FightingUI_TopAnchor/P1_Anchor/Name");
            if (p1Name != null && p1Name.GetComponent<Text>() != null)
                p1Name.GetComponent<Text>().text = pName.ToUpper();
                
            // Find enemy name text
            GameObject p2Name = GameObject.Find("FightingUI_TopAnchor/P2_Anchor/Name");
            if (p2Name != null && p2Name.GetComponent<Text>() != null)
                p2Name.GetComponent<Text>().text = eName.ToUpper();

            EditorSceneManager.SaveScene(scene);
        }

        // Return to Level1
        if (System.IO.File.Exists(Application.dataPath.Replace("Assets", "") + levels[0]))
            EditorSceneManager.OpenScene(levels[0], OpenSceneMode.Single);

        EditorUtility.DisplayDialog("✅ Success", $"Names updated!\n\nPlayer: {pName}\nEnemy: {eName}\n\nAll 3 levels were saved.", "OK");
    }
}
