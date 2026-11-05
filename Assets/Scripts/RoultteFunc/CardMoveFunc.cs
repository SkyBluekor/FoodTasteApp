using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardMoveFunc : MonoBehaviour
{
    public Button CardPickBtn, randomPickBtn, RefreshBtn;
    public GameObject cardPanel;
    public float time;
    public GameObject WarningImg;
    public TMP_Dropdown Food_Dropdown;
    public TMP_InputField Food_Search;
    public List<Transform> cardList;

    private const float MoveSpeed = 1000f;
    private const float EdgePadding = 65f;
    private Transform selectedCard;

    private void Start()
    {
        CardPickBtn.onClick.AddListener(PickRandomCard);
    }

    private void PickRandomCard()
    {
        cardList = new List<Transform>();
        foreach (Transform card in cardPanel.transform)
        {
            TextMeshProUGUI label = card.GetComponentInChildren<TextMeshProUGUI>();
            if (label == null || string.IsNullOrWhiteSpace(label.text))
            {
                StartCoroutine(ShowWarning(2f));
                return;
            }
            cardList.Add(card);
        }
        if (cardList.Count == 0)
            return;

        StartCoroutine(ShuffleCards());
    }

    private IEnumerator ShowWarning(float seconds)
    {
        WarningImg.SetActive(true);
        yield return new WaitForSeconds(seconds);
        WarningImg.SetActive(false);
    }

    private void SetControlsEnabled(bool enabled)
    {
        CardPickBtn.interactable = enabled;
        randomPickBtn.interactable = enabled;
        Food_Dropdown.interactable = enabled;
        RefreshBtn.interactable = enabled;
        Food_Search.interactable = enabled;
    }

    private Vector3 RandomPosition(Transform card)
    {
        Rect bounds = ((RectTransform)cardPanel.transform).rect;
        RectTransform rect = (RectTransform)card;
        float halfWidth = rect.rect.width * Mathf.Abs(rect.localScale.x) * 0.5f;
        float halfHeight = rect.rect.height * Mathf.Abs(rect.localScale.y) * 0.5f;
        float minX = bounds.xMin + halfWidth + EdgePadding;
        float maxX = bounds.xMax - halfWidth - EdgePadding;
        float minY = bounds.yMin + halfHeight + EdgePadding;
        float maxY = bounds.yMax - halfHeight - EdgePadding;
        return new Vector3(minX <= maxX ? Random.Range(minX, maxX) : bounds.center.x,
            minY <= maxY ? Random.Range(minY, maxY) : bounds.center.y, 0f);
    }

    private IEnumerator ShuffleCards()
    {
        SetControlsEnabled(false);
        var targets = new Dictionary<Transform, Vector3>();
        foreach (Transform card in cardList)
            targets[card] = RandomPosition(card);

        float nextRemoval = Time.time + 1f;
        while (cardList.Count > 1)
        {
            foreach (Transform card in cardList)
            {
                card.localPosition = Vector3.MoveTowards(card.localPosition, targets[card], MoveSpeed * Time.deltaTime);
                if (Vector3.Distance(card.localPosition, targets[card]) < 0.01f)
                    targets[card] = RandomPosition(card);
            }
            if (Time.time >= nextRemoval)
            {
                int index = Random.Range(0, cardList.Count);
                Transform removed = cardList[index];
                cardList.RemoveAt(index);
                targets.Remove(removed);
                Destroy(removed.gameObject);
                nextRemoval = Time.time + 1f;
            }
            yield return null;
        }

        Transform winner = cardList[0];
        Vector3 center = ((RectTransform)cardPanel.transform).rect.center;
        while (winner != null && Vector3.Distance(winner.localPosition, center) > 0.01f)
        {
            winner.localPosition = Vector3.MoveTowards(winner.localPosition, center, MoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (winner != null)
        {
            winner.localPosition = center;
            if (winner != selectedCard)
            {
                winner.localScale = new Vector3(winner.localScale.x * 2f, winner.localScale.y * 2f, winner.localScale.z);
                selectedCard = winner;
            }
        }
        SetControlsEnabled(true);
    }
}
