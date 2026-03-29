using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Assignment 02 — Fix All Scenes Tool
/// Menu: Tools/Assignment 02/🔧 Fix All Scenes + Build Settings
/// 
/// What this does, in order:
///  1) Registers ALL scenes into Build Settings automatically
///  2) Opens Level1 and injects all required manager GameObjects
///  3) Saves each scene that was changed
/// </summary>
public class FixAllScenesTool : Editor
{
    static readonly string[] SCENE_PATHS = {
        "Assets/Scenes/SplashScreen.unity",
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/LevelSelect.unity",
        "Assets/Scenes/Level1.unity",
        "Assets/Scenes/GameOver.unity"
    };

    // ── Entry Point ────────────────────────────────────────────────────────
    [MenuItem("Tools/Assignment 02/🔧 Fix All Scenes + Build Settings")]
    public static void Run()
    {
        // ── STEP 1: Register scenes in Build Settings ─────────────────────
        var buildScenes = new List<EditorBuildSettingsScene>();
        foreach (var path in SCENE_PATHS)
        {
            if (File.Exists(path))
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            else
                Debug.LogWarning($"[FixTool] Scene not found: {path}");
        }
        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log($"[FixTool] ✅ Registered {buildScenes.Count} scenes in Build Settings.");

        // ── STEP 2: Fix Level1 ────────────────────────────────────────────
        FixLevel1();

        // ── STEP 3: Fix GameOver ──────────────────────────────────────────
        FixGameOver();

        EditorUtility.DisplayDialog("✅ Fix All Scenes Complete",
            $"Build Settings updated with {buildScenes.Count} scenes.\n\n" +
            "Level1 managers verified/injected.\n" +
            "GameOver UI verified.\n\n" +
            "You can now press ▶ Play or use Build Windows EXE.", "OK");
    }

    // ── Fix Level1 Scene ──────────────────────────────────────────────────
    static void FixLevel1()
    {
        string path = "Assets/Scenes/Level1.unity";
        if (!File.Exists(path)) { Debug.LogWarning("[FixTool] Level1.unity not found!"); return; }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(scene);

        int changes = 0;

        // ── GameManager ──────────────────────────────────────────────────
        if (Object.FindObjectOfType<GameManager>() == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            changes++;
            Debug.Log("[FixTool] Added GameManager to Level1.");
        }

        // ── AudioManager ─────────────────────────────────────────────────
        if (Object.FindObjectOfType<AudioManager>() == null)
        {
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
            changes++;
            Debug.Log("[FixTool] Added AudioManager to Level1.");
        }

        // ── EffectsManager ───────────────────────────────────────────────
        if (Object.FindObjectOfType<EffectsManager>() == null)
        {
            var go = new GameObject("EffectsManager");
            go.AddComponent<EffectsManager>();
            changes++;
            Debug.Log("[FixTool] Added EffectsManager to Level1.");
        }

        // ── CameraShake ──────────────────────────────────────────────────
        if (Object.FindObjectOfType<CameraShake>() == null)
        {
            var cam = Camera.main?.gameObject ?? new GameObject("Main Camera");
            if (!cam.GetComponent<CameraShake>()) cam.AddComponent<CameraShake>();
            changes++;
        }

        // ── EventSystem ──────────────────────────────────────────────────
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            changes++;
        }

        // ── Canvas + UIManager (minimum required) ────────────────────────
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            cgo.AddComponent<GraphicRaycaster>();
            changes++;
        }

        if (Object.FindObjectOfType<UIManager>() == null)
        {
            canvas.gameObject.AddComponent<UIManager>();
            changes++;
            Debug.Log("[FixTool] Added UIManager to Canvas in Level1.");
        }

        // ── SceneButtonWirer ─────────────────────────────────────────────
        if (Object.FindObjectOfType<SceneButtonWirer>() == null)
        {
            canvas.gameObject.AddComponent<SceneButtonWirer>();
            changes++;
        }

        // ── EnemySpawner ─────────────────────────────────────────────────
        if (Object.FindObjectOfType<EnemySpawner>() == null)
        {
            var go = new GameObject("EnemySpawner");
            go.AddComponent<EnemySpawner>();
            changes++;
        }

        // ── HealthPackSpawner ────────────────────────────────────────────
        if (Object.FindObjectOfType<HealthPackSpawner>() == null)
        {
            var go = new GameObject("HealthPackSpawner");
            go.AddComponent<HealthPackSpawner>();
            changes++;
        }

        // ── Player tag check ─────────────────────────────────────────────
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogWarning("[FixTool] ⚠️ No GameObject with tag 'Player' found in Level1! Please tag your player.");
        else
        {
            if (!player.GetComponent<HealthSystem>())   { player.AddComponent<HealthSystem>();   changes++; }
            if (!player.GetComponent<PlayerMovement>()) { player.AddComponent<PlayerMovement>(); changes++; }
            if (!player.GetComponent<PlayerCombat>())   { player.AddComponent<PlayerCombat>();   changes++; }
        }

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[FixTool] Level1 fixed with {changes} changes and saved.");
        }
        else
        {
            Debug.Log("[FixTool] Level1 already OK — no changes needed.");
        }

        EditorSceneManager.CloseScene(scene, true);
    }

    // ── Fix GameOver Scene ────────────────────────────────────────────────
    static void FixGameOver()
    {
        string path = "Assets/Scenes/GameOver.unity";
        if (!File.Exists(path)) return;

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(scene);

        int changes = 0;

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            changes++;
        }

        // Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            changes++;
        }

        // Background
        var bg = GetOrCreate(canvas.transform, "GameOverBG");
        var bgImg = bg.GetComponent<Image>() ?? bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.02f, 0.02f, 1f);
        StretchFull(bg.GetComponent<RectTransform>());

        // Title text
        var titleGO = GetOrCreate(canvas.transform, "GameOverTitle");
        var titleTxt = titleGO.GetComponent<Text>() ?? titleGO.AddComponent<Text>();
        titleTxt.text = "GAME OVER";
        titleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleTxt.fontSize = 90;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.9f, 0.08f, 0.08f, 1f);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.60f);
        titleRT.anchorMax = new Vector2(1, 0.82f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Subtitle
        var subGO = GetOrCreate(canvas.transform, "GameOverSub");
        var subTxt = subGO.GetComponent<Text>() ?? subGO.AddComponent<Text>();
        subTxt.text = "Your warrior has fallen.";
        subTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        subTxt.fontSize = 30;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = new Color(0.85f, 0.65f, 0.65f, 1f);
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.1f, 0.50f);
        subRT.anchorMax = new Vector2(0.9f, 0.62f);
        subRT.offsetMin = subRT.offsetMax = Vector2.zero;

        // GameOverController
        if (!canvas.GetComponent<GameOverController>())
        {
            var goc = canvas.gameObject.AddComponent<GameOverController>();
            goc.resultText = subTxt;
            changes++;
        }

        // Restart Button
        MakeButton(canvas.transform, "RestartButton", "▶  PLAY AGAIN",
            new Vector2(0.20f, 0.28f), new Vector2(0.48f, 0.42f), new Color(0.7f, 0.1f, 0.1f));

        // Main Menu Button
        MakeButton(canvas.transform, "MenuButton", "⌂  MAIN MENU",
            new Vector2(0.52f, 0.28f), new Vector2(0.80f, 0.42f), new Color(0.15f, 0.15f, 0.20f));

        // SceneButtonWirer
        if (!canvas.GetComponent<SceneButtonWirer>())
        {
            canvas.gameObject.AddComponent<SceneButtonWirer>();
            changes++;
        }

        if (changes > 0 || true) // always save
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.CloseScene(scene, true);
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    static GameObject GetOrCreate(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bg)
    {
        var btnGO = GetOrCreate(parent, name);
        if (!btnGO.GetComponent<Button>()) btnGO.AddComponent<Button>();
        var img = btnGO.GetComponent<Image>() ?? btnGO.AddComponent<Image>();
        img.color = bg;
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(8, 4); rt.offsetMax = new Vector2(-8, -4);

        var lbl = GetOrCreate(btnGO.transform, "Label");
        var txt = lbl.GetComponent<Text>() ?? lbl.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 28; txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        StretchFull(lbl.GetComponent<RectTransform>());
    }
}
