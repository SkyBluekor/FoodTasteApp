using System;
using UnityEngine;

/// <summary>
/// Resolves the local MySQL password without putting a secret in source control.
/// Unity Hub/Editor can keep an old process environment after Windows user
/// environment variables have changed, so fall back to the persistent user value.
/// </summary>
public static class FoodTasteDbPassword
{
    private const string Variable = "FOODTASTE_DB_PASSWORD";
    private static bool missingReported;
    private static bool fallbackReported;

    public static string Resolve()
    {
        string value = Environment.GetEnvironmentVariable(Variable);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        try
        {
            value = Environment.GetEnvironmentVariable(Variable, EnvironmentVariableTarget.User);
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!fallbackReported)
                {
                    fallbackReported = true;
                    Debug.Log("[FoodTasteDB] Reading MySQL password from Windows user environment.");
                }
                return value;
            }
        }
        catch (PlatformNotSupportedException)
        {
            // Some non-Windows players do not expose per-user environment settings.
        }
        catch (NotSupportedException)
        {
            // Fall through to the descriptive diagnostic below.
        }

        if (!missingReported)
        {
            missingReported = true;
            Debug.LogError(
                "[FoodTasteDB] MySQL password is missing from both the Unity process " +
                "and the Windows user environment. Set FOODTASTE_DB_PASSWORD to the " +
                "local MySQL password; never use the FoodTasteApp demo login password.");
        }
        return string.Empty;
    }
}
