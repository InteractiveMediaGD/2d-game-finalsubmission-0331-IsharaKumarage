using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoFixUI : ScriptableObject
{
    [MenuItem("Tools/Broken Warrior/🚑 Inject Enemy Health Bar & Fix UI")]
    public static void GenerateFullUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please run 'Fix My Missing UI' first to generate your canvas!", "OK");
            return;
        }

        UIManager uiMgr = Object.FindObjectOfType<UIManager>();

        // Destroy previous missing bars to safely regenerate them perfectly
        if (uiMgr.playerHealthBar != null) DestroyImmediate(uiMgr.playerHealthBar.gameObject);
        if (uiMgr.enemyHealthBar != null) DestroyImmediate(uiMgr.enemyHealthBar.gameObject);

        // Create Both Health Bars using the custom Sprite
        uiMgr.playerHealthBar = CreateHealthBar(canvas.transform, "PlayerHealthBar", "Assets/Art/UI/HealthBarUI.png", new Vector2(0.05f, 0.9f), Color.green);
        
        // Enemy is on the right side of the screen
        uiMgr.enemyHealthBar = CreateHealthBar(canvas.transform, "EnemyHealthBar", "Assets/Art/UI/HealthBarUI.png", new Vector2(0.95f, 0.9f), Color.red, true);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("✅ Player and Enemy Health Bars officially injected!");
        EditorUtility.DisplayDialog("UI Fixed!", "Enemy and Player Health bars are fully visual and mathematically connected to the combat logic. You will now see them decrease when fighting!", "OK");

        AssetDatabase.DeleteAsset("Assets/Scripts/Editor/AutoFixUI.cs");
    }

    static Slider CreateHealthBar(Transform parent, string title, string spritePath, Vector2 anchor, Color fillColor, bool rightAlign = false)
    {
        var go = new GameObject(title);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        
        // If it's the enemy bar, anchor to top right and pivot on the right
        float pivotX = rightAlign ? 1f : 0f;
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(pivotX, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(400, 60);

        var bg = go.AddComponent<Image>();
        Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (customSprite != null) bg.sprite = customSprite;
        else bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Add a text label above it indicating if it's Player or Enemy
        var lblObj = new GameObject("Label");
        lblObj.transform.SetParent(go.transform, false);
        var lRT = lblObj.AddComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0f, 1f); lRT.anchorMax = new Vector2(1f, 1f);
        lRT.pivot = new Vector2(0.5f, 0f); lRT.sizeDelta = new Vector2(0, 30);
        lRT.anchoredPosition = Vector2.zero;
        var txt = lblObj.AddComponent<Text>();
        txt.text = title.Replace("HealthBar", "").ToUpper();
        txt.color = Color.white; txt.fontStyle = FontStyle.Bold; txt.fontSize = 24;
        txt.alignment = rightAlign ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

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
        fillImg.color = fillColor;

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
        slider.fillRect = fillRT;
        slider.direction = rightAlign ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;
        slider.interactable = false;

        return slider;
    }
}
