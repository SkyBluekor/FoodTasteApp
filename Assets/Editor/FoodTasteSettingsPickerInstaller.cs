#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class FoodTasteSettingsPickerInstaller
{
    const string Art = "Assets/Resources/FoodTasteProfilePicker/";
    static readonly Color Dark = new Color(.32f, .16f, .075f, 1);
    [MenuItem("Tools/FoodTaste/Apply Settings Profile Picker")]
    public static void Apply()
    {
        UpdateProfileFunc controller = null;
        foreach (var candidate in Resources.FindObjectsOfTypeAll<UpdateProfileFunc>())
            if (candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded) { controller = candidate; break; }
        if (controller == null || controller.ChProPanel == null) throw new InvalidOperationException("Settings profile picker not found.");
        Transform popup = controller.ChProPanel.transform;
        Transform bg = popup.Find("BackGround");
        Transform scroll = bg != null ? bg.Find("Scroll View") : null;
        Transform viewport = scroll != null ? scroll.Find("Viewport") : null;
        Transform content = viewport != null ? viewport.Find("Content") : null;
        if (bg == null || scroll == null || content == null) throw new InvalidOperationException("Settings profile picker hierarchy changed.");
        Center((RectTransform)popup, 0, 0, 1530, 900);
        var overlay = popup.GetComponent<Image>(); if (overlay != null) { overlay.color = new Color(0, 0, 0, .35f); overlay.raycastTarget = true; }
        popup.SetAsLastSibling();
        Center((RectTransform)bg, 0, 0, 1530, 900);
        ImageArt(bg, "PickerBackground", false);
        var header = EnsureChild(bg, "PickerHeaderArt"); Center(header, 0, 385, 1460, 125); ImageArt(header, "PickerHeader", false); header.SetAsFirstSibling();
        var title = EnsureChild(header, "PickerHeading"); Center(title, -240, 6, 820, 70); var titleText = Text(title, "프로필 이미지 설정", 42); titleText.fontStyle = FontStyles.Bold;
        var heading = EnsureChild(bg, "PickerListHeadingArt"); Center(heading, -430, 280, 540, 62); ImageArt(heading, "PickerHeader", false);
        DictionarySpawnerinSetting spawner = null;
        foreach (var candidate in Resources.FindObjectsOfTypeAll<DictionarySpawnerinSetting>())
            if (candidate.gameObject.scene == controller.gameObject.scene) { spawner = candidate; break; }
        if (spawner == null) throw new InvalidOperationException("Settings directory browser missing.");
        var directoryLabel = new SerializedObject(spawner);
        var dirText = directoryLabel.FindProperty("textDicName").objectReferenceValue as TextMeshProUGUI;
        if (dirText == null) throw new InvalidOperationException("Directory label missing.");
        Center(dirText.rectTransform, -430, 281, 510, 55); Style(dirText, 26); dirText.alignment = TextAlignmentOptions.MidlineLeft; dirText.enableWordWrapping = false; dirText.overflowMode = TextOverflowModes.Ellipsis; dirText.transform.SetAsLastSibling();
        Center((RectTransform)scroll, -430, -45, 540, 570);
        var sr = scroll.GetComponent<ScrollRect>(); sr.horizontal = false; sr.vertical = true; sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 28;
        var scrollBg = scroll.GetComponent<Image>(); if (scrollBg != null) { scrollBg.sprite = Load("PickerList"); scrollBg.color = Color.white; }
        var oldMask = viewport.GetComponent<Mask>(); if (oldMask != null) oldMask.enabled = false;
        var clip = viewport.GetComponent<RectMask2D>(); if (clip == null) clip = viewport.gameObject.AddComponent<RectMask2D>(); clip.enabled = true;
        var viewportImage = viewport.GetComponent<Image>(); if (viewportImage != null) viewportImage.color = new Color(1,1,1,.02f);
        var cr = (RectTransform)content; cr.anchorMin = new Vector2(0, 1); cr.anchorMax = Vector2.one; cr.pivot = new Vector2(0, 1); cr.sizeDelta = Vector2.zero; cr.anchoredPosition = Vector2.zero;
        var grid = content.GetComponent<GridLayoutGroup>();
        if (grid != null) { grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 1; grid.cellSize = new Vector2(490, 58); grid.spacing = new Vector2(0, 6); grid.padding = new RectOffset(8,8,10,10); grid.childAlignment = TextAnchor.UpperLeft; }
        var vertical = content.GetComponent<VerticalLayoutGroup>();
        if (vertical != null) { vertical.childAlignment = TextAnchor.UpperLeft; vertical.padding = new RectOffset(8,8,10,10); vertical.spacing = 6; vertical.childControlWidth = true; vertical.childControlHeight = false; vertical.childForceExpandWidth = true; vertical.childForceExpandHeight = false; }
        var fitter = content.GetComponent<ContentSizeFitter>(); if (fitter != null) { fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; }
        var preview = EnsureChild(bg, "PickerPreviewArt"); Center(preview, 320, -20, 760, 650); ImageArt(preview, "PickerPreview", false); preview.SetSiblingIndex(1);
        var previewHeading = EnsureChild(bg, "PickerPreviewHeading"); Center(previewHeading, 320, 260, 650, 55); Text(previewHeading, "선택한 이미지 미리보기", 29);
        Center(controller.ChProfileIMg.rectTransform, 320, 45, 320, 300); controller.ChProfileIMg.preserveAspect = true;
        Center(controller.StateCheckText.rectTransform, 320, -170, 650, 75); Style(controller.StateCheckText, 24); controller.StateCheckText.alignment = TextAlignmentOptions.Center; controller.StateCheckText.enableWordWrapping = true; controller.StateCheckText.overflowMode = TextOverflowModes.Ellipsis;
        Center((RectTransform)controller.ChangeBtn.transform, 185, -305, 270, 72); ImageArt(controller.ChangeBtn.transform, "PickerButton", true); ButtonText(controller.ChangeBtn, "프로필 적용");
        Center((RectTransform)controller.CloseProPanelBtn.transform, 490, -305, 270, 72); ImageArt(controller.CloseProPanelBtn.transform, "PickerButton", true); ButtonText(controller.CloseProPanelBtn, "닫기");
        var row = PrefabUtility.LoadPrefabContents("Assets/PanelData2.prefab");
        try {
            ((RectTransform)row.transform).sizeDelta = new Vector2(490, 58);
            var icon = (RectTransform)row.transform.Find("ImageDataIcon"); var name = (RectTransform)row.transform.Find("TextDataName");
            if (icon == null || name == null) throw new InvalidOperationException("Settings file row changed.");
            icon.anchorMin = icon.anchorMax = new Vector2(0, .5f); icon.pivot = new Vector2(.5f,.5f); icon.anchoredPosition = new Vector2(34,0); icon.sizeDelta = new Vector2(35,35); icon.localScale = Vector3.one;
            name.anchorMin = Vector2.zero; name.anchorMax = Vector2.one; name.offsetMin = new Vector2(64,6); name.offsetMax = new Vector2(-12,-6); name.localScale = Vector3.one;
            var label = name.GetComponent<TextMeshProUGUI>(); Style(label,24); label.alignment = TextAlignmentOptions.MidlineLeft; label.enableWordWrapping = false; label.overflowMode = TextOverflowModes.Ellipsis; label.characterSpacing = 0; label.raycastTarget = true;
            PrefabUtility.SaveAsPrefabAsset(row, "Assets/PanelData2.prefab");
        } finally { PrefabUtility.UnloadPrefabContents(row); }
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene); EditorSceneManager.SaveScene(controller.gameObject.scene); AssetDatabase.SaveAssets();
        Debug.Log("Settings profile picker applied.");
    }
    static Sprite Load(string name) { var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png"); if (s == null) throw new InvalidOperationException("Missing image " + name); return s; }
    static RectTransform EnsureChild(Transform parent, string name) { var t = parent.Find(name); if (t != null) return (RectTransform)t; var g = new GameObject(name, typeof(RectTransform)); g.layer = parent.gameObject.layer; g.transform.SetParent(parent,false); return (RectTransform)g.transform; }
    static void Center(RectTransform r,float x,float y,float w,float h) { r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f,.5f); r.anchoredPosition = new Vector2(x,y); r.sizeDelta = new Vector2(w,h); r.localScale = Vector3.one; }
    static void ImageArt(Transform t,string name,bool raycast) { var i=t.GetComponent<Image>(); if (i==null) i=t.gameObject.AddComponent<Image>(); i.sprite=Load(name); i.color=Color.white; i.type=Image.Type.Simple; i.raycastTarget=raycast; }
    static void Style(TMP_Text t,int size) { var f=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/GmarketSans Korean SDF.asset"); if(f!=null)t.font=f; t.color=Dark; t.fontSize=size; t.enableAutoSizing=false; t.raycastTarget=false; }
    static TextMeshProUGUI Text(RectTransform r,string value,int size) { var t=r.GetComponent<TextMeshProUGUI>(); if(t==null)t=r.gameObject.AddComponent<TextMeshProUGUI>(); Style(t,size); t.text=value; t.alignment=TextAlignmentOptions.Center; return t; }
    static void ButtonText(Button b,string value) { var t=b.GetComponentInChildren<TMP_Text>(true); if(t!=null) { Style(t,28); t.text=value; t.alignment=TextAlignmentOptions.Center; } }
}
#endif
