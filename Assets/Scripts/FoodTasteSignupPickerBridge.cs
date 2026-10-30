using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Opens the existing registration profile-image picker without replacing ImageLoader.
/// Serialized references are assigned by the signup layout tools in the Unity Editor.
/// </summary>
public sealed class FoodTasteSignupPickerBridge : MonoBehaviour
{
    public GameObject Browser;
    public Button OpenButton;
    public Button CloseButton;

    private void Awake()
    {
        if (Browser != null)
            Browser.SetActive(false);
        if (OpenButton != null)
            OpenButton.onClick.AddListener(Open);
        if (CloseButton != null)
            CloseButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        if (OpenButton != null)
            OpenButton.onClick.RemoveListener(Open);
        if (CloseButton != null)
            CloseButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        if (Browser == null) return;
        Browser.SetActive(true);
        Browser.transform.SetAsLastSibling();
        if (CloseButton != null)
        {
            CloseButton.gameObject.SetActive(true);
            CloseButton.transform.SetAsLastSibling();
        }
    }

    public void Close()
    {
        if (CloseButton != null)
            CloseButton.gameObject.SetActive(false);
        if (Browser != null)
            Browser.SetActive(false);
    }
}
