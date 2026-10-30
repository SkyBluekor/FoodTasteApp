using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads any food photo by the Fooddata.FoodNum primary key.
/// In Editor: Assets/Images/FoodImg/{FoodNum}.jpg/png.
/// In builds: StreamingAssets/FoodImg, or a FoodImg directory beside the executable.
/// Missing files are handled without crashing or showing another food's photograph.
/// </summary>
public static class FoodImageCatalog
{
    private static readonly Dictionary<int, Sprite> Sprites = new Dictionary<int, Sprite>();
    private static readonly HashSet<int> Missing = new HashSet<int>();
    private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png" };
    private static readonly Dictionary<string, Sprite> CoverSprites = new Dictionary<string, Sprite>();
    private static readonly HashSet<int> ReportedMissing = new HashSet<int>();
    private static readonly Dictionary<int, int> RepresentativeMap = new Dictionary<int, int>();
    private static bool mappingLoaded;
    private static bool fallbackAttempted;
    private static Sprite fallbackSprite;

    // Unity embeds Assets/Resources/FoodImageMapping.csv in both Editor and builds.
    private static void EnsureMappingLoaded()
    {
        if (mappingLoaded)
            return;
        mappingLoaded = true;
        TextAsset asset = Resources.Load<TextAsset>("FoodImageMapping");
        if (asset == null)
        {
            Debug.LogWarning("[FoodTasteApp] FoodImageMapping.csv missing from Resources; photos will use exact IDs.");
            return;
        }

        using (var reader = new StringReader(asset.text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("FoodNum", StringComparison.OrdinalIgnoreCase))
                    continue;

                string[] fields = line.TrimStart('\uFEFF').Split(',');
                int source, target;
                if (fields.Length >= 2 &&
                    int.TryParse(fields[0].Trim(), out source) &&
                    int.TryParse(fields[1].Trim(), out target) &&
                    source > 0 && target > 0)
                    RepresentativeMap[source] = target;
            }
        }
    }

    private static Sprite GetFallback()
    {
        if (fallbackAttempted)
            return fallbackSprite;
        fallbackAttempted = true;
        string filename = FindNonNumberedImage("FoodImageFallback.png");
        if (filename == null)
            return null;
        try
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(filename), false))
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }
            texture.name = "FoodImageFallback";
            fallbackSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FoodTasteApp] Could not load fallback image: " + ex.Message);
        }
        return fallbackSprite;
    }

    private static string FindNonNumberedImage(string filename)
    {
        string[] dirs = {
            Path.Combine(Application.dataPath, "Images", "FoodImg"),
            Path.Combine(Application.streamingAssetsPath, "FoodImg"),
            Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", "FoodImg")
        };
        foreach (var dir in dirs)
        {
            string file = Path.Combine(dir, filename);
            if (File.Exists(file))
                return file;
        }
        return null;
    }

    public static Sprite Get(int foodNumber)
    {
        if (foodNumber <= 0)
            return null;

        Sprite cached;
        if (Sprites.TryGetValue(foodNumber, out cached))
            return cached;
        if (Missing.Contains(foodNumber))
            return GetFallback();

        // Direct FoodNum files always take priority over any representative mapping.
        string filename = FindImage(foodNumber);
        if (filename == null)
        {
            EnsureMappingLoaded();
            int representativeId;
            if (RepresentativeMap.TryGetValue(foodNumber, out representativeId) && representativeId != foodNumber)
                filename = FindImage(representativeId);
        }
        if (filename == null)
        {
            Missing.Add(foodNumber);
            if (ReportedMissing.Add(foodNumber))
                Debug.LogWarning("[FoodTasteApp] FoodImg file missing: FoodNum=" + foodNumber +
                    "; showing fallback if available.");
            return GetFallback();
        }

        try
        {
            byte[] raw = File.ReadAllBytes(filename);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, raw, false))
            {
                UnityEngine.Object.Destroy(texture);
                Missing.Add(foodNumber);
                Debug.LogWarning("[FoodTasteApp] Invalid food image: " + filename);
                return GetFallback();
            }
            texture.name = "FoodPhoto_" + foodNumber;
            var sprite = Sprite.Create(texture,
                new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = "FoodPhoto_" + foodNumber;
            Sprites[foodNumber] = sprite;
            return sprite;
        }
        catch (Exception ex)
        {
            Missing.Add(foodNumber);
            Debug.LogWarning("[FoodTasteApp] Could not read food image #" + foodNumber +
                ": " + ex.Message);
            return GetFallback();
        }
    }

    public static bool Apply(Image target, int foodNumber)
    {
        if (target == null)
            return false;

        Sprite sprite = Get(foodNumber);
        target.sprite = sprite;
        target.preserveAspect = true;
        // Keep an empty placeholder visible; don't leave a photo of another food.
        target.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
        return sprite != null && sprite != fallbackSprite;
    }

    /// <summary>
    /// The legacy three-column canvas scales X (0.2) and Y (0.78) differently.
    /// Fit a true 4:3 food image inside the EXISTING rank/photo area in screen
    /// space without moving the rows, changing the parent's scale or hiding text.
    /// </summary>
    public static bool ApplyToExistingSlot(Image slot, int foodNumber)
    {
        if (slot == null)
            return false;

        RectTransform parent = slot.rectTransform;
        Transform existing = parent.Find("FoodPhotoInsideSlot");
        GameObject go;
        if (existing != null)
            go = existing.gameObject;
        else
        {
            go = new GameObject("FoodPhotoInsideSlot", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
        }

        go.SetActive(true);
        float ratio = HorizontalCorrection(parent);
        const float imageHeight = 90f;
        float imageWidth = Mathf.Min(470f, imageHeight * (4f / 3f) * ratio);

        RectTransform photoRect = go.GetComponent<RectTransform>();
        photoRect.anchorMin = new Vector2(0.5f, 0.5f);
        photoRect.anchorMax = new Vector2(0.5f, 0.5f);
        photoRect.pivot = new Vector2(0.5f, 0.5f);
        // Align its LEFT edge with the existing RankPlaceImg left edge.
        photoRect.anchoredPosition = new Vector2((imageWidth - parent.rect.width) * 0.5f, 0f);
        photoRect.sizeDelta = new Vector2(imageWidth, imageHeight);
        photoRect.localScale = Vector3.one;

        Image photo = go.GetComponent<Image>();
        photo.type = Image.Type.Simple;
        photo.raycastTarget = false;
        // PreserveAspect=true makes a 4:3 source extremely skinny on this
        // non-uniformly scaled canvas. Cover is instead calculated in pixels.
        photo.preserveAspect = false;
        Sprite source = Get(foodNumber);
        if (source == null)
        {
            photo.sprite = null;
            photo.color = Color.clear;
            return false;
        }

        if (source == fallbackSprite)
            photo.sprite = source; // Never crop the missing-photo icon.
        else
        {
            // Center crop source to 4:3 BEFORE the panel's scale correction.
            string key = foodNumber + "_four_by_three";
            Sprite cropped;
            if (!CoverSprites.TryGetValue(key, out cropped))
            {
                Texture2D texture = source.texture;
                float width = texture.width;
                float height = texture.height;
                const float aspect = 4f / 3f;
                if (width / height > aspect)
                    width = height * aspect;
                else
                    height = width / aspect;
                cropped = Sprite.Create(texture,
                    new Rect((texture.width - width) * 0.5f,
                        (texture.height - height) * 0.5f, width, height),
                    new Vector2(0.5f, 0.5f));
                cropped.name = "FoodPhotoScreenCorrected_" + foodNumber;
                CoverSprites[key] = cropped;
            }
            photo.sprite = cropped;
        }

        photo.color = Color.white;
        go.transform.SetAsLastSibling();
        return source != fallbackSprite;
    }

    /// <summary>Compensate for the legacy UI's nonuniform parent scales.</summary>
    private static float HorizontalCorrection(RectTransform item)
    {
        float x = Mathf.Abs(item.lossyScale.x);
        float y = Mathf.Abs(item.lossyScale.y);
        return x > 0.0001f ? Mathf.Clamp(y / x, 0.5f, 8f) : 1f;
    }

    private static void FitRowText(Transform row, string childName,
        float left, float right, int fontMax, int fontMin)
    {
        Transform child = row.Find(childName);
        if (child == null)
            return;
        TMPro.TextMeshProUGUI label = child.GetComponent<TMPro.TextMeshProUGUI>();
        RectTransform rect = child.GetComponent<RectTransform>();
        if (label == null || rect == null)
            return;

        float correction = HorizontalCorrection(row.GetComponent<RectTransform>());
        // The child looks normal in screen pixels, but occupies the SAME
        // physical bounds as before: scale X and divide local width by X.
        rect.localScale = new Vector3(correction, 1f, 1f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2((left + right) * 0.5f, 0f);
        rect.sizeDelta = new Vector2((right - left) / correction, 98f);
        label.enableWordWrapping = true;
        label.enableAutoSizing = true;
        label.fontSizeMin = fontMin;
        label.fontSizeMax = fontMax;
        label.fontSize = fontMax;
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.overflowMode = TMPro.TextOverflowModes.Ellipsis;
    }

    public static void FitRecommendationRow(Transform row)
    {
        if (row == null)
            return;
        FitRowText(row, "PreFoodNameText", -465f, 665f, 43, 25);
        FitRowText(row, "PreScoreText", 685f, 950f, 45, 30);
    }

    public static void FitPopularRow(Transform row)
    {
        if (row == null)
            return;
        FitRowText(row, "PopularFoodNameText", -695f, 825f, 43, 25);
        FitRowText(row, "PopularScoreText", 850f, 1190f, 42, 24);
        // The small "search count" caption stays in its own column.
        FitRowText(row, "PopularText", 850f, 1190f, 28, 19);
        Transform caption = row.Find("PopularText");
        if (caption != null)
        {
            RectTransform rt = caption.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 27f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 42f);
        }
        Transform score = row.Find("PopularScoreText");
        if (score != null)
            score.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(score.GetComponent<RectTransform>().anchoredPosition.x, -24f);
    }

    public static void FitSimilarUserRow(Image image, TMPro.TextMeshProUGUI name,
        TMPro.TextMeshProUGUI score)
    {
        if (image == null)
            return;
        RectTransform photo = image.rectTransform;
        float correction = HorizontalCorrection(photo);
        const float height = 132f;
        float width = Mathf.Min(730f, height * (4f / 3f) * correction);
        photo.anchorMin = new Vector2(0.5f, 0.5f);
        photo.anchorMax = new Vector2(0.5f, 0.5f);
        photo.pivot = new Vector2(0.5f, 0.5f);
        photo.anchoredPosition = new Vector2(-960f + width * 0.5f, 0f);
        photo.sizeDelta = new Vector2(width, height);
        photo.localScale = Vector3.one;
        photo.preserveAspect = false;
        // Center-cropped source sprites can be used when available. A regular
        // 4:3 source also displays correctly once the rect's ratio is adjusted.
        if (name != null)
            FitRowText(name.transform.parent, name.name, -265f, 580f, 45, 27);
        if (score != null)
            FitRowText(score.transform.parent, score.name, 605f, 955f, 44, 25);
    }

    /// <summary>
    /// Clean up incorrectly inserted thumbnail from the old recommendation UI.
    /// Existing RankPlaceImg is not touched.
    /// </summary>
    public static void RemoveLegacyThumbnail(Transform row)
    {
        if (row == null)
            return;
        Transform old = row.Find("FoodPhotoThumbnail");
        if (old != null)
        {
            old.gameObject.SetActive(false);
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(old.gameObject);
            else
                UnityEngine.Object.DestroyImmediate(old.gameObject);
        }
    }

    /// <summary>
    /// Adds a thumbnail to an existing UI row, including rows with no Image child.
    /// A reused/pool row is updated in place without duplicating components.
    /// </summary>
    public static void ApplyThumbnail(RectTransform row, int foodNumber,
        float leftPadding = 12f, float size = 56f)
    {
        if (row == null)
            return;
        Transform existing = row.Find("FoodPhotoThumbnail");
        GameObject go = existing != null ? existing.gameObject :
            new GameObject("FoodPhotoThumbnail", typeof(RectTransform), typeof(Image));
        if (existing == null)
            go.transform.SetParent(row, false);
        go.SetActive(true);

        RectTransform rect = go.GetComponent<RectTransform>();
        float correction = HorizontalCorrection(row);
        float height = Mathf.Min(size, Mathf.Max(1f, row.rect.height - 5f));
        float width = height * (4f / 3f) * correction;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(leftPadding, 0f);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;

        Image graphic = go.GetComponent<Image>();
        graphic.raycastTarget = false;
        graphic.type = Image.Type.Simple;
        Apply(graphic, foodNumber);
        graphic.preserveAspect = false; // Rect has screen-correct 4:3 proportions.
        go.transform.SetAsFirstSibling();
    }

    private static string FindImage(int foodNumber)
    {
        string[] dirs = {
            Path.Combine(Application.dataPath, "Images", "FoodImg"),
            Path.Combine(Application.streamingAssetsPath, "FoodImg"),
            Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", "FoodImg")
        };
        foreach (string dir in dirs)
        {
            foreach (string ext in Extensions)
            {
                string candidate = Path.Combine(dir, foodNumber + ext);
                if (File.Exists(candidate))
                    return candidate;
            }
        }
        return null;
    }
}
