#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Replace only the IntroPanel artwork and layout in IntroScene.
/// Existing Button, TMP_InputField, and other scene behaviours are preserved.
/// Menu: Tools > FoodTasteApp > Apply Intro Artwork
/// </summary>
public static class FoodTasteIntroArtInstaller
{
    private const string ScenePath = "Assets/Scenes/IntroScene.unity";
    private const string ArtFolder = "Assets/Resources/FoodTasteIntro/";

    private static readonly string[] RequiredArt =
    {
        "IntroBackground.png", "FoodTasteAppLogo.png", "IdLabel.png",
        "PasswordLabel.png", "SignupButton.png", "LoginButton.png"
    };

    [MenuItem("Tools/FoodTasteApp/Apply Intro Artwork")]
    public static void Apply()
    {
        var missing = RequiredArt.Where(name =>
            !System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath,
                "Resources/FoodTasteIntro", name))).ToArray();
        if (missing.Length > 0)
        {
            Debug.LogError("FoodTasteApp Intro artwork missing: " + string.Join(", ", missing));
            EditorUtility.DisplayDialog("FoodTasteApp", "Missing artwork:\n" +
                string.Join("\n", missing), "OK");
            return;
        }

        foreach (string name in RequiredArt)
        {
            string path = ArtFolder + name;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite ||
                                     importer.spriteImportMode != SpriteImportMode.Single))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        Sprite[] sprite = RequiredArt.Select(name =>
            AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + name)).ToArray();
        if (sprite.Any(s => s == null))
        {
            Debug.LogError("FoodTasteApp: some sprites failed to import.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("FoodTasteApp: scene installation cancelled.");
            return;
        }
        if (!System.IO.File.Exists(ScenePath))
        {
            Debug.LogError("FoodTasteApp: IntroScene.unity not found.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform intro = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .FirstOrDefault(t => t.name == "IntroPanel");
        if (intro == null)
        {
            Debug.LogError("FoodTasteApp: IntroPanel not found, no scene changes applied.");
            return;
        }

        ReplaceImage(intro, sprite[0], false);
        Place(intro, Vector2.zero, new Vector2(1920f, 1080f));
        var title = Find(intro, "Image");
        ReplaceImage(title, sprite[1], true);
        Place(title, new Vector2(0f, 286f), new Vector2(950f, 316f));

        SetLabel(Find(intro, "IDText"), sprite[2], new Vector2(-285f, 34f));
        SetLabel(Find(intro, "PWDText"), sprite[3], new Vector2(-285f, -79f));

        SetInput(Find(intro, "IDInputField"), new Vector2(155f, 34f));
        SetInput(Find(intro, "PwdInputField"), new Vector2(155f, -79f));

        SetButton(Find(intro, "SignupEnterBtn"), sprite[4], new Vector2(-175f, -248f));
        SetButton(Find(intro, "LoginBtn"), sprite[5], new Vector2(175f, -248f));

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            Debug.LogError("FoodTasteApp: IntroScene failed to save.");
            return;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("FoodTasteApp: Intro artwork applied and IntroScene saved. Existing scripts and button events preserved.");
        EditorUtility.DisplayDialog("FoodTasteApp", "IntroScene artwork applied and saved.", "OK");
    }

    private static Transform Find(Transform parent, string name)
    {
        if (parent == null) throw new InvalidOperationException("Missing parent for " + name);
        Transform child = parent.Find(name);
        if (child == null) throw new InvalidOperationException("Intro UI object not found: " + name);
        return child;
    }

    private static void ReplaceImage(Transform obj, Sprite image, bool preserveAspect)
    {
        if (obj == null) throw new InvalidOperationException("Missing UI Image target");
        Image graphic = obj.GetComponent<Image>();
        if (graphic == null) throw new InvalidOperationException("No UI Image on " + obj.name);
        graphic.sprite = image;
        graphic.type = Image.Type.Simple;
        graphic.preserveAspect = preserveAspect;
        graphic.color = Color.white;
        if (obj.name != "LoginBtn" && obj.name != "SignupEnterBtn")
            graphic.raycastTarget = false;
    }

    private static void Place(Transform obj, Vector2 pos, Vector2 size)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) throw new InvalidOperationException("No RectTransform on " + obj.name);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
    }

    private static void SetLabel(Transform label, Sprite image, Vector2 pos)
    {
        Place(label, pos, new Vector2(290f, 98f));
        Transform child = Find(label, "Image");
        ReplaceImage(child, image, true);
        Place(child, Vector2.zero, new Vector2(290f, 98f));
        foreach (TMP_Text text in label.GetComponents<TMP_Text>())
            text.enabled = false;
    }

    private static void SetInput(Transform input, Vector2 pos)
    {
        Place(input, pos, new Vector2(420f, 78f));
        Image background = input.GetComponent<Image>();
        if (background != null)
            background.color = new Color(1f, 0.98f, 0.92f, 0.97f);
    }

    private static void SetButton(Transform target, Sprite image, Vector2 pos)
    {
        ReplaceImage(target, image, true);
        Place(target, pos, new Vector2(315f, 111f));
        foreach (TMP_Text text in target.GetComponentsInChildren<TMP_Text>(true))
            text.enabled = false;
    }
}
#endif
