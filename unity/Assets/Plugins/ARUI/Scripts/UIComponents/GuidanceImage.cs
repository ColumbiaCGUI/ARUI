using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class GuidanceImage : Singleton<GuidanceImage>
{
    // Reference to the Image component on the GameObject
    private Image targetImage;

    private bool _registered = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Image component attached to this GameObject
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogError("No Image component found on the GameObject.");
        }
        targetImage.enabled = false;
    }

    /// <summary>
    /// Loads a Base64 encoded image, adjusts its width and height, and applies it to the Image component
    /// </summary>
    /// <param name="base64String">Base64 encoded image string</param>
    public void LoadBase64Image(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            Debug.LogError("Base64 string is null or empty.");
            return;
        }

        try
        {
            // Decode Base64 string to byte array
            byte[] imageData = Convert.FromBase64String(base64String);

            // Create a new Texture2D and load the image data
            Texture2D texture = new Texture2D(2, 2); // Size doesn't matter; it will resize when loading the image
            if (!texture.LoadImage(imageData))
            {
                Debug.LogError("Failed to load image from Base64 string.");
                return;
            }

            // Create a Sprite from the Texture2D
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Assign the Sprite to the Image component
            targetImage.sprite = sprite;

            // Calculate new dimensions to ensure the max dimension is 1
            float maxDimension = Mathf.Max(texture.width, texture.height);
            float newWidth = texture.width / maxDimension; // Normalize width
            float newHeight = texture.height / maxDimension; // Normalize height

            // Adjust RectTransform size to the normalized dimensions
            RectTransform rectTransform = targetImage.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

            if (_registered == false)
            {
                AngelARUI.Instance.RegisterTetheredObject(gameObject.GetInstanceID(), gameObject);
                AngelARUI.Instance.Tether(gameObject.GetInstanceID());
                targetImage.enabled = true;
                _registered = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while processing Base64 image: {ex.Message}");
        }
    }
}