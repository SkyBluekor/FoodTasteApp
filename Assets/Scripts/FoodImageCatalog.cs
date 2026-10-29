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
    private static bool missingFolderWarned;

    public static Sprite Get(int foodNumber)
    {
        if (foodNumber <= 0)
            return null;

        Sprite cached;
        if (Sprites.TryGetValue(foodNumber, out cached))
            return cached;
        if (Missing.Contains(foodNumber))
            return null;

        string filename = FindImage(foodNumber);
        if (filename == null)
        {
            Missing.Add(foodNumber);
            if (!missingFolderWarned)
            {
                missingFolderWarned = true;
                Debug.LogWarning("[FoodTasteApp] A food image was not found for FoodNum=" + foodNumber +
                    ". Confirm that Assets/Images/FoodImg contains numbered JPG/PNG photos.");
            }
            return null;
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
                return null;
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
            return null;
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
        return sprite != null;
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
