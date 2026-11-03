using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ApplySettingsArtwork
{
    const string Art = "Assets/Resources/FoodTasteSettingsUI/";
    const string PrefabPath = "Assets/Resources/PreFab/FoodInfoPrefab.prefab";
    static readonly Color Ink = new Color(0.36f, 0.19f, 0.09f, 1f);

    [MenuItem("Tools/FoodTaste/Apply Settings Artwork")]
    public static void Apply()
    {
        string[] names = { "SettingsScreen_Background", "UserInfoPanel_Background", "FoodRatingStats_Card", "FoodRating_Row", "ScoreDropdown_Background", "CommonButton_Background", "WithdrawButton_Background", "CommonPopup_Background" };
        foreach (string name in names) {
            var importer = AssetImporter.GetAtPath(Art + name + ".png") as TextureImporter;
            if (importer == null) { Debug.LogError("Settings artwork missing: " + name); return; }
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single) {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.SaveAndReimport();
            }
        }
        var all = Resources.FindObjectsOfTypeAll<MyInfoPanelManager>();
        MyInfoPanelManager manager = null;
        foreach (var candidate in all) if (candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded) { manager = candidate; break; }
        if (manager == null || manager.SettingPanel == null) { Debug.LogError("SettingPanel not found in the loaded scene."); return; }
        var root = manager.SettingPanel.transform;
        SetImage(root.Find("SettingPanelBackGround"), "SettingsScreen_Background");
        var background = root.Find("SettingPanelBackGround");
        SetImage(background.Find("MyinfoBackGround"), "UserInfoPanel_Background");
        SetImage(background.Find("FoodInfoBackGround"), "FoodRatingStats_Card");
        var info = background.Find("MyinfoBackGround");
        var infoRect = (RectTransform)info;
        infoRect.sizeDelta = new Vector2(900, 1000);
        var title = info.Find("MyinfoText") as RectTransform;
        title.anchoredPosition = new Vector2(0, 405);
        var scroll = background.Find("FoodScoreChangePanel/FoodScoreListScrollView");
        var scrollImage = scroll.GetComponent<Image>(); if (scrollImage != null) scrollImage.color = new Color(1, 1, 1, 0);
        foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true)) {
            text.color = Ink; text.enableAutoSizing = true; text.fontSizeMin = Mathf.Min(text.fontSize, 15f); text.fontSizeMax = text.fontSize;
        }
        foreach (var button in root.GetComponentsInChildren<Button>(true)) {
            var img = button.GetComponent<Image>(); if (img == null) continue;
            bool danger = button.name == "DeletePanelOpenBtn" || button.name == "DeleteIdBtn";
            SetImage(button.transform, danger ? "WithdrawButton_Background" : "CommonButton_Background");
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) { label.enableWordWrapping = false; label.enableAutoSizing = true; label.fontSizeMin = 12; label.fontSizeMax = Mathf.Max(16, label.fontSize); }
        }
        foreach (string name in new[] { "PasswordChangePanel", "NickNameChangePanel", "DeleteIdPanel" }) {
            var popup = background.Find(name); SetImage(popup, "CommonPopup_Background");
            var header = popup.Find("Image"); if (header != null && header.GetComponent<Image>() != null) header.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }
        SetImage(background.Find("ProImgChangePanel/BackGround"), "UserInfoPanel_Background");
        var inputs = root.GetComponentsInChildren<TMP_InputField>(true);
        var existingInput = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/FoodTasteMainUI/FriendInput.png");
        foreach (var input in inputs) {
            var img = input.GetComponent<Image>(); if (img != null && existingInput != null) { img.sprite = existingInput; img.color = Color.white; img.type = Image.Type.Simple; }
        }
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        SetImage(prefab.transform, "FoodRating_Row");
        var drop = prefab.transform.Find("ChangeScoreDropDown"); SetImage(drop, "ScoreDropdown_Background");
        var arrow = drop.Find("Arrow"); if (arrow != null) arrow.gameObject.SetActive(false);
        SetImage(prefab.transform.Find("FoodScoreChangeBtn"), "CommonButton_Background");
        foreach (var text in prefab.GetComponentsInChildren<TextMeshProUGUI>(true)) {
            text.color = Ink; text.enableAutoSizing = true; text.fontSizeMin = 14; text.fontSizeMax = Mathf.Max(16, text.fontSize); text.enableWordWrapping = false;
        }
        var rowRect = (RectTransform)prefab.transform; rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, 94);
        PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath); PrefabUtility.UnloadPrefabContents(prefab);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
        AssetDatabase.SaveAssets();
        Debug.Log("FoodTaste settings artwork applied.");
    }
    static void SetImage(Transform target, string spriteName)
    {
        if (target == null) { Debug.LogWarning("Settings artwork target missing: " + spriteName); return; }
        var image = target.GetComponent<Image>();
        if (image == null) { Debug.LogWarning("Image component missing: " + target.name); return; }
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + spriteName + ".png"); image.color = Color.white; image.type = Image.Type.Simple; image.preserveAspect = false;
    }
}
