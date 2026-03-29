using UnityEngine;
using UnityEditor;

public class ModularCharacterTool : Editor
{
    [MenuItem("Tools/Setup Modular Hierarchy")]
    public static void SetupHierarchy()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("Please select the Player or Enemy GameObject first!");
            return;
        }

        // Add the manager if it doesn't exist
        var skinManager = selected.GetComponent<CharacterSkinManager>();
        if (skinManager == null) skinManager = selected.AddComponent<CharacterSkinManager>();

        // Create parts if they don't exist
        skinManager.head = CreatePart(selected.transform, "Head", new Vector3(0, 0.8f, 0));
        skinManager.torso = CreatePart(selected.transform, "Torso", new Vector3(0, 0.4f, 0));
        skinManager.l_arm = CreatePart(selected.transform, "L_Arm", new Vector3(-0.3f, 0.4f, 0));
        skinManager.r_arm = CreatePart(selected.transform, "R_Arm", new Vector3(0.3f, 0.4f, 0));
        skinManager.l_leg = CreatePart(selected.transform, "L_Leg", new Vector3(-0.15f, 0, 0));
        skinManager.r_leg = CreatePart(selected.transform, "R_Leg", new Vector3(0.15f, 0, 0));

        // Create Weapon slot under R_Arm
        skinManager.weapon = CreatePart(skinManager.r_arm.transform, "Sword", new Vector3(0.2f, 0, 0));
        skinManager.weaponPivot = skinManager.r_arm.transform;

        Debug.Log("Modular Hierarchy Setup Complete for " + selected.name);
    }

    private static SpriteRenderer CreatePart(Transform parent, string name, Vector3 localPos)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            child = go.transform;
        }

        var sr = child.GetComponent<SpriteRenderer>();
        if (sr == null) sr = child.gameObject.AddComponent<SpriteRenderer>();
        
        // Sorting layers should be set appropriately
        sr.sortingOrder = 5; 
        if (name == "Torso") sr.sortingOrder = 4;
        if (name.Contains("L_")) sr.sortingOrder = 3;

        return sr;
    }
}
