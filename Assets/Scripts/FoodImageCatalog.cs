using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using MySql.Data.MySqlClient;

/// <summary>
/// Resolves database foods against the original XML and displays reviewed photos.
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
    private static bool fallbackAttempted;
    private static Sprite fallbackSprite;

    // The numbered collection through 996 follows the XML sequence. From
    // 1000 onward the image sequence differs, so never use those IDs directly.
    private static bool IsUsablePhotoNumber(int number)
    {
        return number > 0 && number <= 996;
    }

    private static readonly Dictionary<int, int> RepresentativeMap = new Dictionary<int, int>();
    private static bool representativeMappingLoaded;

    private static void EnsureRepresentativeMappingLoaded()
    {
        if (representativeMappingLoaded) return;
        representativeMappingLoaded = true;
        TextAsset asset = Resources.Load<TextAsset>("FoodImageMapping");
        if (asset == null) return;
        using (var reader = new StringReader(asset.text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.TrimStart('\uFEFF').Split(',');
                int source, target;
                if (fields.Length >= 2 && int.TryParse(fields[0], out source) &&
                    int.TryParse(fields[1], out target) && source > 0 && IsUsablePhotoNumber(target))
                    RepresentativeMap[source] = target;
            }
        }
    }
    private static readonly Dictionary<int, int> DatabaseToPhotoNumber = new Dictionary<int, int>();
    private static bool databaseMappingLoaded;

    // With domain reload disabled, static fields survive between Play sessions,
    // but Unity may already have destroyed the sprites they referenced.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForPlaySession()
    {
        Sprites.Clear();
        Missing.Clear();
        CoverSprites.Clear();
        ReportedMissing.Clear();
        RepresentativeMap.Clear();
        DatabaseToPhotoNumber.Clear();
        representativeMappingLoaded = false;
        databaseMappingLoaded = false;
        fallbackAttempted = false;
        fallbackSprite = null;
    }

    private static void EnsureDatabaseMappingLoaded()
    {
        if (databaseMappingLoaded) return;
        databaseMappingLoaded = true;
        try
        {
            TextAsset asset = Resources.Load<TextAsset>("XML/전국통합식품영양성분정보(음식)표준데이터");
            if (asset == null) throw new InvalidOperationException("Food XML is missing");
            var xml = new XmlDocument();
            xml.LoadXml(asset.text);
            var namesByNumber = new Dictionary<int, string>();
            var numbersByName = new Dictionary<string, int>(StringComparer.Ordinal);
            var earlyNumbersByName = new Dictionary<string, int>(StringComparer.Ordinal);
            int number = 0;
            foreach (XmlNode node in xml.SelectNodes("dataGrid/records/record"))
            {
                number++;
                XmlNode nameNode = node.SelectSingleNode("식품명");
                string name = nameNode == null ? "" : nameNode.InnerText.Trim();
                namesByNumber[number] = name;
                if (name.Length == 0) continue;
                if (IsUsablePhotoNumber(number) && !earlyNumbersByName.ContainsKey(name))
                    earlyNumbersByName.Add(name, number);
                // A repeated name cannot safely identify one particular photo.
                if (numbersByName.ContainsKey(name)) numbersByName[name] = 0;
                else numbersByName.Add(name, number);
            }
            string connectionString = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT FoodNum, FoodName FROM fooddata", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int dbNumber = Convert.ToInt32(reader.GetValue(0));
                        string dbName = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                        string expected;
                        int photoNumber;
                        if (dbName.Length > 0 && earlyNumbersByName.TryGetValue(dbName, out photoNumber) &&
                            (!namesByNumber.TryGetValue(dbNumber, out expected) || expected != dbName || !IsUsablePhotoNumber(dbNumber)))
                            DatabaseToPhotoNumber[dbNumber] = photoNumber;
                        else if (namesByNumber.TryGetValue(dbNumber, out expected) && expected == dbName && dbName.Length > 0)
                            DatabaseToPhotoNumber[dbNumber] = dbNumber;
                        else if (numbersByName.TryGetValue(dbName, out photoNumber) && photoNumber > 0)
                            DatabaseToPhotoNumber[dbNumber] = photoNumber;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DatabaseToPhotoNumber.Clear();
            Debug.LogWarning("[FoodTasteApp] Could not verify food image mapping; using fallback: " + ex.Message);
        }
    }


    private static Sprite GetFallback()
    {
        if (fallbackAttempted && fallbackSprite != null)
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
            return GetFallback();

        EnsureDatabaseMappingLoaded();
        int photoNumber;
        if (!DatabaseToPhotoNumber.TryGetValue(foodNumber, out photoNumber))
            return GetFallback();

        Sprite cached;
        if (Sprites.TryGetValue(foodNumber, out cached))
        {
            if (cached != null) return cached;
            Sprites.Remove(foodNumber);
        }
        if (Missing.Contains(foodNumber))
            return GetFallback();

        string filename = IsUsablePhotoNumber(photoNumber) ? FindImage(photoNumber) : null;
        if (filename == null && IsUsablePhotoNumber(photoNumber))
        {
            EnsureRepresentativeMappingLoaded();
            int representative;
            if (RepresentativeMap.TryGetValue(photoNumber, out representative))
                filename = FindImage(representative);
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
            if (!CoverSprites.TryGetValue(key, out cropped) || cropped == null)
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

    /// <summary>
    /// Top panel titles used a very large font in a 50-unit-high box.
    /// They became vertically clipped when the panels were scaled 0.2x/0.78y.
    /// Fit their text within the existing title strip, without touching buttons.
    /// </summary>
    public static void FitMainPanelTitles()
    {
        GameObject function = GameObject.Find("FunctionPanel");
        if (function == null)
            return;
        Transform root = function.transform;
        FitPanelTitle(root.Find("GetPreFoodPanel/PanelBackGroundImg/Text (TMP)"));
        FitPanelTitle(root.Find("GetSimUserHighFoodPanel/PanelBackGrounImg/Text (TMP)"));
        FitPanelTitle(root.Find("PoPularFoodPanel/PanelBackGroundImg/Text (TMP)"));
    }

    private static void FitPanelTitle(Transform title)
    {
        if (title == null)
            return;
        RectTransform rect = title.GetComponent<RectTransform>();
        TMPro.TextMeshProUGUI label = title.GetComponent<TMPro.TextMeshProUGUI>();
        if (rect == null || label == null)
            return;
        RectTransform parent = rect.parent as RectTransform;
        float correction = parent != null ? HorizontalCorrection(parent) : 1f;
        rect.localScale = new Vector3(correction, 1f, 1f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 488f);
        rect.sizeDelta = new Vector2(1790f / correction, 97f);
        label.enableWordWrapping = true;
        label.enableAutoSizing = true;
        label.fontSizeMin = 22f;
        label.fontSizeMax = 42f;
        label.fontSize = 42f;
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.overflowMode = TMPro.TextOverflowModes.Ellipsis;
    }

    public static void FitRecommendationRow(Transform row)
    {
        if (row == null)
            return;
        FitRowText(row, "PreFoodNameText", -465f, 665f, 43, 25);
        FitRowText(row, "PreScoreText", 685f, 950f, 43, 28);
        FitRowText(row, "PreText", 685f, 950f, 25, 17);
        Transform caption = row.Find("PreText");
        if (caption != null)
        {
            RectTransform rt = caption.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 28f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 39f);
        }
        Transform score = row.Find("PreScoreText");
        if (score != null)
        {
            RectTransform rt = score.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -22f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 55f);
        }
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
        {
            RectTransform rt = score.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -22f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 55f);
        }
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
        image.preserveAspect = false;
        image.type = Image.Type.Simple;
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
