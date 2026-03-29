using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Assignment 02 - Automation Tool
/// This tool automatically configures the current scene with all required scripts 
/// and settings for the assignment with one click.
/// </summary>
public class Assignment02SetupTool : EditorWindow
{
    [MenuItem("Tools/Assignment 02/✨ Implement All Things")]
    public static void ImplementAll()
    {
        int changes = 0;

        // 1. Configure Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (!player.GetComponent<HealthSystem>()) { player.AddComponent<HealthSystem>(); changes++; }
            if (!player.GetComponent<ProjectileLauncher>()) { player.AddComponent<ProjectileLauncher>(); changes++; }
            if (!player.GetComponent<PlayerMovement>()) { player.AddComponent<PlayerMovement>(); changes++; }
            if (!player.GetComponent<PlayerCombat>()) { player.AddComponent<PlayerCombat>(); changes++; }
            
            // Unify Hierarchy
            Selection.activeGameObject = player;
            ModularCharacterTool.SetupHierarchy();
            
            // Fix: Ensure Player's sword targets enemies
            var playerSword = player.GetComponentInChildren<SwordHitDetector>();
            if (playerSword != null) playerSword.targetTag = "Enemy";
            
            var combat = player.GetComponent<PlayerCombat>();
            if (combat != null) combat.enemyLayer = LayerMask.GetMask("Enemy");

            // Attach SwordVisual for a professional-looking sword sprite
            var skinMgr = player.GetComponent<CharacterSkinManager>();
            if (skinMgr != null && skinMgr.weapon != null)
            {
                var sv = skinMgr.weapon.GetComponent<SwordVisual>() ?? skinMgr.weapon.gameObject.AddComponent<SwordVisual>();
                sv.isEnemy = false;
                sv.BuildSword();
                changes++;
            }

            changes++;
            Debug.Log("[Setup] Player components & target tags verified.");
        }

        // 2. Configure UI
        UIManager ui = Object.FindObjectOfType<UIManager>();
        if (ui != null)
        {
            // Add Numeric Health for Player
            if (ui.playerHealthBar != null)
            {
                var numeric = ui.playerHealthBar.GetComponentInChildren<Text>() ?? ui.playerHealthBar.GetComponentInChildren<HealthBarNumeric>()?.GetComponent<Text>();
                if (numeric == null)
                {
                    // Create if missing
                    GameObject go = new GameObject("HealthText");
                    go.transform.SetParent(ui.playerHealthBar.transform);
                    numeric = go.AddComponent<Text>();
                    numeric.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    numeric.alignment = TextAnchor.MiddleCenter;
                    RectTransform rt = go.GetComponent<RectTransform>();
                    rt.localPosition = Vector3.zero;
                    rt.sizeDelta = new Vector2(160, 30);
                }
                
                if (!numeric.GetComponent<HealthBarNumeric>())
                {
                    var script = numeric.gameObject.AddComponent<HealthBarNumeric>();
                    script.targetTag = "Player";
                    changes++;
                }

                // Requirement Creative Mod: Health Bar Gradient
                var fillRect = ui.playerHealthBar.fillRect;
                if (fillRect != null)
                {
                    Image fillImage = fillRect.GetComponent<Image>();
                    if (fillImage != null && !fillImage.GetComponent<HealthBarGradient>())
                    {
                        var gradient = fillImage.gameObject.AddComponent<HealthBarGradient>();
                        gradient.fillImage = fillImage;
                        
                        // Setup default gradient (Green -> Yellow -> Red)
                        gradient.healthGradient = new Gradient();
                        var colors = new GradientColorKey[3];
                        colors[0] = new GradientColorKey(Color.red, 0.0f);
                        colors[1] = new GradientColorKey(Color.yellow, 0.5f);
                        colors[2] = new GradientColorKey(Color.green, 1.0f);
                        
                        var alphas = new GradientAlphaKey[2];
                        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
                        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);
                        
                        gradient.healthGradient.SetKeys(colors, alphas);
                        
                        changes++;
                        Debug.Log("[Setup] HealthBarGradient configured on Player Health Bar.");
                    }
                }
            }

            // Ensure Score Display is there
            if (ui.scoreText != null && !ui.scoreText.GetComponent<ScoreDisplay>())
            {
                ui.scoreText.gameObject.AddComponent<ScoreDisplay>();
                changes++;
            }

            // Requirement Creative Mod: Health Bar Gradient for Enemy
            if (ui.enemyHealthBar != null)
            {
                var fillRect = ui.enemyHealthBar.fillRect;
                if (fillRect != null)
                {
                    Image fillImage = fillRect.GetComponent<Image>();
                    if (fillImage != null && !fillImage.GetComponent<HealthBarGradient>())
                    {
                        var gradient = fillImage.gameObject.AddComponent<HealthBarGradient>();
                        gradient.fillImage = fillImage;
                        gradient.targetTag = "Enemy";

                        gradient.healthGradient = new Gradient();
                        var colors = new GradientColorKey[3];
                        colors[0] = new GradientColorKey(Color.red, 0.0f);
                        colors[1] = new GradientColorKey(Color.yellow, 0.5f);
                        colors[2] = new GradientColorKey(Color.green, 1.0f);
                        var alphas = new GradientAlphaKey[2] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
                        gradient.healthGradient.SetKeys(colors, alphas);

                        changes++;
                    }
                }
            }
            
            Debug.Log("[Setup] UI systems verified/added.");
        }

        // 3. Configure Enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (!e.GetComponent<HealthSystem>()) { e.AddComponent<HealthSystem>(); changes++; }
            if (!e.GetComponent<EnemyAI>()) { e.AddComponent<EnemyAI>(); changes++; }

            // Unify Hierarchy
            Selection.activeGameObject = e;
            ModularCharacterTool.SetupHierarchy();
            
            // Fix: Ensure Enemy's sword targets the player, NOT other enemies
            var enemySword = e.GetComponentInChildren<SwordHitDetector>();
            if (enemySword != null)
            {
                enemySword.targetTag = "Player";
                enemySword.damage    = 50f; // assignment default
            }

            var enemyAI = e.GetComponent<EnemyAI>();
            if (enemyAI != null) enemyAI.playerLayer = LayerMask.GetMask("Player");

            // Distinguish Enemy Visuals (Requirement 5)
            var skin = e.GetComponent<CharacterSkinManager>();
            if (skin != null && skin.torso != null) 
            {
                skin.torso.color = new Color(0.8f, 0.2f, 0.2f); // Dark red torso for enemies
                if (skin.head != null) skin.head.color = new Color(0.5f, 0.1f, 0.1f);

                // Attach SwordVisual (enemy sword = red-tinted)
                if (skin.weapon != null)
                {
                    var sv = skin.weapon.GetComponent<SwordVisual>() ?? skin.weapon.gameObject.AddComponent<SwordVisual>();
                    sv.isEnemy = true;
                    sv.BuildSword();
                }
            }
            changes++;
        }

        // 4. Configure Obstacles
        Obstacle[] obstacles = Object.FindObjectsOfType<Obstacle>();
        foreach (var o in obstacles)
        {
            if (o.GetComponent<Collider2D>() && !o.GetComponent<Collider2D>().isTrigger)
            {
                o.GetComponent<Collider2D>().isTrigger = true;
                changes++;
            }
        }

        // 5. Configure Health Pack Spawner (New Requirement)
        HealthPackSpawner spawner = Object.FindObjectOfType<HealthPackSpawner>();
        if (spawner == null)
        {
            GameObject spawnerGO = new GameObject("HealthPackSpawner");
            spawner = spawnerGO.AddComponent<HealthPackSpawner>();
            changes++;
        }

        // Auto-link prefab if possible
        if (spawner.healthPackPrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("HealthPack t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                spawner.healthPackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                // Ensure HealthPack Prefab has a Collider2D for clicking!
                if (spawner.healthPackPrefab != null)
                {
                    var col = spawner.healthPackPrefab.GetComponent<Collider2D>();
                    if (col == null) spawner.healthPackPrefab.AddComponent<BoxCollider2D>().isTrigger = true;
                }
            }
        }

        // 6. Wall Color Updater (New Creative Requirement)
        WallColorUpdater wallUpdater = Object.FindObjectOfType<WallColorUpdater>();
        if (wallUpdater == null)
        {
            GameObject updaterGO = new GameObject("WallColorUpdater");
            wallUpdater = updaterGO.AddComponent<WallColorUpdater>();
            
            wallUpdater.healthGradient = new Gradient();
            var colors = new GradientColorKey[3];
            colors[0] = new GradientColorKey(new Color(1f, 0.4f, 0.4f), 0.0f); // Soft Red for low health walls
            colors[1] = new GradientColorKey(new Color(1f, 1f, 0.4f), 0.5f);   // Yellow
            colors[2] = new GradientColorKey(new Color(0.4f, 1f, 0.4f), 1.0f); // Soft Green for full health
            var alphas = new GradientAlphaKey[2] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            wallUpdater.healthGradient.SetKeys(colors, alphas);

            changes++;
        }

        // 7. Enemy Spawner (New Requirement)
        EnemySpawner enemySpawner = Object.FindObjectOfType<EnemySpawner>();
        if (enemySpawner == null)
        {
            GameObject spawnerGO = new GameObject("EnemySpawner");
            enemySpawner = spawnerGO.AddComponent<EnemySpawner>();
            changes++;
        }

        // Auto-link prefab if possible
        if (enemySpawner.enemyPrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("Enemy t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                enemySpawner.enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        EditorUtility.DisplayDialog("Assignment 02 Setup", 
            $"Successfully processed {changes} integration changes!\n\n" +
            "1. Player configured with Health/Projectiles.\n" +
            "2. UI configured with Numeric Display & Score.\n" +
            "3. All Enemies & Obstacles updated.\n" +
            "4. Creative mods (Gradient/Flash) integrated.", "Great!");
    }

    [MenuItem("Tools/Assignment 02/📦 Create Build")]
    public static void RunBuild()
    {
        BuildGame.BuildWindows();
    }
}
