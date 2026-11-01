#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UI-only migration of the existing signup file browser.
/// Runs after pulling the repository; preserves DirectoryController,
/// DictionarySpawner, Data, ImageLoader and existing serialized references.
/// </summary>
[InitializeOnLoad]
public static class FoodTasteProfilePickerInstaller
{
    private const string ScenePath = "Assets/Scenes/IntroScene.unity";
    private const string PrefabPath = "Assets/PanelData.prefab";
    private const string Marker = "FoodTastePickerApplied_20261101_VisibleRows";
    private const string Art = "Assets/Resources/FoodTasteProfilePicker/";
    private static bool busy;

    private static readonly Color Dark = new Color(.32f, .16f, .075f, 1f);
    private static readonly Color Light = new Color(1f, .976f, .906f, 1f);

    static FoodTasteProfilePickerInstaller()
    {
        EditorApplication.delayCall += TryAutoApply;
        EditorSceneManager.sceneSaved += OnSceneSaved;
    }

    private static void OnSceneSaved(Scene scene)
    {
        if (!busy && scene.path == ScenePath)
            EditorApplication.delayCall += TryAutoApply;
    }

    [MenuItem("Tools/FoodTasteApp/Repair Profile Picker UI")]
    public static void ManualRepair() { Apply(false); }

    private static void TryAutoApply()
    {
        if (busy || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (!busy) EditorApplication.delayCall += TryAutoApply;
            return;
        }
        if (!File.Exists(ScenePath)) return;
        try
        {
            if (File.ReadAllText(ScenePath).Contains("m_Name: " + Marker))
                return;
        }
        catch (IOException) { return; }
        Apply(true);
    }

    private static void Apply(bool automatic)
    {
        if (busy) return;
        busy = true;
        Scene scene = default(Scene);
        bool opened = false;
        try
        {
            Sprite panel = Load("PickerBackground");
            Sprite header = Load("PickerHeader");
            Sprite buttonArt = Load("PickerButton");
            Sprite listArt = Load("PickerList");
            Sprite previewArt = Load("PickerPreview");

            Scene loaded = SceneManager.GetSceneByPath(ScenePath);
            if (loaded.IsValid() && loaded.isLoaded)
            {
                if (automatic && loaded.isDirty)
                {
                    Debug.LogWarning("[FoodTasteApp] Save IntroScene to allow automatic profile-picker layout installation.");
                    return;
                }
                scene = loaded;
            }
            else
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }

            Transform signup = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "SinupPanel" || t.name == "SignupPanel");
            if (signup == null) throw new InvalidOperationException("Signup panel missing");

            Transform popup = signup.Find("PanelImageViewer");
            Transform directory = popup != null ? popup.Find("PanelDirectory") : null;
            Transform listTitle = directory != null ? directory.Find("TextDirectoryName") : null;
            Transform scroll = directory != null ? directory.Find("Scroll View") : null;
            Transform photoPanel = signup.Find("ImagePanel");
            Transform sourcePhoto = photoPanel != null ? photoPanel.Find("ImageDrawTexture") : null;
            Transform sourceInfo = photoPanel != null ? photoPanel.Find("ImagePanelName") : null;
            Transform closeTransform = popup != null ? popup.Find("SignupClosePhotoButton") : null;
            if (popup == null || directory == null || listTitle == null ||
                scroll == null || sourcePhoto == null || sourceInfo == null ||
                closeTransform == null)
                throw new InvalidOperationException("The original profile picker hierarchy has changed. Scene not modified.");

            RectTransform popupRect = popup as RectTransform;
            SetCenter(popupRect, 0, 0, 1530, 900);
            popup.SetAsLastSibling();
            Image popupImage = popup.GetComponent<Image>();
            if (popupImage != null)
            {
                popupImage.color = Color.white;
                popupImage.raycastTarget = true;
            }

            // Add themed bitmap components behind the existing interactive controls.
            RectTransform backdrop = Child(popup, "PickerArtBackground");
            SetStretch(backdrop);
            SetSprite(backdrop, panel, Color.white, false);
            backdrop.SetAsFirstSibling();

            RectTransform head = Child(popup, "PickerArtHeader");
            SetCenter(head, 0, 389, 1470, 132);
            SetSprite(head, header, Color.white, false);
            head.SetSiblingIndex(1);
            SetText(Child(head, "PickerHeading"), "프로필 이미지 설정", 50, Dark,
                -296, 13, 800, 67, FontStyles.Bold);
            SetText(Child(head, "PickerSubheading"), "사용할 JPG 또는 PNG 이미지를 선택하세요.",
                25, new Color(.47f, .31f, .18f, 1f), -296, -38, 800, 47,
                FontStyles.Normal);

            SetCenter(directory as RectTransform, -453, -63, 567, 704);
            RectTransform listBackground = Child(directory, "PickerListArt");
            SetStretch(listBackground);
            SetSprite(listBackground, listArt, Color.white, false);
            listBackground.SetAsFirstSibling();

            SetCenter(listTitle as RectTransform, 0, 311, 506, 61);
            TMP_Text directoryLabel = listTitle.GetComponent<TMP_Text>();
            if (directoryLabel != null)
            {
                ApplyTextStyle(directoryLabel, 32, Dark, FontStyles.Bold);
                directoryLabel.alignment = TextAlignmentOptions.MidlineLeft;
                directoryLabel.enableWordWrapping = false;
                directoryLabel.overflowMode = TextOverflowModes.Ellipsis;
                directoryLabel.margin = new Vector4(16, 0, 10, 0);
            }

            SetCenter(scroll as RectTransform, 0, -34, 525, 570);
            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            if (sr != null)
            {
                sr.horizontal = false;
                sr.vertical = true;
                sr.scrollSensitivity = 28f;
                sr.movementType = ScrollRect.MovementType.Clamped;
            }
            Transform viewport = scroll.Find("Viewport");
            if (viewport == null) throw new InvalidOperationException("Scroll viewport missing");
            if (viewport.GetComponent<Mask>() == null &&
                viewport.GetComponent<RectMask2D>() == null)
                viewport.gameObject.AddComponent<RectMask2D>();
            Image viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.color = new Color(1f, 1f, 1f, .02f);
                viewportImage.raycastTarget = true;
            }

            Transform content = viewport.Find("Content");
            if (content == null) throw new InvalidOperationException("Scroll content missing");
            // The existing Content uses GridLayoutGroup, not VerticalLayoutGroup.
            // Its legacy 1730px-wide cells were centered in a ~508px viewport,
            // putting all icons and filenames outside the clipped area.
            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.childAlignment = TextAnchor.UpperLeft;
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 1;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                grid.cellSize = new Vector2(490f, 58f);
                grid.spacing = new Vector2(0f, 6f);
                grid.padding = new RectOffset(8, 8, 10, 10);
            }
            else
            {
                VerticalLayoutGroup vertical = content.GetComponent<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    vertical.childAlignment = TextAnchor.UpperLeft;
                    vertical.spacing = 6f;
                    vertical.padding = new RectOffset(8, 8, 10, 10);
                    vertical.childForceExpandHeight = false;
                    vertical.childControlHeight = false;
                }
            }
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null)
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Keep the existing scrollbar and drag interaction.
            Transform scrollbar = scroll.Find("Scrollbar Vertical");
            if (scrollbar != null)
            {
                var rt = scrollbar as RectTransform;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 17f);
                Scrollbar bar = scrollbar.GetComponent<Scrollbar>();
                if (bar != null && bar.handleRect != null)
                {
                    Image handle = bar.handleRect.GetComponent<Image>();
                    if (handle != null)
                        handle.color = new Color(.77f, .47f, .24f, 1f);
                }
            }

            RectTransform previewRoot = Child(popup, "PickerPreviewRoot");
            SetCenter(previewRoot, 309, -65, 820, 704);
            RectTransform previewFrame = Child(previewRoot, "PickerPreviewArt");
            SetStretch(previewFrame);
            SetSprite(previewFrame, previewArt, Color.white, false);
            previewFrame.SetAsFirstSibling();
            SetText(Child(previewRoot, "PickerPreviewTitle"),
                "선택한 이미지 미리보기", 31, Dark, 0, 301, 760, 54,
                FontStyles.Bold);

            RectTransform photoFrame = Child(previewRoot, "PickerImageInset");
            SetCenter(photoFrame, 0, 42, 754, 455);
            Image photoBackground = SetSprite(photoFrame, null,
                new Color(.985f, .974f, .937f, 1f), false);
            photoBackground.raycastTarget = false;
            var mask = photoFrame.GetComponent<RectMask2D>();
            if (mask == null) photoFrame.gameObject.AddComponent<RectMask2D>();

            RectTransform previewImage = Child(photoFrame, "PickerMirroredImage");
            SetStretch(previewImage);
            previewImage.offsetMin = new Vector2(12, 12);
            previewImage.offsetMax = new Vector2(-12, -12);
            Image previewPicture = SetSprite(previewImage, null, Color.white, false);
            previewPicture.preserveAspect = true;
            previewPicture.enabled = false;
            TextMeshProUGUI emptyText = SetText(Child(photoFrame, "PickerEmptyText"),
                "JPG 또는 PNG 이미지를 선택하세요.", 27,
                new Color(.58f, .45f, .32f, 1f), 0, 0, 690, 100,
                FontStyles.Normal);

            TextMeshProUGUI info = SetText(Child(previewRoot, "PickerFileDetails"),
                "파일을 선택하면 이미지 정보가 표시됩니다.", 24,
                Dark, 0, -244, 740, 87, FontStyles.Normal);
            info.enableWordWrapping = true;
            info.overflowMode = TextOverflowModes.Ellipsis;

            RectTransform confirm = Child(previewRoot, "PickerUseImageButton");
            SetCenter(confirm, 0, -319, 315, 71);
            Image confirmGraphic = SetSprite(confirm, buttonArt, Color.white, true);
            confirmGraphic.raycastTarget = true;
            Button confirmButton = confirm.GetComponent<Button>();
            if (confirmButton == null) confirmButton = confirm.gameObject.AddComponent<Button>();
            confirmButton.targetGraphic = confirmGraphic;
            ApplyButtonTint(confirmButton);
            SetText(Child(confirm, "Caption"), "이 이미지로 선택", 29,
                Color.white, 0, 0, 300, 60, FontStyles.Bold);

            SetCenter(closeTransform as RectTransform, 637, 390, 194, 74);
            Image closeImage = SetSprite(closeTransform as RectTransform, buttonArt,
                Color.white, true);
            closeImage.raycastTarget = true;
            Button closeButton = closeTransform.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.targetGraphic = closeImage;
                ApplyButtonTint(closeButton);
            }
            SetText(Child(closeTransform, "Caption"), "닫기", 31,
                Color.white, 0, 0, 172, 60, FontStyles.Bold);
            closeTransform.SetAsLastSibling();

            FoodTasteSignupPickerBridge bridge = signup.GetComponent<FoodTasteSignupPickerBridge>();
            if (bridge == null) throw new InvalidOperationException("Existing photo picker bridge missing");
            bridge.Browser = popup.gameObject;
            bridge.CloseButton = closeButton;
            // The existing ImageLoader already records the selected file.
            // The confirmation button only closes the overlay.
            confirmButton.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(confirmButton.onClick, bridge.Close);

            FoodTastePickerPreviewMirror mirror = popup.GetComponent<FoodTastePickerPreviewMirror>();
            if (mirror == null)
                mirror = popup.gameObject.AddComponent<FoodTastePickerPreviewMirror>();
            mirror.Browser = popup.gameObject;
            mirror.SourceImage = sourcePhoto.GetComponent<Image>();
            mirror.SourceInfo = sourceInfo.GetComponent<TMP_Text>();
            mirror.PreviewImage = previewPicture;
            mirror.PreviewInfo = info;
            mirror.EmptyHint = emptyText;

            // A no-graphics marker ensures repeat pulls don't add duplicate UI.
            RectTransform marker = Child(popup, Marker);
            SetCenter(marker, 0, 0, 0, 0);
            marker.gameObject.SetActive(false);
            popup.gameObject.SetActive(false);

            UpdateRowPrefab();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new IOException("Cannot save IntroScene");
            AssetDatabase.SaveAssets();
            Debug.Log("[FoodTasteApp] ProfilePicker theme, scroll list, preview and buttons saved. Original file navigation preserved.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[FoodTasteApp] Profile picker UI installation failed: " + ex);
            if (!automatic)
                EditorUtility.DisplayDialog("FoodTasteApp", ex.Message, "확인");
        }
        finally
        {
            if (opened && scene.IsValid())
                EditorSceneManager.CloseScene(scene, true);
            busy = false;
        }
    }

    private static void UpdateRowPrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            RectTransform row = root.GetComponent<RectTransform>();
            row.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 58);
            Transform iconObject = root.transform.Find("ImageDataIcon");
            Transform nameObject = root.transform.Find("TextDataName");
            if (iconObject == null || nameObject == null)
                throw new InvalidOperationException("PanelData prefab changed");
            var icon = iconObject as RectTransform;
            icon.anchorMin = icon.anchorMax = new Vector2(0f, .5f);
            icon.pivot = new Vector2(.5f, .5f);
            icon.anchoredPosition = new Vector2(34, 0);
            icon.sizeDelta = new Vector2(35, 35);
            icon.localScale = Vector3.one;
            Image iconImage = iconObject.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = true;
            }

            var textRect = nameObject as RectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(64, 6);
            textRect.offsetMax = new Vector2(-12, -6);
            textRect.localScale = Vector3.one;
            TMP_Text text = nameObject.GetComponent<TMP_Text>();
            if (text != null)
            {
                ApplyTextStyle(text, 24, Dark, FontStyles.Normal);
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.enableWordWrapping = false;
                text.overflowMode = TextOverflowModes.Ellipsis;
                text.characterSpacing = 0;
            }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Sprite Load(string name)
    {
        Sprite result = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        if (result == null)
            throw new InvalidOperationException("Missing picker sprite: " + name);
        return result;
    }

    private static RectTransform Child(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        if (found != null) return found as RectTransform;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void SetCenter(RectTransform rect, float x, float y, float w, float h)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
        rect.localScale = Vector3.one;
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static Image SetSprite(RectTransform rect, Sprite sprite, Color tint,
        bool sliced)
    {
        Image image = rect.GetComponent<Image>();
        if (image == null) image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = tint;
        image.type = sliced && sprite != null && sprite.border.sqrMagnitude > 0
            ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = false;
        image.enabled = true;
        return image;
    }

    private static TextMeshProUGUI SetText(RectTransform rect, string title,
        int size, Color color, float x, float y, float w, float h, FontStyles style)
    {
        SetCenter(rect, x, y, w, h);
        TextMeshProUGUI label = rect.GetComponent<TextMeshProUGUI>();
        if (label == null) label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(label, size, color, style);
        label.text = title;
        label.alignment = TextAlignmentOptions.Center;
        return label;
    }

    private static void ApplyTextStyle(TMP_Text label, int size,
        Color color, FontStyles style)
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/GmarketSans Korean SDF.asset");
        if (font != null) label.font = font;
        label.fontSize = size;
        label.color = color;
        label.fontStyle = style;
        label.enableAutoSizing = false;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void ApplyButtonTint(Button button)
    {
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, .9f, .75f, 1f);
        colors.pressedColor = new Color(.78f, .67f, .54f, 1f);
        button.colors = colors;
    }
}
#endif
