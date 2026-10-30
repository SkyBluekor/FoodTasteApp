using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Opens the existing registration profile-image picker without replacing ImageLoader.
/// Serialized references are assigned by FoodTasteSignupSceneFix in the Unity Editor.
/// </summary>
public sealed class SignupPhotoBrowserController : MonoBehaviour
{
    public GameObject Browser;
    public Button OpenButton;
    public Button CloseButton;

    private void Awake()
    {
        if (Browser != null) Browser.SetActive(false);
        if (OpenButton != null) OpenButton.onClick.AddListener(Open);
        if (CloseButton != null) CloseButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        if (OpenButton != null) OpenButton.onClick.RemoveListener(Open);
        if (CloseButton != null) CloseButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        if (Browser == null) return;
        Browser.SetActive(true);
        Browser.transform.SetAsLastSibling();
        if (CloseButton != null) CloseButton.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (Browser != null) Browser.SetActive(false);
        if (CloseButton != null) CloseButton.gameObject.SetActive(false);
    }
}
