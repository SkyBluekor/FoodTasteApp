using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Forwards clicks from the full card to its existing food button.
public class FoodCardHitArea : MonoBehaviour, IPointerClickHandler
{
    public Button foodButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            foodButton != null && foodButton.isActiveAndEnabled && foodButton.IsInteractable())
        {
            foodButton.onClick.Invoke();
        }
    }
}
