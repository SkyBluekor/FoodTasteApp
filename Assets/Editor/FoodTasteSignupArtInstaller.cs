#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-click setup of the *existing* SinupPanel in Assets/Scenes/IntroScene.unity.
/// Keeps the original TMP_InputField / TMP_Dropdown / Button components,
/// SignupPanelManager bindings, DB logic, region options and image picker.
/// </summary>
public static class FoodTasteSignupArtInstaller
{
    private const string ScenePath = "Assets/Scenes/IntroScene.unity";
    private const string ArtPath = "Assets/Resources/FoodTasteSignup/";
    private const string SignupCode = "Assets/Scripts/SignupPanelManager.cs";
    private static readonly string[] ArtFiles = {
        "Background", "ProfileBanner", "IdLabel", "PasswordLabel",
        "ConfirmPasswordLabel", "NicknameLabel", "AgeLabel", "RegionLabel",
        "SignupButton", "BackButton", "InputNormal", "InputFocused", "Dropdown"
    };

    [MenuItem("Tools/FoodTasteApp/Apply Signup UI + Tab Order")]
    public static void Apply()
    {
        try
        {
            foreach (string name in ArtFiles)
            {
                string path = ArtPath + name + ".png";
                if (!File.Exists(path))
                    throw new FileNotFoundException("Signup image missing", path);
            }
            if (!File.Exists(ScenePath) || !File.Exists(SignupCode))
                throw new FileNotFoundException("IntroScene or SignupPanelManager.cs missing");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Sprite[] sprites = new Sprite[ArtFiles.Length];
            for (int i = 0; i < ArtFiles.Length; i++)
            {
                string path = ArtPath + ArtFiles[i] + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    throw new InvalidOperationException("Texture import failed: " + path);
                bool sliced = ArtFiles[i] == "InputNormal" ||
                              ArtFiles[i] == "InputFocused" || ArtFiles[i] == "Dropdown";
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = ArtFiles[i] != "Background";
                importer.mipmapEnabled = false;
                importer.spriteBorder = sliced ? new Vector4(120f, 85f, 120f, 85f) : Vector4.zero;
                importer.SaveAndReimport();
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null)
                    throw new InvalidOperationException("Sprite could not be loaded: " + path);
            }

            // Take a local backup before writing a serialized scene.
            string backup = ScenePath + ".signup_backup_" +
                            DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
            File.Copy(ScenePath, backup, false);
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform signup = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "SinupPanel" || t.name == "SignupPanel");
            if (signup == null)
                throw new InvalidOperationException("IntroScene has no SinupPanel");

            // Check exact serialized scene objects before any layout mutation.
            foreach (string name in new[] {
                "IDinput", "PWDInput", "PWDCheckInput", "NickNameInput",
                "TensDigit", "OneDigit", "DoRegion", "SiguRegion",
                "IDText", "PWDText", "PWDCheckText", "NickNameText",
                "AgeText", "RegionText", "SignupBtn", "BackBtn",
                "ImagePanel", "PanelImageViewer" })
            {
                if (signup.Find(name) == null)
                    throw new InvalidOperationException("Signup component not found: " + name);
            }

            // Background and panel always stay behind the actual controls.
            RectTransform backdrop = EnsureRect(signup, "SignupArtworkBackground");
            Stretch(backdrop);
            Image bg = EnsureImage(backdrop);
            SetSprite(bg, sprites[0], false);
            backdrop.SetAsFirstSibling();

            RectTransform plate = EnsureRect(signup, "SignupFormPlate");
            Place(plate, new Vector2(0f, -12f), new Vector2(1640f, 980f));
            Image plateGraphic = EnsureImage(plate);
            plateGraphic.sprite = null;
            plateGraphic.color = new Color(1f, .965f, .88f, .93f);
            plateGraphic.raycastTarget = false;
            plate.SetSiblingIndex(1);

            RectTransform heading = EnsureRect(signup, "SignupArtTitle");
            Place(heading, new Vector2(0f, 412f), new Vector2(980f, 110f));
            var headingText = heading.GetComponent<TextMeshProUGUI>();
            if (headingText == null) headingText = heading.gameObject.AddComponent<TextMeshProUGUI>();
            headingText.text = "회원가입";
            headingText.fontSize = 66;
            headingText.color = new Color(.31f, .15f, .07f, 1f);
            headingText.fontStyle = FontStyles.Bold;
            headingText.alignment = TextAlignmentOptions.Center;
            headingText.raycastTarget = false;

            // Reuse the original profile-image preview. Its ImageLoader still works.
            Transform profile = signup.Find("ImagePanel");
            Place(profile.GetComponent<RectTransform>(), new Vector2(-555f, -55f),
                new Vector2(1100f, 1080f));
            profile.localScale = new Vector3(.47f, .51f, 1f);
            Transform oldName = profile.Find("ImagePanelName");
            if (oldName != null)
            {
                TMP_Text titleText = oldName.GetComponent<TMP_Text>();
                if (titleText != null) titleText.enabled = false;
            }
            Transform browser = signup.Find("PanelImageViewer");
            browser.gameObject.SetActive(false);

            RectTransform profileButtonRect = EnsureRect(signup, "SignupArtProfileButton");
            Place(profileButtonRect, new Vector2(-555f, 307f), new Vector2(485f, 139f));
            Image profileButtonArt = EnsureImage(profileButtonRect);
            SetSprite(profileButtonArt, sprites[1], true);
            profileButtonArt.raycastTarget = true;
            Button profileButton = profileButtonRect.GetComponent<Button>();
            if (profileButton == null)
                profileButton = profileButtonRect.gameObject.AddComponent<Button>();
            profileButton.targetGraphic = profileButtonArt;

            Label(signup, "IDText", sprites[2], new Vector2(-103f, 236f));
            Label(signup, "PWDText", sprites[3], new Vector2(-103f, 129f));
            Label(signup, "PWDCheckText", sprites[4], new Vector2(-103f, 22f));
            Label(signup, "NickNameText", sprites[5], new Vector2(-103f, -85f));
            Label(signup, "AgeText", sprites[6], new Vector2(-103f, -192f));
            Label(signup, "RegionText", sprites[7], new Vector2(-103f, -299f));

            SetInput(signup.Find("IDinput"), sprites[10], sprites[11], new Vector2(380f, 236f));
            SetInput(signup.Find("PWDInput"), sprites[10], sprites[11], new Vector2(380f, 129f));
            SetInput(signup.Find("PWDCheckInput"), sprites[10], sprites[11], new Vector2(380f, 22f));
            SetInput(signup.Find("NickNameInput"), sprites[10], sprites[11], new Vector2(380f, -85f));

            SetDropdown(signup.Find("TensDigit"), sprites[12], new Vector2(210f, -192f), 204f);
            SetDropdown(signup.Find("OneDigit"), sprites[12], new Vector2(458f, -192f), 204f);
            SetDropdown(signup.Find("DoRegion"), sprites[12], new Vector2(210f, -299f), 204f);
            SetDropdown(signup.Find("SiguRegion"), sprites[12], new Vector2(458f, -299f), 204f);

            SetButton(signup.Find("SignupBtn"), sprites[8], new Vector2(235f, -405f));
            SetButton(signup.Find("BackBtn"), sprites[9], new Vector2(555f, -405f));

            // Navigation is explicit for mouse, keyboard and controllers.
            Selectable[] order = new Selectable[] {
                signup.Find("IDinput").GetComponent<TMP_InputField>(),
                signup.Find("PWDInput").GetComponent<TMP_InputField>(),
                signup.Find("PWDCheckInput").GetComponent<TMP_InputField>(),
                signup.Find("NickNameInput").GetComponent<TMP_InputField>(),
                signup.Find("TensDigit").GetComponent<TMP_Dropdown>(),
                signup.Find("OneDigit").GetComponent<TMP_Dropdown>(),
                signup.Find("DoRegion").GetComponent<TMP_Dropdown>(),
                signup.Find("SiguRegion").GetComponent<TMP_Dropdown>()
            };
            for (int i = 0; i < order.Length; i++)
            {
                if (order[i] == null)
                    throw new InvalidOperationException("Signup Selectable is missing at index " + i);
                Navigation navigation = order[i].navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = order[(i - 1 + order.Length) % order.Length];
                navigation.selectOnDown = order[(i + 1) % order.Length];
                navigation.selectOnLeft = null;
                navigation.selectOnRight = null;
                order[i].navigation = navigation;
            }

            RectTransform closeRect = EnsureRect(signup, "SignupArtClosePhotoBrowser");
            Place(closeRect, new Vector2(-155f, 418f), new Vector2(175f, 70f));
            Image closeBg = EnsureImage(closeRect);
            closeBg.sprite = null;
            closeBg.color = new Color(.94f, .77f, .49f, .98f);
            Button close = closeRect.GetComponent<Button>();
            if (close == null) close = closeRect.gameObject.AddComponent<Button>();
            close.targetGraphic = closeBg;
            TMP_Text closeText = closeRect.GetComponentInChildren<TMP_Text>(true);
            if (closeText == null)
            {
                var closeTextObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                closeTextObject.transform.SetParent(closeRect, false);
                closeText = closeTextObject.GetComponent<TextMeshProUGUI>();
            }
            Place(closeText.GetComponent<RectTransform>(), Vector2.zero, closeRect.rect.size);
            closeText.text = "닫기";
            closeText.fontSize = 34;
            closeText.color = new Color(.27f, .12f, .05f, 1f);
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.raycastTarget = false;

            SignupPhotoBrowserController controller = signup.GetComponent<SignupPhotoBrowserController>();
            if (controller == null)
                controller = signup.gameObject.AddComponent<SignupPhotoBrowserController>();
            controller.Browser = browser.gameObject;
            controller.OpenButton = profileButton;
            controller.CloseButton = close;
            close.gameObject.SetActive(false);
            // The close button belongs to the browser popup lifecycle.
            // It is activated by the controller when the browser opens.

            // Other legacy children and serialized bindings remain unmodified.
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new IOException("Unable to save IntroScene");

            PatchSignupTabCode();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FoodTasteApp] Signup artwork and explicit Tab order applied. Backup: " + backup);
            EditorUtility.DisplayDialog("FoodTasteApp",
                "회원가입 UI와 Tab 이동 순서가 적용되었습니다.\n" +
                "씬 백업: " + backup + "\n" +
                "스크립트 변경 후 Unity가 재컴파일합니다.", "확인");
        }
        catch (Exception error)
        {
            Debug.LogError("[FoodTasteApp] Signup UI installation failed: " + error);
            EditorUtility.DisplayDialog("FoodTasteApp Signup UI", error.Message, "확인");
        }
    }

    private static RectTransform EnsureRect(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<RectTransform>();
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<RectTransform>();
    }

    private static Image EnsureImage(Transform target)
    {
        Image graphic = target.GetComponent<Image>();
        if (graphic == null) graphic = target.gameObject.AddComponent<Image>();
        return graphic;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(.5f, .5f);
        rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void SetSprite(Image target, Sprite sprite, bool preserve)
    {
        target.enabled = true;
        target.sprite = sprite;
        target.type = Image.Type.Simple;
        target.preserveAspect = preserve;
        target.color = Color.white;
        target.raycastTarget = false;
    }

    private static void Label(Transform signup, string targetName, Sprite sprite, Vector2 position)
    {
        Transform label = signup.Find(targetName);
        Place(label.GetComponent<RectTransform>(), position, new Vector2(275f, 92f));
        foreach (var txt in label.GetComponents<TMP_Text>()) txt.enabled = false;
        RectTransform art = EnsureRect(label, "SignupLabelArtwork");
        Stretch(art);
        SetSprite(EnsureImage(art), sprite, true);
    }

    private static void SetInput(Transform target, Sprite normal, Sprite focused, Vector2 position)
    {
        Place(target.GetComponent<RectTransform>(), position, new Vector2(620f, 78f));
        Image image = EnsureImage(target);
        image.sprite = normal;
        image.color = Color.white;
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.raycastTarget = true;
        TMP_InputField field = target.GetComponent<TMP_InputField>();
        field.targetGraphic = image;
        field.transition = Selectable.Transition.SpriteSwap;
        SpriteState ss = field.spriteState;
        ss.highlightedSprite = focused;
        ss.selectedSprite = focused;
        ss.pressedSprite = focused;
        field.spriteState = ss;
        if (field.textComponent != null)
        {
            field.textComponent.fontSize = 36;
            field.textComponent.color = new Color(.21f, .12f, .07f, 1f);
        }
        TMP_Text hint = field.placeholder as TMP_Text;
        if (hint != null)
        {
            hint.fontSize = 31;
            hint.color = new Color(.43f, .34f, .25f, .77f);
        }
    }

    private static void SetDropdown(Transform target, Sprite art, Vector2 position, float width)
    {
        Place(target.GetComponent<RectTransform>(), position, new Vector2(width, 76f));
        Image image = EnsureImage(target);
        image.sprite = art;
        image.color = Color.white;
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.raycastTarget = true;
        TMP_Dropdown dropdown = target.GetComponent<TMP_Dropdown>();
        dropdown.targetGraphic = image;
        if (dropdown.captionText != null)
        {
            dropdown.captionText.fontSize = 31;
            dropdown.captionText.color = new Color(.25f, .13f, .07f, 1f);
        }
        Transform arrow = target.Find("Arrow");
        if (arrow != null)
        {
            Image originalArrow = arrow.GetComponent<Image>();
            if (originalArrow != null) originalArrow.enabled = false;
        }
    }

    private static void SetButton(Transform target, Sprite image, Vector2 position)
    {
        Place(target.GetComponent<RectTransform>(), position, new Vector2(300f, 135f));
        Image art = EnsureImage(target);
        SetSprite(art, image, true);
        art.raycastTarget = true;
        Button button = target.GetComponent<Button>();
        button.targetGraphic = art;
        foreach (TMP_Text txt in target.GetComponentsInChildren<TMP_Text>(true))
            txt.enabled = false;
    }

    private static void PatchSignupTabCode()
    {
        string text = File.ReadAllText(SignupCode);
        if (text.Contains("FindSelectableOnDown() used spatial/auto navigation"))
            return; // Already fixed (for example, pulled from GitHub).
        if (!text.Contains("FindSelectableOnDown()"))
            throw new InvalidOperationException("Unexpected local SignupPanelManager navigation code: not replacing it automatically.");
        int start = text.IndexOf("    private void Update()", StringComparison.Ordinal);
        int end = text.IndexOf("    private void SetText()", start >= 0 ? start : 0, StringComparison.Ordinal);
        if (start < 0 || end <= start)
            throw new InvalidOperationException("SignupPanelManager.Update format changed; Tab patch not applied.");
        string replacement = @"    private void Update()
    {
        // Only handle signup navigation while the signup page is visible.
        // FindSelectableOnDown() used spatial/auto navigation and sometimes
        // jumped over PWDCheckInput. Use an explicit, stable field order.
        if (SignupPanel == null || !SignupPanel.activeInHierarchy)
            return;

        if (PWDInput.isFocused || PWDCHeckInput.isFocused || IDInput.isFocused)
            Input.imeCompositionMode = IMECompositionMode.Off;
        else
            Input.imeCompositionMode = IMECompositionMode.On;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable[] order = {
                IDInput, PWDInput, PWDCHeckInput, NickNameInput,
                TenDigitInput, OneDigitInput, DoRegionInput, SiguRegionInput
            };
            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject : null;
            Selectable current = selected != null ? selected.GetComponent<Selectable>() : null;
            int index = Array.IndexOf(order, current);
            int step = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -1 : 1;
            int nextIndex = index < 0 ? (step > 0 ? 0 : order.Length - 1)
                : (index + step + order.Length) % order.Length;
            if (current is TMP_InputField currentInput)
                currentInput.DeactivateInputField();
            Selectable target = order[nextIndex];
            if (target != null)
            {
                target.Select();
                if (target is TMP_InputField input)
                    input.ActivateInputField();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Enter on a dropdown selects its option; it must not submit signup.
            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null || selected.GetComponent<TMP_Dropdown>() == null)
                SignupBtn.onClick.Invoke();
        }
    }

";
        string backupPath = SignupCode + ".signup_tab_backup_" +
                            DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
        File.Copy(SignupCode, backupPath, false);
        File.WriteAllText(SignupCode, text.Substring(0, start) + replacement + text.Substring(end),
            new UTF8Encoding(false));
        AssetDatabase.ImportAsset(SignupCode, ImportAssetOptions.ForceUpdate);
    }
}
#endif
