using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class CardMoveFunc : MonoBehaviour
{
    public Button CardPickBtn, randomPickBtn,RefreshBtn;
    public GameObject cardPanel;
    public float time;
    public GameObject WarningImg;
    public TMP_Dropdown Food_Dropdown;
    public TMP_InputField Food_Search;

    private Vector3 lastPos;
    private float moveSpeed = 1000f;
    public List<Transform> cardList;

    // Start is called before the first frame update
    private void Start()
    {

        lastPos = new Vector3(0f,0f,0f);
        CardPickBtn.onClick.AddListener(PickRandomCard);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PickRandomCard()
    {
        cardList = new List<Transform>();
        for (int i = 0; i < cardPanel.transform.childCount; i++)
        {
            Transform card = cardPanel.transform.GetChild(i);
            cardList.Add(card);
        }
        Debug.Log("Card List ����" + cardList.Count);
        time = cardList.Count;
        Debug.Log("timex  " + time);

        foreach (Transform card in cardList) 
        { 
            Debug.Log("cardName" + card.gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
            if(card.gameObject.GetComponentInChildren<TextMeshProUGUI>().text == "")
            {
                StartCoroutine(ShowWarning(2f));
                return;
            }
        }
        for (int i = 0;i < cardList.Count;i++)
        {
            float x = Random.Range(-cardPanel.GetComponent<RectTransform>().rect.width / 2, cardPanel.GetComponent<RectTransform>().rect.width / 2);
            float y = Random.Range(-cardPanel.GetComponent<RectTransform>().rect.height / 2, cardPanel.GetComponent<RectTransform>().rect.height / 2);
            Vector3 targetPos = new Vector3(x, y, 0f);
            if (cardList[0].transform.localPosition != lastPos)
                StartCoroutine(MoveToPos(cardList[i],targetPos));
        }
        InvokeRepeating("DestroyCardObj",0f,1f);

    }

    IEnumerator ShowWarning(float seconds)
    {
        WarningImg.SetActive (true);
        yield return new WaitForSeconds(seconds);
        WarningImg.SetActive (false);

    }

    IEnumerator MoveToPos(Transform obj, Vector3 targetPos)
    {
        CardPickBtn.interactable = false;
        randomPickBtn.interactable = false;
        Food_Dropdown.interactable = false;
        RefreshBtn.interactable = false;
        Food_Search.interactable = false;
        float startTime = Time.time;
        while (obj != null && obj.transform.localPosition != targetPos )
        {
            obj.transform.localPosition = Vector3.MoveTowards(obj.transform.localPosition, targetPos, moveSpeed * Time.deltaTime);
            if(obj.transform.localPosition == targetPos) 
            { 
                float endTime = Time.time;
                Debug.Log("Start TIme" + startTime + "EndTIme" + endTime + "s -e " + (endTime - startTime));
                Debug.Log("timey  " + time);
                if (endTime - startTime < time) 
                {
                    float x = Random.Range(-cardPanel.GetComponent<RectTransform>().rect.width / 2, cardPanel.GetComponent<RectTransform>().rect.width / 2);
                    float y = Random.Range(-cardPanel.GetComponent<RectTransform>().rect.height / 2, cardPanel.GetComponent<RectTransform>().rect.height / 2);
                    Vector3 newTargetPos = new Vector3(x, y, 0f);
                    obj.transform.localPosition = Vector3.MoveTowards(obj.transform.localPosition,newTargetPos,moveSpeed * Time.deltaTime);
                    targetPos = newTargetPos;
                }
                else
                {

                    StartCoroutine(MoveEndCard());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator MoveEndCard()
    {
        if (cardList != null)
        {
            while (cardList[0].transform.localPosition != lastPos)
                {
                    if (cardList[0] != null)
                    {
                        cardList[0].transform.localPosition = Vector3.MoveTowards(cardList[0].transform.localPosition, lastPos, moveSpeed * Time.deltaTime);
                        if (cardList[0].transform.localPosition == lastPos)
                        {
                            yield return new WaitForEndOfFrame();
                            if (Vector3.Distance(cardList[0].transform.localPosition, lastPos) <= 0.01f)
                            {
                                cardList[0].transform.localScale = new Vector3(2f, 2f, 1f);
                                yield break;
                            }
                        }
                    }
                    CardPickBtn.interactable = true;
                    randomPickBtn.interactable = true;
                    Food_Dropdown.interactable = true;
                    RefreshBtn.interactable = true;
                    Food_Search.interactable = true;
                    yield return null;
                }
        }
    }

    private void DestroyCardObj()
    {
        if(cardList.Count > 1)
        {
            int randomindex = Random.Range(0,cardList.Count);
            Destroy(cardList[randomindex].gameObject); 
            cardList.RemoveAt(randomindex);
        }
        else if (cardList.Count == 1)
        {
            StopCoroutine("DestroyCardObj");
        }
    }
    

}
