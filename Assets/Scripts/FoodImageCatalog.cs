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
    /// Place a 220x92 food photo INSIDE the existing 350x100 RankPlaceImg slot.
    /// Preserve the original 1st/2nd/3rd rank artwork rather than replacing it.
    /// The photo sits on the right, leaving the left rank marker uncovered.
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

        RectTransform photoRect = go.GetComponent<RectTransform>();
        photoRect.anchorMin = new Vector2(0.5f, 0.5f);
        photoRect.anchorMax = new Vector2(0.5f, 0.5f);
        photoRect.pivot = new Vector2(0.5f, 0.5f);
        photoRect.anchoredPosition = new Vector2(62f, 0f);
        photoRect.sizeDelta = new Vector2(220f, 92f);

        Image photoImage = go.GetComponent<Image>();
        photoImage.raycastTarget = false;
        photoImage.type = Image.Type.Simple;
        photoImage.preserveAspect = true;

        Sprite original = Get(foodNumber);
        if (original == null)
        {
            photoImage.sprite = null;
            photoImage.color = Color.clear;
            return false;
        }
        if (original == fallbackSprite)
        {
            // Fallback needs the whole plate icon; do not center-crop it.
            photoImage.sprite = original;
            photoImage.color = Color.white;
            go.transform.SetAsLastSibling();
            return false;
        }

        // Center crop to fill the photo area without distortion or letterboxing.
        const float width = 220f;
        const float height = 92f;
        float aspect = width / height;
        string key = foodNumber + "_" + Mathf.RoundToInt(aspect * 100f);
        Sprite cropped;
        if (!CoverSprites.TryGetValue(key, out cropped))
        {
            Texture2D tex = original.texture;
            float cropWidth = tex.width;
            float cropHeight = tex.height;
            if (cropWidth / cropHeight > aspect)
                cropWidth = cropHeight * aspect;
            else
                cropHeight = cropWidth / aspect;

            Rect region = new Rect((tex.width - cropWidth) * 0.5f,
                (tex.height - cropHeight) * 0.5f,
                Mathf.Max(1f, cropWidth), Mathf.Max(1f, cropHeight));
            cropped = Sprite.Create(tex, region, new Vector2(0.5f, 0.5f));
            cropped.name = "FoodPhotoCover_" + foodNumber;
            CoverSprites.Add(key, cropped);
        }

        photoImage.sprite = cropped;
        photoImage.color = Color.white;
        go.transform.SetAsLastSibling();
        return true;
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
        GameObject go;
        if (existing != null)
            go = existing.gameObject;
        else
        {
            go = new GameObject("FoodPhotoThumbnail", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(row, false);
        }

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(leftPadding, 0f);
        rect.sizeDelta = new Vector2(size, size);

        var graphic = go.GetComponent<Image>();
        graphic.raycastTarget = false;
        Apply(graphic, foodNumber);
        // Thumbnail should not hide existing buttons or text.
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
