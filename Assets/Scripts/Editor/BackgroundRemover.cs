#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Tools → Broken Warrior → 🎨 Remove Background Color
/// Three modes to handle any sprite type:
///   1. Color Key   – removes pixels matching a specific color (e.g. white, magenta)
///   2. Dark-to-Opaque – for BLACK silhouettes on light bg: dark=opaque, bright=transparent
///   3. Invert Alpha – if alpha is already present but inverted (Unity shows inverted result)
/// </summary>
public class BackgroundRemover : EditorWindow
{
    private enum RemoveMode
    {
        ColorKey,          // Remove specific background color
        DarkToOpaque,      // Black silhouette on light bg → make dark=opaque, bright=transparent
        InvertAlpha        // Existing alpha channel is inverted → flip it
    }

    private static string     targetPath  = "Assets/Art/Characters/FighterSpriteSheet.png";
    private static Color      bgColor     = Color.white;
    private static float      tolerance   = 0.15f;
    private static bool       autoDetect  = true;
    private static RemoveMode mode        = RemoveMode.DarkToOpaque;  // best for black silhouettes

    [MenuItem("Tools/Broken Warrior/🎨 Remove Background Color")]
    public static void ShowWindow()
    {
        var w = GetWindow<BackgroundRemover>("Remove BG Color");
        w.minSize = new Vector2(420, 360);
        DetectBackgroundColor();
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("🎨 Background Color Remover", EditorStyles.boldLabel);
        GUILayout.Space(6);

        // File picker
        EditorGUILayout.BeginHorizontal();
        targetPath = EditorGUILayout.TextField("Sprite Sheet", targetPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string full = EditorUtility.OpenFilePanel("Select Sprite Sheet", "Assets", "png");
            if (!string.IsNullOrEmpty(full))
            {
                targetPath = "Assets" + full.Substring(Application.dataPath.Length);
                DetectBackgroundColor();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ── Mode chooser ─────────────────────────────────────────────────
        GUILayout.Label("Removal Mode", EditorStyles.boldLabel);
        mode = (RemoveMode)GUILayout.SelectionGrid((int)mode, new[]
        {
            "1. Color Key\n(remove one specific color)",
            "2. Dark-to-Opaque ⭐\n(black silhouette on white/light bg)",
            "3. Invert Alpha\n(alpha channel is flipped)"
        }, 1, GUILayout.Height(90));

        GUILayout.Space(8);

        // ── Mode-specific settings ────────────────────────────────────────
        if (mode == RemoveMode.ColorKey)
        {
            EditorGUILayout.HelpBox(
                "Removes all pixels that match the chosen background color.\n" +
                "Use this when your background is a flat solid color (e.g. magenta, blue).",
                MessageType.Info);

            autoDetect = EditorGUILayout.Toggle("Auto-detect from corner pixel", autoDetect);
            if (!autoDetect)
                bgColor = EditorGUILayout.ColorField("Background Color", bgColor);
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ColorField("Detected BG Color", bgColor);
                EditorGUI.EndDisabledGroup();
            }
            tolerance = EditorGUILayout.Slider("Tolerance", tolerance, 0f, 0.5f);
        }
        else if (mode == RemoveMode.DarkToOpaque)
        {
            EditorGUILayout.HelpBox(
                "⭐ RECOMMENDED for black silhouettes on white/light backgrounds.\n\n" +
                "Dark pixels   → fully opaque  (character body stays visible)\n" +
                "Bright pixels → fully transparent  (background removed)\n\n" +
                "This also fixes inverted-alpha issues caused by AI generators.",
                MessageType.Info);

            tolerance = EditorGUILayout.Slider(
                "Edge softness (0=sharp  0.3=soft)", tolerance, 0f, 0.5f);
        }
        else // InvertAlpha
        {
            EditorGUILayout.HelpBox(
                "Flips the alpha channel.\n" +
                "Use this if the sprite already has an alpha channel but it is inverted\n" +
                "(i.e. the background is opaque and the character is transparent).",
                MessageType.Warning);
        }

        GUILayout.Space(12);

        if (GUILayout.Button("🔍 Preview – pixel count"))
            Preview();

        GUILayout.Space(4);

        GUI.backgroundColor = new Color(0.85f, 0.2f, 0.2f);
        if (GUILayout.Button("✂  Apply & Save PNG", GUILayout.Height(36)))
            Apply();
        GUI.backgroundColor = Color.white;

        GUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "⚠ Overwrites the PNG in Assets/. Keep a backup if unsure.",
            MessageType.Warning);
    }

    // ── Auto corner-pixel detection ───────────────────────────────────────
    static void DetectBackgroundColor()
    {
        if (!autoDetect) return;
        var tex = LoadRaw(targetPath);
        if (tex == null) return;
        bgColor = tex.GetPixel(0, tex.height - 1);
        Object.DestroyImmediate(tex);
    }

    // ── Preview ───────────────────────────────────────────────────────────
    static void Preview()
    {
        if (mode == RemoveMode.ColorKey && autoDetect) DetectBackgroundColor();
        var tex = LoadRaw(targetPath);
        if (tex == null) { Debug.LogError("Cannot load: " + targetPath); return; }

        Color[] px = tex.GetPixels();
        int count = 0;
        foreach (var p in px)
        {
            if      (mode == RemoveMode.ColorKey)    { if (ColorDist(p, bgColor) <= tolerance) count++; }
            else if (mode == RemoveMode.DarkToOpaque){ if (Luminance(p) >= (1f - tolerance))  count++; }
            else                                     { count = px.Length; }   // all pixels flipped
        }
        Object.DestroyImmediate(tex);
        EditorUtility.DisplayDialog("Preview",
            $"Mode: {mode}\n{count:N0} pixels will be affected.", "OK");
    }

    // ── Main apply ────────────────────────────────────────────────────────
    static void Apply()
    {
        if (mode == RemoveMode.ColorKey && autoDetect) DetectBackgroundColor();

        var tex = LoadRaw(targetPath);
        if (tex == null) { Debug.LogError("Cannot load: " + targetPath); return; }

        Color[] px = tex.GetPixels();
        int changed = 0;

        for (int i = 0; i < px.Length; i++)
        {
            Color p = px[i];
            switch (mode)
            {
                case RemoveMode.ColorKey:
                    if (ColorDist(p, bgColor) <= tolerance)
                    { px[i] = Color.clear; changed++; }
                    break;

                case RemoveMode.DarkToOpaque:
                    // Luminance: 0=black (keep solid), 1=white (remove)
                    float lum   = Luminance(p);
                    float alpha = 1f - Mathf.SmoothStep(
                        1f - tolerance * 2f, 1f, lum);   // smooth falloff at edges
                    // Recolor pixel to pure black so the silhouette looks crisp
                    px[i] = new Color(0f, 0f, 0f, alpha);
                    if (alpha < 0.98f) changed++;
                    break;

                case RemoveMode.InvertAlpha:
                    px[i] = new Color(p.r, p.g, p.b, 1f - p.a);
                    changed++;
                    break;
            }
        }

        tex.SetPixels(px);
        tex.Apply();

        // Write PNG
        string fullPath = Application.dataPath.Replace("Assets", "") + targetPath;
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        // Re-import with correct settings
        TextureImporter ti = AssetImporter.GetAtPath(targetPath) as TextureImporter;
        if (ti != null)
        {
            ti.alphaIsTransparency = true;
            ti.alphaSource         = TextureImporterAlphaSource.FromInput;
            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Multiple;
            ti.filterMode          = FilterMode.Point;
            ti.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ [{mode}] Processed {changed:N0} pixels in {targetPath}");

        EditorUtility.DisplayDialog("✅ Done!",
            $"Mode: {mode}\n{changed:N0} pixels processed.\n\n" +
            "Your sprite sheet now has correct transparency!\n" +
            "Run 🚀 Setup Full Game to apply to the scene.", "Great!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static Texture2D LoadRaw(string path)
    {
        string full = Application.dataPath.Replace("Assets", "") + path;
        if (!File.Exists(full)) { Debug.LogError("Not found: " + full); return null; }
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(File.ReadAllBytes(full));
        return tex;
    }

    static float ColorDist(Color a, Color b) =>
        Mathf.Sqrt((a.r-b.r)*(a.r-b.r) + (a.g-b.g)*(a.g-b.g) + (a.b-b.b)*(a.b-b.b)) / 1.732f;

    static float Luminance(Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
}
#endif
