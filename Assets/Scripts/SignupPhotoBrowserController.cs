using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the EXISTING FoodTasteApp profile file browser on demand.
/// Does not replace the legacy file selection or image-loading behaviour.
/// </summary>
public sealed class SignupPhotoBrowserController : MonoBehaviour
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
