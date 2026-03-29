using UnityEngine;
using UnityEditor;

/// <summary>
/// Advanced Rigging Tool to align your sprites perfectly with the combat animations.
/// Select your Player/Enemy and click 'Auto-Rig'.
/// </summary>
public class ModularRiggingTool : EditorWindow
{
    [MenuItem("Tools/Character Rigging Assistant")]
    public static void ShowWindow() => GetWindow<ModularRiggingTool>("Rigging Assistant");

    private GameObject target;

    void OnGUI()
    {
        GUILayout.Label("Character Rigging Assistant", EditorStyles.boldLabel);
        target = (GameObject)EditorGUILayout.ObjectField("Character To Rig", target, typeof(GameObject), true);

        if (GUILayout.Button("Setup Rigging Hierarchy"))
        {
            if (target == null) return;
            SetupHierarchy(target);
        }

        if (GUILayout.Button("Smart Auto-Parent Sprites"))
        {
            if (target == null) return;
            SmartDistributeSprites(target);
        }

        if (GUILayout.Button("Final Auto-Align Transforms"))
        {
            if (target == null) return;
            AlignParts(target);
        }

        EditorGUILayout.HelpBox("1. Parent your sprite parts to the matching child objects.\n2. Ensure the 'Sword' is a child of 'R_Arm'.\n3. Click 'Auto-Align' to center everything.", MessageType.Info);
    }

    void SetupHierarchy(GameObject go)
    {
        Transform t = go.transform;
        
        // Ensure Skin Manager exists
        var skin = go.GetComponent<CharacterSkinManager>() ?? go.AddComponent<CharacterSkinManager>();

        // Create standard skeletal structure
        skin.torso = GetOrCreate(t, "Torso", Vector3.zero);
        skin.head = GetOrCreate(skin.torso.transform, "Head", new Vector3(0, 0.4f, 0));
        skin.l_arm = GetOrCreate(skin.torso.transform, "L_Arm", new Vector3(-0.35f, 0.15f, 0));
        skin.r_arm = GetOrCreate(skin.torso.transform, "R_Arm", new Vector3(0.35f, 0.15f, 0));
        skin.l_leg = GetOrCreate(skin.torso.transform, "L_Leg", new Vector3(-0.15f, -0.4f, 0));
        skin.r_leg = GetOrCreate(skin.torso.transform, "R_Leg", new Vector3(0.15f, -0.4f, 0));
        skin.weapon = GetOrCreate(skin.r_arm.transform, "Sword", new Vector3(0.25f, 0.1f, 0));
        
        skin.weaponPivot = skin.r_arm.transform;

        Debug.Log("Rigging hierarchy setup complete!");
    }

    void AlignParts(GameObject go)
    {
        var skin = go.GetComponent<CharacterSkinManager>();
        if (skin == null) return;

        // Reset all local rotations/positions to ensure pivots are clean
        if (skin.head) skin.head.transform.localPosition = new Vector3(0, 0.4f, 0);
        if (skin.l_arm) skin.l_arm.transform.localPosition = new Vector3(-0.35f, 0.15f, 0);
        if (skin.r_arm) skin.r_arm.transform.localPosition = new Vector3(0.35f, 0.15f, 0);
        if (skin.weapon) skin.weapon.transform.localPosition = new Vector3(0.25f, 0.1f, 0);
        
        Debug.Log("Body parts aligned to standard pivots!");
    }

    void SmartDistributeSprites(GameObject go)
    {
        var skin = go.GetComponent<CharacterSkinManager>();
        if (skin == null) { SetupHierarchy(go); skin = go.GetComponent<CharacterSkinManager>(); }

        SpriteRenderer[] allSrs = go.GetComponentsInChildren<SpriteRenderer>(true);
        Transform root = go.transform;

        foreach (var sr in allSrs)
        {
            // Don't move the target slots themselves
            if (sr == skin.head || sr == skin.torso || sr == skin.l_arm || 
                sr == skin.r_arm || sr == skin.l_leg || sr == skin.r_leg || sr == skin.weapon) continue;

            Vector3 pos = sr.transform.position;
            float relY = pos.y - root.position.y;
            float relX = pos.x - root.position.x;

            // Logic to guess part based on position
            if (relY > 0.6f) sr.transform.SetParent(skin.head.transform);
            else if (relY < -0.2f)
            {
                if (relX < 0) sr.transform.SetParent(skin.l_leg.transform);
                else sr.transform.SetParent(skin.r_leg.transform);
            }
            else
            {
                if (Mathf.Abs(relX) < 0.2f) sr.transform.SetParent(skin.torso.transform);
                else if (relX < 0) sr.transform.SetParent(skin.l_arm.transform);
                else sr.transform.SetParent(skin.r_arm.transform);
            }
            
            // Re-center locally
            sr.transform.localPosition = Vector3.zero;
        }

        Debug.Log("Smart Distribution Complete! Sprites have been moved to their estimated skeletal parents.");
    }

    SpriteRenderer GetOrCreate(Transform parent, string name, Vector3 pos)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            GameObject newGo = new GameObject(name);
            newGo.transform.SetParent(parent);
            child = newGo.transform;
        }
        child.localPosition = pos;
        return child.GetComponent<SpriteRenderer>() ?? child.gameObject.AddComponent<SpriteRenderer>();
    }
}
