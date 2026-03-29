using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically finds and removes all "Missing (Mono Script)" components from the current Scene AND all Prefabs in the Project.
/// </summary>
public class RemoveMissingScriptsTool : Editor
{
    [MenuItem("Tools/Assignment 02/🧹 Remove Missing Scripts")]
    public static void CleanupMissingScripts()
    {
        int totalRemovedCount = 0;
        int objectsAffected = 0;

        // 1. Clean Scene
        GameObject[] sceneObjects = Object.FindObjectsOfType<GameObject>(true);
        foreach (var go in sceneObjects)
        {
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (count > 0)
            {
                totalRemovedCount += count;
                objectsAffected++;
                EditorUtility.SetDirty(go);
            }
        }

        // 2. Clean Prefabs (Asset Database)
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                if (count > 0)
                {
                    totalRemovedCount += count;
                    objectsAffected++;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();

        if (totalRemovedCount > 0)
        {
            Debug.Log($"[Cleanup] Complete! Removed {totalRemovedCount} missing scripts across {objectsAffected} GameObjects/Prefabs.");
            EditorUtility.DisplayDialog("Cleanup Complete", $"Removed {totalRemovedCount} missing scripts from {objectsAffected} objects.\n\nThe warnings should be gone now!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Cleanup Complete", "No missing scripts were found! Your project is clean.", "OK");
        }
    }
}
