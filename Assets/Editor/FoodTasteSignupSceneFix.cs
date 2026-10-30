#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Makes the existing IntroScene/SinupPanel usable and visually consistent with the
/// FoodTasteApp intro art. All graphics come from assets already in the repository
/// or built-in Unity UI sprites. No manual ZIP install or menu action is required.
/// Existing references, input fields, dropdowns, buttons and DB logic are preserved.
/// </summary>
[InitializeOnLoad]
public static class FoodTasteSignupSceneFix
{
    private const string ScenePath = "Assets/Scenes/IntroScene.unity";
    private const string ReadyName = "FoodTasteSignupUI_Applied_20261030";
    private static readonly Color Brown = new Color(.29f, .14f, .065f, 1f);
    private static readonly Color Orange = new Color(1f, .62f, .085f, 1f);
    private static readonly Color Cream = new Color(1f, .963f, .86f, .96f);
    private static readonly Color Pale = new Color(1f, .916f, .72f, .98f);

    private static bool isApplying;

    static FoodTasteSignupSceneFix()
    {
        EditorApplication.delayCall += AutoApply;
        EditorSceneManager.sceneSaved += AfterSceneSaved;
    }

    private static void AfterSceneSaved(Scene saved)
    {
        if (!isApplying && saved.path == ScenePath)
            EditorApplication.delayCall += AutoApply;
    }

    [MenuItem("Tools/FoodTasteApp/Repair Signup UI")]
    public static void RepairFromMenu() { Apply(false); }

    private static void AutoApply()
    {
        if (isApplying) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += AutoApply;
            return;
        }
        if (!File.Exists(ScenePath)) return;
        // A fast, non-mutating check to avoid opening/saving the scene each launch.
        string yaml;
        try { yaml = File.ReadAllText(ScenePath); }
        catch (IOException) { return; }
        if (yaml.Contains("m_Name: " + ReadyName)) return;
        Apply(true);
    }

    private static void Apply(bool automatic)
    {
        if (isApplying) return;
        isApplying = true;
        Scene scene = default(Scene);
        bool openedAdditive = false;
        try
        {
            Scene loaded = SceneManager.GetSceneByPath(ScenePath);
            if (loaded.IsValid() && loaded.isLoaded)
            {
                // Never clobber in-progress scene edits. Manual menu call can
                // deliberately rerun after the user has saved their scene.
                if (loaded.isDirty && automatic)
                {
                    Debug.LogWarning("[FoodTasteApp] Signup UI repair postponed: IntroScene has unsaved edits. Save the scene and use Tools > FoodTasteApp > Repair Signup UI.");
                    return;
                }
                scene = loaded;
            }
            else
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                openedAdditive = true;
            }

            Transform signup = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "SinupPanel" || t.name == "SignupPanel");
            if (signup == null) throw new InvalidOperationException("SinupPanel missing from IntroScene");

            string[] expected = {
                "IDinput", "PWDInput", "PWDCheckInput", "NickNameInput",
                "TensDigit", "OneDigit", "DoRegion", "SiguRegion",
                "IDText", "PWDText", "PWDCheckText", "NickNameText",
                "AgeText", "RegionText", "SignupBtn", "BackBtn",
                "ImagePanel", "PanelImageViewer"
            };
            foreach (string name in expected)
                if (signup.Find(name) == null)
                    throw new InvalidOperationException("Missing signup control: " + name);

            Sprite builtIn = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite foodBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/FoodTasteIntro/IntroBackground.png");
            if (foodBackdrop == null)
                foodBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Recovered/Signup2.png");

            RectTransform artRoot = Rect(signup, "FoodTasteSignupVisualRoot");
            Stretch(artRoot);
            artRoot.SetAsFirstSibling();
            Image background = Graphic(artRoot, null, Color.white);
            background.sprite = foodBackdrop;
            background.color = foodBackdrop != null ? Color.white : new Color(.51f, .30f, .17f, 1f);
            background.type = Image.Type.Simple;
            background.preserveAspect = false;

            RectTransform plate = Rect(artRoot, "SignupCreamPlate");
            Position(plate, 0, -8, 1670, 988);
            Graphic(plate, builtIn, Cream);
            AddShadow(plate.gameObject, new Color(.18f, .09f, .04f, .4f), new Vector2(8f, -9f));

            RectTransform accent = Rect(artRoot, "SignupTitleLine");
            Position(accent, 0, 365, 1320, 4);
            Graphic(accent, null, new Color(.75f, .38f, .12f, .45f));

            Text(artRoot, "SignupVisibleTitle", "회원가입", 70, Brown, 0, 422, 820, 110, true);

            // Keep the original photo preview and image-loading components.
            Transform photo = signup.Find("ImagePanel");
            Position((RectTransform)photo, -560, -62, 1090, 1070);
            photo.localScale = new Vector3(.44f, .46f, 1f);
            Transform previousTitle = photo.Find("ImagePanelName");
            if (previousTitle != null)
                foreach (TMP_Text legacy in previousTitle.GetComponentsInChildren<TMP_Text>(true))
                    legacy.enabled = false;

            RectTransform photoFrame = Rect(artRoot, "SignupPhotoFrame");
            Position(photoFrame, -560, -85, 525, 598);
            Graphic(photoFrame, builtIn, new Color(.69f, .39f, .17f, 1f));
            RectTransform photoInner = Rect(artRoot, "SignupPhotoInterior");
            Position(photoInner, -560, -85, 510, 584);
            Graphic(photoInner, builtIn, new Color(.96f, .89f, .74f, 1f));
            // Keep the actual profile image above the frame, so it can display.
            photo.SetAsLastSibling();

            RectTransform openArea = Rect(signup, "SignupOpenPhotoButton");
            Position(openArea, -560, 294, 520, 92);
            Image profileArt = Graphic(openArea, builtIn, new Color(1f, .77f, .35f, 1f));
            profileArt.raycastTarget = true;
            UnityEngine.UI.Button openButton = openArea.GetComponent<UnityEngine.UI.Button>();
            if (openButton == null) openButton = openArea.gameObject.AddComponent<UnityEngine.UI.Button>();
            openButton.targetGraphic = profileArt;
            Tint(openButton);
            Text(openArea, "Caption", "프로필 이미지 설정", 34, Brown, 0, 0, 495, 80, true);

            Label(signup, "IDText", "아이디", -137, 245, builtIn);
            Label(signup, "PWDText", "비밀번호", -137, 139, builtIn);
            Label(signup, "PWDCheckText", "비밀번호 확인", -137, 33, builtIn);
            Label(signup, "NickNameText", "닉네임", -137, -73, builtIn);
            Label(signup, "AgeText", "나이", -137, -185, builtIn);
            Label(signup, "RegionText", "지역", -137, -291, builtIn);

            Input(signup.Find("IDinput"), 436, 245, builtIn, "아이디를 입력하세요", false);
            Input(signup.Find("PWDInput"), 436, 139, builtIn, "비밀번호를 입력하세요", true);
            Input(signup.Find("PWDCheckInput"), 436, 33, builtIn, "비밀번호를 다시 입력하세요", true);
            Input(signup.Find("NickNameInput"), 436, -73, builtIn, "닉네임을 입력하세요", false);

            Dropdown(signup.Find("TensDigit"), 249, -185, 270, builtIn);
            Dropdown(signup.Find("OneDigit"), 615, -185, 270, builtIn);
            Dropdown(signup.Find("DoRegion"), 249, -291, 270, builtIn);
            Dropdown(signup.Find("SiguRegion"), 615, -291, 270, builtIn);

            Button(signup.Find("SignupBtn"), "가입하기", 240, -418, builtIn,
                new Color(1f, .61f, .055f, 1f));
            Button(signup.Find("BackBtn"), "뒤로가기", 610, -418, builtIn,
                new Color(.58f, .30f, .16f, 1f));

            Transform browser = signup.Find("PanelImageViewer");
            browser.gameObject.SetActive(false);
            RectTransform closeRect = Rect(browser, "SignupClosePhotoButton");
            Position(closeRect, 510, 420, 200, 82);
            Image closeArt = Graphic(closeRect, builtIn, new Color(.62f, .29f, .13f, 1f));
            closeArt.raycastTarget = true;
            UnityEngine.UI.Button closeButton = closeRect.GetComponent<UnityEngine.UI.Button>();
            if (closeButton == null) closeButton = closeRect.gameObject.AddComponent<UnityEngine.UI.Button>();
            closeButton.targetGraphic = closeArt;
            Tint(closeButton);
            Text(closeRect, "Caption", "닫기", 34, Color.white, 0, 0, 180, 72, true);

            FoodTasteSignupPickerBridge controller = signup.GetComponent<FoodTasteSignupPickerBridge>();
            if (controller == null)
                controller = signup.gameObject.AddComponent<FoodTasteSignupPickerBridge>();
            controller.Browser = browser.gameObject;
            controller.OpenButton = openButton;
            controller.CloseButton = closeButton;

            RectTransform marker = Rect(artRoot, ReadyName);
            Position(marker, 0, 0, 0, 0);
            marker.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new IOException("Could not save IntroScene");
            Debug.Log("[FoodTasteApp] Saved signup visuals to IntroScene. Original TMP inputs and SignupPanelManager remain connected.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[FoodTasteApp] Signup scene repair failed: " + ex);
            if (!automatic) EditorUtility.DisplayDialog("FoodTasteApp", ex.Message, "확인");
        }
        finally
        {
            if (openedAdditive && scene.IsValid())
                EditorSceneManager.CloseScene(scene, true);
            isApplying = false;
        }
    }

    private static RectTransform Rect(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child.GetComponent<RectTransform>();
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void Position(RectTransform rect, float x, float y, float w, float h)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
        rect.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static Image Graphic(RectTransform rect, Sprite sprite, Color color)
    {
        Image image = rect.GetComponent<Image>();
        if (image == null) image = rect.gameObject.AddComponent<Image>();
        image.enabled = true;
        image.sprite = sprite;
        image.type = sprite != null && sprite.border.sqrMagnitude > 0
            ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void AddShadow(GameObject obj, Color color, Vector2 distance)
    {
        Shadow shadow = obj.GetComponent<Shadow>();
        if (shadow == null) shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    private static TextMeshProUGUI Text(Transform parent, string name, string value,
        float size, Color color, float x, float y, float w, float h, bool bold)
    {
        RectTransform rect = Rect(parent, name);
        Position(rect, x, y, w, h);
        TextMeshProUGUI label = rect.GetComponent<TextMeshProUGUI>();
        if (label == null) label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/GmarketSans Korean SDF.asset");
        if (font != null) label.font = font;
        label.text = value;
        label.enabled = true;
        label.fontSize = size;
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        return label;
    }

    private static void Label(Transform signup, string name, string caption,
        float x, float y, Sprite rounded)
    {
        Transform target = signup.Find(name);
        Position((RectTransform)target, x, y, 255, 76);
        foreach (TMP_Text old in target.GetComponents<TMP_Text>())
            old.enabled = false;
        RectTransform labelArt = Rect(target, "SignupLabelBackground");
        Stretch(labelArt);
        Graphic(labelArt, rounded, new Color(.98f, .79f, .49f, 1f));
        labelArt.SetAsFirstSibling();
        Text(target, "SignupReadableLabel", caption,
            caption.Length > 5 ? 29 : 34, Brown, 0, 0, 239, 70, true);
    }

    private static void Input(Transform target, float x, float y, Sprite rounded,
        string placeholder, bool password)
    {
        Position((RectTransform)target, x, y, 645, 78);
        Image image = Graphic((RectTransform)target, rounded, Pale);
        image.raycastTarget = true;
        TMP_InputField input = target.GetComponent<TMP_InputField>();
        input.targetGraphic = image;
        if (password) input.contentType = TMP_InputField.ContentType.Password;
        input.transition = Selectable.Transition.ColorTint;
        ColorBlock block = input.colors;
        block.normalColor = Color.white;
        block.highlightedColor = new Color(1f, .97f, .83f, 1f);
        block.selectedColor = new Color(1f, .94f, .75f, 1f);
        input.colors = block;
        if (input.textViewport != null)
        {
            RectTransform viewport = input.textViewport;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(24, 7);
            viewport.offsetMax = new Vector2(-22, -7);
        }
        TMP_FontAsset korean = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/GmarketSans Korean SDF.asset");
        if (input.textComponent != null)
        {
            if (korean != null) input.textComponent.font = korean;
            input.textComponent.fontSize = 31;
            input.textComponent.color = Brown;
        }
        TMP_Text hint = input.placeholder as TMP_Text;
        if (hint != null)
        {
            if (korean != null) hint.font = korean;
            hint.text = placeholder;
            hint.fontSize = 28;
            hint.color = new Color(.55f, .40f, .28f, .85f);
        }
    }

    private static void Dropdown(Transform target, float x, float y, float width,
        Sprite rounded)
    {
        Position((RectTransform)target, x, y, width, 78);
        Image image = Graphic((RectTransform)target, rounded, Pale);
        image.raycastTarget = true;
        TMP_Dropdown dropdown = target.GetComponent<TMP_Dropdown>();
        dropdown.targetGraphic = image;
        Tint(dropdown);
        if (dropdown.captionText != null)
        {
            TMP_FontAsset korean = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Fonts/GmarketSans Korean SDF.asset");
            if (korean != null) dropdown.captionText.font = korean;
            dropdown.captionText.fontSize = 30;
            dropdown.captionText.color = Brown;
        }
    }

    private static void Button(Transform target, string caption, float x, float y,
        Sprite rounded, Color tint)
    {
        Position((RectTransform)target, x, y, 310, 93);
        Image image = Graphic((RectTransform)target, rounded, tint);
        image.raycastTarget = true;
        UnityEngine.UI.Button button = target.GetComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        Tint(button);
        foreach (TMP_Text old in target.GetComponentsInChildren<TMP_Text>(true))
            old.enabled = false;
        Text(target, "SignupReadableCaption", caption, 41, Color.white, 0, 0, 292, 82, true);
    }

    private static void Tint(Selectable selectable)
    {
        selectable.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = selectable.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, .93f, .78f, 1f);
        colors.pressedColor = new Color(.85f, .75f, .65f, 1f);
        colors.selectedColor = Color.white;
        selectable.colors = colors;
    }
}
#endif
