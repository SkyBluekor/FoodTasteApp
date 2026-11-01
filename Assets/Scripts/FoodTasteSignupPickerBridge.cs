using System.Collections;
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
        ScrollRect list = Browser.GetComponentInChildren<ScrollRect>(true);
        if (list != null && list.viewport != null)
        {
            // The legacy sprite-based stencil mask hides the generated rows on
            // some machines. A rectangular clip matches this scroll viewport.
            Mask stencilMask = list.viewport.GetComponent<Mask>();
            if (stencilMask != null)
                stencilMask.enabled = false;
            RectMask2D rectMask = list.viewport.GetComponent<RectMask2D>();
            if (rectMask == null)
                rectMask = list.viewport.gameObject.AddComponent<RectMask2D>();
            rectMask.enabled = true;
        }
        Browser.SetActive(true);
        Browser.transform.SetAsLastSibling();
        if (CloseButton != null)
        {
            CloseButton.gameObject.SetActive(true);
            CloseButton.transform.SetAsLastSibling();
        }

        // The picker can be populated in Awake while hidden. Rebuild after
        // it becomes visible so the content isn't left at the old bottom offset.
        StartCoroutine(RestoreVisibleListTop());
    }

    private IEnumerator RestoreVisibleListTop()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (Browser == null || !Browser.activeInHierarchy) yield break;
        ScrollRect scroll = Browser.GetComponentInChildren<ScrollRect>(true);
        if (scroll == null || scroll.content == null) yield break;

        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        Canvas.ForceUpdateCanvases();
        scroll.StopMovement();
        Vector2 position = scroll.content.anchoredPosition;
        position.y = 0f; // Content uses top anchors and top pivot.
        scroll.content.anchoredPosition = position;
        scroll.verticalNormalizedPosition = 1f;
    }

    public void Close()
    {
        if (CloseButton != null)
            CloseButton.gameObject.SetActive(false);
        if (Browser != null)
            Browser.SetActive(false);
    }
}
