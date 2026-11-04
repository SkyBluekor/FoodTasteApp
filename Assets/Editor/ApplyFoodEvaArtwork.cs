using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ApplyFoodEvaArtwork
{
    const string Art = "Assets/Resources/FoodTasteFoodEva/";
    static readonly Color Ink = new Color(0.36f, 0.19f, 0.09f, 1f);

    [MenuItem("Tools/FoodTaste/Apply Food Evaluation Artwork")]
    public static void Apply()
    {
        string[] names = { "FoodEva_Background", "FoodEva_ConfirmButton", "FoodEva_SkipButton", "FoodEva_ExitButton", "EmptyStar", "FullStar", "FoodEva_CompleteButton", "FoodEva_AlternateExit" };
        foreach (var name in names) {
            var importer = AssetImporter.GetAtPath(Art + name + ".png") as TextureImporter;
            if (importer == null) { Debug.LogError("Food evaluation artwork missing: " + name); return; }
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || !importer.alphaIsTransparency) {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.SaveAndReimport();
            }
        }
        FoodEvaManager manager = null;
        foreach (var candidate in Resources.FindObjectsOfTypeAll<FoodEvaManager>())
            if (candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded) { manager = candidate; break; }
        if (manager == null || manager.FoodEvaPanel == null || manager.FullFoodPanel == null) { Debug.LogError("Food evaluation panel not found in loaded scene."); return; }
        var data = new SerializedObject(manager);
        var empty = Load("EmptyStar");
        data.FindProperty("EmptyStarSprite").objectReferenceValue = empty;
        data.FindProperty("FullStarSprite").objectReferenceValue = Load("FullStar");
        var root = manager.FoodEvaPanel.transform;
        SetImage(root.GetComponent<Image>(), "FoodEva_Background", false);
        var food = (Image)data.FindProperty("FoodImg").objectReferenceValue;
        Place((RectTransform)food.transform, -470, 55, 610, 610);
        food.preserveAspect = true;
        var foodText = (TextMeshProUGUI)data.FindProperty("FoodText").objectReferenceValue;
        Place(foodText.rectTransform, -470, -325, 680, 90); StyleText(foodText, 54, 28);
        var stars = ((Button)data.FindProperty("Star1").objectReferenceValue).transform.parent;
        Place((RectTransform)stars, 405, 100, 900, 400);
        var starPanelImage = stars.GetComponent<Image>(); if (starPanelImage != null) starPanelImage.color = new Color(1, 1, 1, 0);
        for (int i = 0; i < 5; i++) {
            var button = (Button)data.FindProperty("Star" + (i + 1)).objectReferenceValue;
            Place((RectTransform)button.transform, (i - 2) * 155, 70, 140, 140);
            button.image.sprite = empty; button.image.color = Color.white; button.image.type = Image.Type.Simple; button.image.preserveAspect = true;
        }
        var message = (TextMeshProUGUI)data.FindProperty("EvaMent").objectReferenceValue;
        Place(message.rectTransform, 0, -85, 800, 85); StyleText(message, 42, 24);
        ApplyButton((Button)data.FindProperty("ConfirmBtn").objectReferenceValue, "FoodEva_ConfirmButton", 220, -310, 285, 145);
        ApplyButton((Button)data.FindProperty("PassBtn").objectReferenceValue, "FoodEva_SkipButton", 570, -310, 285, 145);
        ApplyButton((Button)data.FindProperty("ExitBtn").objectReferenceValue, "FoodEva_ExitButton", 405, 355, 200, 100);
        var popup = manager.FullFoodPanel.transform;
        var popupBody = popup.Find("Image");
        if (popupBody != null) SetImage(popupBody.GetComponent<Image>(), "FoodEva_Background", false);
        var popupButton = popup.GetComponentInChildren<Button>(true);
        if (popupButton != null) {
            SetImage(popupButton.image, "FoodEva_CompleteButton", true);
            foreach (var label in popupButton.GetComponentsInChildren<TextMeshProUGUI>(true)) label.text = "";
        }
        foreach (var label in popup.GetComponentsInChildren<TextMeshProUGUI>(true)) if (popupButton == null || !label.transform.IsChildOf(popupButton.transform)) StyleText(label, 55, 25);
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Food evaluation artwork applied successfully.");
    }
    [MenuItem("Tools/FoodTaste/Move Evaluation Exit")]
    public static void MoveExit() {
        foreach (var manager in Resources.FindObjectsOfTypeAll<FoodEvaManager>()) {
            if (!manager.gameObject.scene.IsValid() || !manager.gameObject.scene.isLoaded) continue;
            var data = new SerializedObject(manager);
            var button = (Button)data.FindProperty("ExitBtn").objectReferenceValue;
            if (button == null) { Debug.LogError("Evaluation exit button not found."); return; }
            Place((RectTransform)button.transform, 405, 355, 200, 100);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            EditorSceneManager.SaveScene(manager.gameObject.scene);
            Debug.Log("Evaluation exit moved."); return;
        }
        Debug.LogError("Food evaluation manager not found.");
    }
    static Sprite Load(string name) { return AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png"); }
    static void SetImage(Image image, string name, bool preserve) {
        if (image == null) { Debug.LogWarning("Food evaluation image missing: " + name); return; }
        image.sprite = Load(name); image.color = Color.white; image.type = Image.Type.Simple; image.preserveAspect = preserve;
    }
    static void Place(RectTransform rect, float x, float y, float width, float height) {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one; rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height);
    }
    static void StyleText(TextMeshProUGUI text, float max, float min) {
        text.color = Ink; text.enableAutoSizing = true; text.fontSizeMin = min; text.fontSizeMax = max;
        text.alignment = TextAlignmentOptions.Center; text.enableWordWrapping = true;
    }
    static void ApplyButton(Button button, string image, float x, float y, float width, float height) {
        Place((RectTransform)button.transform, x, y, width, height); SetImage(button.image, image, true);
        foreach (var label in button.GetComponentsInChildren<TextMeshProUGUI>(true)) label.text = "";
    }
}


