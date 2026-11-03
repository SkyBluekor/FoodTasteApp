using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays a saved profile image, or the shared chef image when none is available.</summary>
public static class DefaultProfileImage
{
    private static Sprite fallback;

    public static void Apply(Image image, byte[] imageData = null)
    {
        if (image == null) return;
        if (fallback == null) fallback = Resources.Load<Sprite>("FoodTasteMainUI/DefaultProfile");
        image.sprite = fallback;
        image.color = Color.white;
        image.preserveAspect = true;
        if (imageData == null || imageData.Length == 0) return;

        var texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageData))
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
        else
            Object.Destroy(texture);
    }
}
