using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// One-click tool to build a professional Game Over screen.
/// Menu: Tools/Assignment 02/🔴 Build Game Over Screen
/// </summary>
public class GameOverUIBuilder : Editor
{
    [MenuItem("Tools/Assignment 02/🔴 Build Game Over Screen")]
    public static void Build()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cgo.AddComponent<GraphicRaycaster>();
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var root = canvas.transform;

        // Background
        var bg = GetOrCreate(root, "Background");
        var bgImg = bg.GetComponent<Image>() ?? bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.02f, 0.02f, 1f);
        StretchFull(bg.GetComponent<RectTransform>());

        // Blood overlay
        var overlay = GetOrCreate(bg.transform, "BloodOverlay");
        var ovImg = overlay.GetComponent<Image>() ?? overlay.AddComponent<Image>();
        ovImg.color = new Color(0.5f, 0f, 0f, 0.25f);
        StretchFull(overlay.GetComponent<RectTransform>());

        // Title
        var title = GetOrCreate(root, "TitleText");
        var titleTxt = title.GetComponent<Text>() ?? title.AddComponent<Text>();
        titleTxt.text = "GAME OVER";
        titleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleTxt.fontSize = 90;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.9f, 0.08f, 0.08f, 1f);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.60f);
        titleRT.anchorMax = new Vector2(1, 0.80f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Subtitle
        var sub = GetOrCreate(root, "SubtitleText");
        var subTxt = sub.GetComponent<Text>() ?? sub.AddComponent<Text>();
        subTxt.text = "Your warrior has fallen.";
        subTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        subTxt.fontSize = 32;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = new Color(0.8f, 0.6f, 0.6f, 1f);
        var subRT = sub.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.1f, 0.50f);
        subRT.anchorMax = new Vector2(0.9f, 0.62f);
        subRT.offsetMin = subRT.offsetMax = Vector2.zero;

        if (!canvas.GetComponent<GameOverController>())
        {
            var goc = canvas.gameObject.AddComponent<GameOverController>();
            goc.resultText = subTxt;
        }

        MakeButton(root, "RestartButton", "▶  PLAY AGAIN",
            new Vector2(0.25f, 0.28f), new Vector2(0.50f, 0.40f), new Color(0.7f, 0.1f, 0.1f));

        MakeButton(root, "MenuButton", "⌂  MAIN MENU",
            new Vector2(0.50f, 0.28f), new Vector2(0.75f, 0.40f), new Color(0.15f, 0.15f, 0.15f));

        EditorUtility.DisplayDialog("✅ Game Over Screen Built",
            "Professional Game Over UI added!\nSceneButtonWirer will wire buttons at runtime.", "OK");
    }

    // Returns GameObject so .AddComponent works
    static GameObject GetOrCreate(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
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

    static void MakeButton(Transform root, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
    {
        var btnGO = GetOrCreate(root, name);
        if (!btnGO.GetComponent<Button>()) btnGO.AddComponent<Button>();
        var img = btnGO.GetComponent<Image>() ?? btnGO.AddComponent<Image>();
        img.color = bgColor;

        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(10, 5);
        rt.offsetMax = new Vector2(-10, -5);

        var lbl = GetOrCreate(btnGO.transform, "Label");
        var txt = lbl.GetComponent<Text>() ?? lbl.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        StretchFull(lbl.GetComponent<RectTransform>());
    }
}
