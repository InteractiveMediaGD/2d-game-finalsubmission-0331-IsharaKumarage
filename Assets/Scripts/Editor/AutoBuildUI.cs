using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoBuildUI : ScriptableObject
{
    [MenuItem("Tools/Fix My Missing UI (Click Me!)")]
    public static void GenerateFullUI()
    {
        // 1. Create EventSystem if missing
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // 2. Create Canvas if missing
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 3. Create UIManager
        UIManager uiMgr = Object.FindObjectOfType<UIManager>();
        if (uiMgr == null)
        {
            GameObject mgrObj = new GameObject("UIManager");
            uiMgr = mgrObj.AddComponent<UIManager>();
        }

        // 4. Create Score Text
        if (uiMgr.scoreText == null)
        {
            GameObject txtObj = new GameObject("ScoreText");
            txtObj.transform.SetParent(canvas.transform, false);
            var rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.8f, 0.9f); rt.anchorMax = new Vector2(0.95f, 0.95f);
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(250, 50);
            var txt = txtObj.AddComponent<Text>();
            txt.text = "Score: 0";
            txt.fontSize = 42; txt.fontStyle = FontStyle.Bold; txt.color = Color.white;
            uiMgr.scoreText = txt;
        }

        // 5. Create Health Bar
        if (uiMgr.playerHealthBar == null)
        {
            uiMgr.playerHealthBar = CreateHealthBar(canvas.transform, "Assets/Art/UI/HealthBarUI.png");
        }

        // 6. Create Win / Lose Panels
        if (uiMgr.winPanel == null)
        {
            uiMgr.winPanel = CreatePanel(canvas.transform, "WinPanel", "YOU WIN!", new Color(0.9f, 0.8f, 0.1f));
            AddRestartButton(uiMgr.winPanel.transform, uiMgr, "Assets/Art/UI/MenuButtons.png");
            uiMgr.winPanel.SetActive(false);
        }

        if (uiMgr.losePanel == null)
        {
            uiMgr.losePanel = CreatePanel(canvas.transform, "LosePanel", "YOU LOSE 💀", new Color(0.9f, 0.15f, 0.15f));
            AddRestartButton(uiMgr.losePanel.transform, uiMgr, "Assets/Art/UI/MenuButtons.png");
            uiMgr.losePanel.SetActive(false);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("✅ FULL UI GENERATED!");
        EditorUtility.DisplayDialog("UI Fixed!", "Your entire Canvas, Health Bar, Score, and Win/Lose Panels have been successfully created from scratch and properly connected!", "OK");

        // Delete this script so it cleans up the menu after running
        AssetDatabase.DeleteAsset("Assets/Scripts/Editor/AutoBuildUI.cs");
    }

    static Slider CreateHealthBar(Transform parent, string spritePath)
    {
        var go = new GameObject("PlayerHealthBar");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.9f); rt.anchorMax = new Vector2(0.05f, 0.9f); rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(400, 60);

        var bg = go.AddComponent<Image>();
        Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (customSprite != null) bg.sprite = customSprite;
        else bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = new Vector2(10, 10); faRT.offsetMax = new Vector2(-10, -10);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;

        var fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.red;

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
        slider.fillRect = fillRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;

        return slider;
    }

    static GameObject CreatePanel(Transform parent, string name, string message, Color textColor)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(panel.transform, false);
        var tRT = textGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(0, 100); tRT.offsetMax = new Vector2(0, 100);
        var text = textGO.AddComponent<Text>();
        text.text = message; text.fontSize = 80; text.fontStyle = FontStyle.Bold;
        text.color = textColor; text.alignment = TextAnchor.MiddleCenter;

        return panel;
    }

    static void AddRestartButton(Transform panel, UIManager uiMgr, string spritePath)
    {
        var go = new GameObject("RestartButton");
        go.transform.SetParent(panel, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.3f); rt.anchorMax = new Vector2(0.5f, 0.3f);
        rt.sizeDelta = new Vector2(300, 80); rt.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (customSprite != null) img.sprite = customSprite;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, uiMgr.OnRestartButton);

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tRT = txtGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
        var text = txtGO.AddComponent<Text>();
        text.text = "PLAY AGAIN"; text.fontSize = 32; text.fontStyle = FontStyle.Bold;
        text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
    }
}
