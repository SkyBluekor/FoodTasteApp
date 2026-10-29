using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MixPic : MonoBehaviour
{
    public string imgpath1;
    public string imgpath2;
    public Texture2D mergedImage;
    public Image mergeImg;

    // Start is called before the first frame update
    void Start()
    {
        imgpath1 = Path.Combine(Application.dataPath, "Images/FoodImg/1240.jpg");
        imgpath2 = Path.Combine(Application.dataPath, "Images/FoodImg/1241.jpg");
        mergeimg();
    }

    void mergeimg()
    {
        // Load the images from file paths
        Texture2D image1 = LoadImageFromFile(imgpath1);
        Texture2D image2 = LoadImageFromFile(imgpath2);

        // Ensure that the images are loaded successfully
        if (image1 == null || image2 == null)
        {
            Debug.LogError("Failed to load the images from file.");
            return;
        }

        // Ensure that the images have the same dimensions
        if (image1.width != image2.width || image1.height != image2.height)
        {
            Debug.LogError("Images must have the same dimensions for merging.");
            return;
        }

        // Create a new texture to hold the merged image
        mergedImage = new Texture2D(image1.width, image1.height);

        // Merge the images by alternating pixels
        for (int y = 0; y < image1.height; y++)
        {
            for (int x = 0; x < image1.width; x++)
            {
                // Alternate between the pixels of image1 and image2
                if ((x + y) % 2 == 0)
                    mergedImage.SetPixel(x, y, image1.GetPixel(x, y));
                else
                    mergedImage.SetPixel(x, y, image2.GetPixel(x, y));
            }
        }

        // Apply the changes to the merged image
        mergedImage.Apply();

        mergeImg.sprite = Sprite.Create(mergedImage, new Rect(0, 0, mergedImage.width, mergedImage.height), Vector2.one * 0.5f);
    }

    Texture2D LoadImageFromFile(string filePath)
    {
        // Read the image data from the file
        byte[] imageData = File.ReadAllBytes(filePath);

        // Create a new texture and load the image data
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(imageData))
        {
            Debug.LogError("Failed to load image from file: " + filePath);
            return null;
        }

        return texture;
    }
}