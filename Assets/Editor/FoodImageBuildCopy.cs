#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
/// Numbered FoodImg JPEG/PNG photos are loaded from files in the original app.
/// Unity does not automatically bundle unreferenced Assets/Images photos.
/// Copies them next to the Windows executable after a successful build.
/// </summary>
public static class FoodImageBuildCopy
{
    [PostProcessBuild(10)]
    public static void CopyFoodImagesToWindowsBuild(BuildTarget target, string builtProject)
    {
        if (target != BuildTarget.StandaloneWindows64 && target != BuildTarget.StandaloneWindows)
            return;

        string source = Path.Combine(Application.dataPath, "Images", "FoodImg");
        if (!Directory.Exists(source))
        {
            Debug.LogWarning("[FoodTasteApp] FoodImg source directory missing; image files were not copied.");
            return;
        }

        string outputDir = Path.GetDirectoryName(builtProject);
        if (string.IsNullOrEmpty(outputDir))
            throw new InvalidOperationException("Unable to resolve output directory of the Windows player.");

        string destination = Path.Combine(outputDir, "FoodImg");
        Directory.CreateDirectory(destination);

        int copied = 0;
        foreach (string path in Directory.GetFiles(source))
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                continue;

            File.Copy(path, Path.Combine(destination, Path.GetFileName(path)), true);
            copied++;
        }
        Debug.Log("[FoodTasteApp] Copied " + copied +
            " numbered food images to " + destination + " for standalone playback.");
    }
}
