using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation-only mirror for the existing ImageLoader preview.
/// No file browsing, selection or database behavior is replaced.
/// </summary>
public sealed class FoodTastePickerPreviewMirror : MonoBehaviour
{
    public GameObject Browser;
    public Image SourceImage;
    public TMP_Text SourceInfo;
    public Image PreviewImage;
    public TMP_Text PreviewInfo;
    public TMP_Text EmptyHint;

    private Sprite lastSprite;
    private string lastInfo = "";

    private void OnEnable()
    {
        lastSprite = null;
        lastInfo = null;
    }

    private void LateUpdate()
    {
        if (Browser == null || !Browser.activeInHierarchy || PreviewImage == null)
            return;

        bool sourceVisible = SourceImage != null && SourceImage.gameObject.activeInHierarchy;
        Sprite sprite = sourceVisible && !string.IsNullOrEmpty(ImageLoader.ImgDirectory)
            ? SourceImage.sprite : null;
        string info = sprite != null && SourceInfo != null ? SourceInfo.text : "";

        if (sprite == lastSprite && info == lastInfo)
            return;

        lastSprite = sprite;
        lastInfo = info;
        PreviewImage.sprite = sprite;
        PreviewImage.enabled = sprite != null;
        PreviewImage.preserveAspect = true;

        if (EmptyHint != null)
            EmptyHint.text = sprite == null ? "JPG 또는 PNG 이미지를 선택하세요." : "";
        if (PreviewInfo != null)
            PreviewInfo.text = sprite != null ? info : "파일을 선택하면 이미지 정보가 표시됩니다.";
    }
}
