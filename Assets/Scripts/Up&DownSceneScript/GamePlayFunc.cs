using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using Color = UnityEngine.Color;

public class GamePlayFunc : MonoBehaviour
{
    private static string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private string currentID = "";
    private string currentNickName = "";
    private List<(string, double, string)> FoodAvgScoreData; //foodnum, foodscore, foodname
    private float time = 60.0f;
    public GameObject GameOverPanel, GamePlayPanel;
    public TextMeshProUGUI TimeTextMeshPro, ScoreTextMeshPro, LeftFoodScoreText, RightFoodscoreText, LeftFoodScore, RightFoodScore;
    public TextMeshProUGUI Text_1st, Text_2nd, Text_3rd, Text_4th, Text_5th, Text_6th, Text_7th;
    public Button LeftFood, RightFood;
    private Button ClickButton;
    public Image LeftFoodImg, RightFoodImg, UpDownImg;
    private List<string> UpDownOption = new List<string> { "UP", "DOWN" };
    private int Score = 0;
    private string UpDownState;

    public Button ReplayBtn, GameExitBtn;
    private List<TextMeshProUGUI> textMeshProUGUIs;
    private float currentvalue = 0;
    private float targetvalue = 5.0f;
    private float Duration = 1.0f;
    private int currentItem1Index = 0, currentItem2Index = 0;
    private ItemIndex stage;
    private Coroutine timeCoroutine;
    private bool gameEnding;
    private TextMeshProUGUI resultText;
    private void Awake()
    {
        Transform result = GameOverPanel.transform.Find("ResultBackground/ResultScore");
        resultText = result != null ? result.GetComponent<TextMeshProUGUI>() : null;
        textMeshProUGUIs = new List<TextMeshProUGUI>();
        stage = new ItemIndex();
        textMeshProUGUIs.Add(LeftFoodScoreText);
        textMeshProUGUIs.Add(LeftFoodScore);
        textMeshProUGUIs.Add(RightFoodscoreText);
        textMeshProUGUIs.Add(RightFoodScore);
    }

#if UNITY_EDITOR
    public void ApplySceneLayout()
    {
        ConfigureGameOverRanking();
        ConfigureFoodCardText(LeftFoodImg, "LeftFoodName", LeftFoodScoreText, LeftFoodScore);
        ConfigureFoodCardText(RightFoodImg, "RightFoodName", RightFoodscoreText, RightFoodScore);
    }

    private static void ConfigureFoodCardText(Image food, string nameObject, TMP_Text caption, TMP_Text score)
    {
        Transform nameTransform = food.transform.Find(nameObject);
        if (nameTransform != null)
        {
            TMP_Text name = nameTransform.GetComponent<TMP_Text>();
            if (name != null)
            {
                name.rectTransform.anchoredPosition = new Vector2(0f, -295f);
                name.rectTransform.sizeDelta = new Vector2(350f, 45f);
                name.enableAutoSizing = true;
                name.fontSizeMin = 18f;
                name.fontSizeMax = 34f;
                name.alignment = TextAlignmentOptions.Center;
                name.enableWordWrapping = false;
            }
        }
        caption.text = "";
        score.rectTransform.anchoredPosition = new Vector2(0f, -345f);
        score.rectTransform.sizeDelta = new Vector2(320f, 42f);
        score.enableAutoSizing = true;
        score.fontSizeMin = 18f;
        score.fontSizeMax = 30f;
        score.alignment = TextAlignmentOptions.Center;
        score.enableWordWrapping = false;
    }

    private void ConfigureGameOverRanking()
    {
        RectTransform panel = GameOverPanel.transform as RectTransform;
        if (panel == null || panel.Find("RankingScroll") != null) return;
        Transform oldTitle = panel.Find("Image/OverPanelTitle");
        if (oldTitle != null) oldTitle.gameObject.SetActive(false);

        GameObject resultBackground = new GameObject("ResultBackground", typeof(RectTransform), typeof(Image));
        RectTransform backgroundRect = (RectTransform)resultBackground.transform;
        backgroundRect.SetParent(panel, false);
        backgroundRect.anchorMin = backgroundRect.anchorMax = backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(0f, 150f);
        backgroundRect.sizeDelta = new Vector2(690f, 78f);
        Image resultImage = resultBackground.GetComponent<Image>();
        resultImage.color = new Color(1f, 0.95f, 0.85f, 0.96f);
        resultImage.raycastTarget = false;
        GameObject resultObject = new GameObject("ResultScore", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform resultRect = (RectTransform)resultObject.transform;
        resultRect.SetParent(backgroundRect, false);
        resultRect.anchorMin = Vector2.zero;
        resultRect.anchorMax = Vector2.one;
        resultRect.offsetMin = new Vector2(10f, 4f);
        resultRect.offsetMax = new Vector2(-10f, -4f);
        resultText = resultObject.GetComponent<TextMeshProUGUI>();
        resultText.font = Text_1st.font;
        resultText.fontSharedMaterial = Text_1st.fontSharedMaterial;
        resultText.fontSize = 28f;
        resultText.enableAutoSizing = true;
        resultText.fontSizeMin = 18f;
        resultText.fontSizeMax = 28f;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.color = new Color(0.35f, 0.15f, 0.07f);
        resultText.raycastTarget = false;
        resultText.text = "이번 점수 0점\n최고 기록 0점 · 기록 유지";

        // The panel artwork has two button-shaped spaces below the ranking area.
        RectTransform replay = ReplayBtn.transform as RectTransform;
        RectTransform exit = GameExitBtn.transform as RectTransform;
        replay.anchoredPosition = new Vector2(-205f, -280f);
        exit.anchoredPosition = new Vector2(205f, -280f);
        replay.sizeDelta = exit.sizeDelta = new Vector2(410f, 150f);

        GameObject scrollObject = new GameObject("RankingScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        RectTransform scrollRect = (RectTransform)scrollObject.transform;
        scrollRect.SetParent(panel, false);
        scrollRect.anchorMin = scrollRect.anchorMax = scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0f, -40f);
        scrollRect.sizeDelta = new Vector2(760f, 280f);
        Image hitArea = scrollObject.GetComponent<Image>();
        hitArea.color = Color.clear;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        RectTransform viewport = (RectTransform)viewportObject.transform;
        viewport.SetParent(scrollRect, false);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(0f, 0f);
        viewport.offsetMax = new Vector2(-24f, 0f);

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        RectTransform content = (RectTransform)contentObject.transform;
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 616f);

        string[] ranks = { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th" };
        TextMeshProUGUI[] names = { Text_1st, Text_2nd, Text_3rd, Text_4th, Text_5th, Text_6th, Text_7th };
        for (int i = 0; i < ranks.Length; i++)
        {
            float y = -44f - 88f * i;
            Transform row = panel.Find("RankingRow_" + (i + 1));
            Transform badge = panel.Find(ranks[i] + "img");
            PlaceRankingElement(row, content, new Vector2(0f, y));
            if (row != null) ((RectTransform)row).sizeDelta = new Vector2(650f, 160f);
            PlaceRankingElement(badge, content, new Vector2(-270f, y));
            PlaceRankingElement(names[i].transform, content, new Vector2(-45f, y));
        }

        GameObject barObject = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        RectTransform barRect = (RectTransform)barObject.transform;
        barRect.SetParent(scrollRect, false);
        barRect.anchorMin = new Vector2(1f, 0f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(1f, 0.5f);
        barRect.offsetMin = new Vector2(-13f, 8f);
        barRect.offsetMax = new Vector2(-5f, -8f);
        barObject.GetComponent<Image>().color = new Color(0.75f, 0.43f, 0.19f, 0.25f);
        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        RectTransform handle = (RectTransform)handleObject.transform;
        handle.SetParent(barRect, false);
        handle.anchorMin = Vector2.zero;
        handle.anchorMax = Vector2.one;
        handle.sizeDelta = Vector2.zero;
        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(0.77f, 0.34f, 0.08f, 0.8f);
        Scrollbar bar = barObject.GetComponent<Scrollbar>();
        bar.handleRect = handle;
        bar.targetGraphic = handleImage;
        bar.direction = Scrollbar.Direction.BottomToTop;

        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 35f;
        scroll.verticalScrollbar = bar;
        scroll.verticalNormalizedPosition = 1f;
    }

    private static void PlaceRankingElement(Transform element, RectTransform content, Vector2 position)
    {
        if (element == null) return;
        RectTransform rect = (RectTransform)element;
        rect.SetParent(content, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
    }
#endif
    private void OnEnable()
    {
        currentID = PlayerPrefs.GetString("playerid");
        Score = 0;
        gameEnding = false;
        ScoreTextMeshPro.text = "0";
        time = 60f;
        TimeTextMeshPro.text = time.ToString();
        ReplayBtn.onClick.RemoveListener(ReplayBtnFunc);
        GameExitBtn.onClick.RemoveListener(ExitGameBtnFunc);
        ReplayBtn.onClick.AddListener(ReplayBtnFunc);
        GameExitBtn.onClick.AddListener(ExitGameBtnFunc);
        StopAllCoroutines();
        StartCoroutine(GetFoodDataAndStartGame());
    }
    private IEnumerator GetFoodDataAndStartGame()
    {
        yield return GetFoodData();
        StartGame();
    }
    public struct ItemIndex
    {
        public int item1index;
        public int item2index;

        public void SetIndex(int x, int y)
        {
            item1index = x;
            item2index = y;
        }
    }

    private void StartGame()
    {
        if (FoodAvgScoreData == null || FoodAvgScoreData.Count < 2)
        {
            LeftFood.interactable = false;
            RightFood.interactable = false;
            GameOverPanel.SetActive(true);
            Text_1st.text = "음식 점수 데이터가 부족합니다";
            Debug.LogWarning("UP & DOWN: 음식 점수 데이터가 부족하거나 데이터베이스에 연결할 수 없습니다.");
            return;
        }
        UpDownState = GetRandomOption();
        Debug.Log("UpdownState: " + UpDownState);
        newStage();

    }

    private void newStage()
    {
        if (FoodAvgScoreData == null || FoodAvgScoreData.Count < 2)
        {
            StartGame();
            return;
        }
        Debug.Log("닉네임: " + currentNickName);
        foreach(var t in textMeshProUGUIs)
        {
            t.gameObject.SetActive(false);
        }
        time = 60f;
        timeCoroutine = StartCoroutine(DecreasecTime());
        UpDownState = GetRandomOption();
        RightFood.onClick.RemoveAllListeners();
        LeftFood.onClick.RemoveAllListeners();
        LeftFood.interactable = true;
        RightFood.interactable= true;
        System.Random random = new System.Random();
        string item1name, item2name;
        int item1index, item2index;
        double item1Score, item2Score;

        int count = FoodAvgScoreData.Count;

        while (count > 0)
        {
            count--;
            int k = random.Next(count + 1);
            var value = FoodAvgScoreData[k];
            FoodAvgScoreData[k] = FoodAvgScoreData[count];
            FoodAvgScoreData[count] = value;
        }

        item1index = random.Next(0, FoodAvgScoreData.Count);
        item1name = FoodAvgScoreData[item1index].Item3;
        item1Score = FoodAvgScoreData[item1index].Item2;

        item2index = 0;
        item2name = "";
        item2Score = 0f;

        

        int item2count = 0;
        foreach(var tuple in FoodAvgScoreData)
        {
            if(tuple.Item3 != item1name)
            {
                Debug.Log("tuple아이템 " + tuple.Item2 + "아이템1 스코어 " + item1Score);
                if(tuple.Item2 != item1Score)
                {
                    item2index = item2count;
                    item2name = tuple.Item3;
                    item2Score = tuple.Item2;
                }
                else
                {
                    Debug.Log("Game End");
                }
            }
            item2count++;
        }

        if(Math.Abs(item1Score - item2Score) < 0.0001 || FoodAvgScoreData.Count < 2)
        {
            Debug.Log("Game Over Because Low Data");
            StartCoroutine(GameOverFunction());
            return;
        }

        stage = new ItemIndex();
        stage.SetIndex(item1index, item2index);

        int leftFoodNumber;
        int rightFoodNumber;
        int.TryParse(FoodAvgScoreData[item1index].Item1, out leftFoodNumber);
        int.TryParse(FoodAvgScoreData[item2index].Item1, out rightFoodNumber);
        FoodImgGet(leftFoodNumber, "Left", FoodAvgScoreData[item1index].Item3);
        FoodImgGet(rightFoodNumber, "Right", FoodAvgScoreData[item2index].Item3);

        Debug.Log("item1: 이름 " + FoodAvgScoreData[item1index].Item3 + "item1 점수: " + FoodAvgScoreData[item1index].Item2);
        Debug.Log("item1: 이름 " + FoodAvgScoreData[item2index].Item3 + "item1 점수: " + FoodAvgScoreData[item2index].Item2);

        Debug.Log("아이템1 이름: "+ item1name + " 아이템1 스코어: " + item1Score);
        Debug.Log("아이템2 이름: " + item2name + " 아이템2 스코어: " + item2Score);
        RightFood.onClick.AddListener(() => ButtonClick(RightFood));
        LeftFood.onClick.AddListener(() => ButtonClick(LeftFood));

    }

    private void ReplayBtnFunc()
    {
        GameOverPanel.SetActive(false);
        StopAllCoroutines();
        Score = 0;
        gameEnding = false;
        ScoreTextMeshPro.text = "0";
        time = 60f;
        TimeTextMeshPro.text = time.ToString();
        newStage();
    }

    private void ExitGameBtnFunc()
    {
        GamePlayPanel.SetActive(false);
        GameOverPanel.SetActive(false);
    }

    private void GameStartBtnFunc()
    {
        GamePlayPanel.SetActive(true);
    }

    private void RankingBtnFunc()
    {
        GameOverPanel.SetActive(true);
    }

    private void FoodImgGet(int foodNumber, string Direction, string foodname)
    {
        Image target = Direction == "Left" ? LeftFoodImg : RightFoodImg;
        FoodImageCatalog.Apply(target, foodNumber);
        TextMeshProUGUI nameLabel = target.GetComponentInChildren<TextMeshProUGUI>();
        if (nameLabel != null)
        {
            nameLabel.color = new Color(0.35f, 0.15f, 0.07f);
            nameLabel.text = foodname;
        }
    }

    private Color GetFoodTextColor(Texture2D image, Vector2 Textvector)
    {
        Color ReturnColor = Color.black;
        Color imgColor = image.GetPixel(
            Mathf.Clamp(Mathf.RoundToInt(image.width / 2f + Textvector.x / 2f), 0, image.width - 1),
            Mathf.Clamp(Mathf.RoundToInt(image.height / 2f + Textvector.y / 2f), 0, image.height - 1));

        float brightness = imgColor.r * 0.299f + imgColor.g * 0.587f + imgColor.b * 0.114f;
        float threshold = 0.5f;
        if (brightness > threshold)
        {
            ReturnColor = Color.black;
        }
        else
        {
            ReturnColor = Color.white;
        }
        return ReturnColor;
    }

    public IEnumerator GetFoodData()
    {
        FoodAvgScoreData = new List<(string,double,string)> ();
        MySqlConnection conn = new MySqlConnection(connStr);

        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select foodnum, round(avg(foodscore),3) as average_score, foodname from foodtastescore group by foodnum,foodname order by average_score desc",conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                FoodAvgScoreData.Add((reader.GetString("foodnum"), reader.GetDouble("average_score"), reader.GetString("foodname")));
            }
            reader.Close();


            cmd = new MySqlCommand("select nickname from user where id = @currentid",conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                currentNickName = reader.GetString("nickname");
            }

            reader.Close();

        }
        catch(MySqlException e)  
        { 
            Debug.Log(e);
        }
        finally
        { conn.Close(); }
        yield return null;

    }

    private IEnumerator DecreasecTime()
    {
        while(time > 0f)
        {
            yield return new WaitForSeconds(1f);
            time -= 1f;
            Debug.Log(time);
            TimeTextMeshPro.text = time.ToString();
        }
        if(time == 0f)
        {
            StopCoroutine(DecreasecTime());
            LeftFood.interactable = false;
            RightFood.interactable = false;
            StartCoroutine(GameOverFunction());
        }
    }

    private void ButtonClick(Button button)
    {
        foreach(var t in textMeshProUGUIs)
        {
            t.gameObject.SetActive(true);
        }
        LeftFood.interactable = false;
        RightFood.interactable = false;
        ClickButton = button;
        StartCoroutine(AnimateNumber());
        currentItem1Index = stage.item1index;
        currentItem2Index = stage.item2index;
        double LeftItemScore, RightItemScore;
        LeftItemScore = FoodAvgScoreData[currentItem1Index].Item2;
        RightItemScore = FoodAvgScoreData[currentItem2Index].Item2;

        Debug.Log("클릭버튼: " + ClickButton.name);
        string ClickButtonName = ClickButton.name;
        if(ClickButtonName == "LeftFood")
        {
            if(UpDownState == "UP")
            {
                if (LeftItemScore > RightItemScore)
                {
                    Score += TimetoScore();
                    StartCoroutine(SetScoreTxt());
                }
                else
                {
                    StartCoroutine(GameOverFunction());
                }
            }
            else if(UpDownState == "DOWN")
            {
                if (LeftItemScore < RightItemScore)
                {
                    Score += TimetoScore();
                    StartCoroutine(SetScoreTxt());
                }
                else
                {
                    StartCoroutine(GameOverFunction());
                }
            }
        }
        else if(ClickButtonName == "RightFood")
        {
            if (UpDownState == "UP")
            {
                if (LeftItemScore < RightItemScore)
                {
                    Score += TimetoScore();
                    StartCoroutine(SetScoreTxt());
                }
                else
                {
                    StartCoroutine(GameOverFunction());
                }
            }
            else if (UpDownState == "DOWN")
            {
                if (LeftItemScore > RightItemScore)
                {
                    Score += TimetoScore();
                    StartCoroutine(SetScoreTxt());
                }
                else
                {
                    StartCoroutine(GameOverFunction());
                }
            }
        }
        time = 60f;

    }

    private int TimetoScore()
    {
        int plusScore = 0;
        if(time > 55)
        {
            plusScore = 100;
        }
        else if (time <= 55 && time > 50)
        {
            plusScore = 70;
        }
        else if(time <= 50 && time > 40)
        {
            plusScore = 50;
        }
        else if(time <=40 && time > 30)
        {
            plusScore = 30;
        }
        else if(time <= 30 && time > 20)
        {
            plusScore = 20;
        }
        else
        {
            plusScore = 10;
        }
        return plusScore;

    }

    private string GetRandomOption()
    {
        System.Random random = new System.Random();
        int index = random.Next(0, UpDownOption.Count);
        string option = UpDownOption[index];
        UpDownImg.sprite = Resources.Load<Sprite>("FoodTasteUpDownUI/" + (option == "UP" ? "UpImg" : "DownImg"));
        UpDownImg.preserveAspect = true;
        return option;
    }

    private IEnumerator AnimateNumber()
    {
        StopCoroutine(timeCoroutine);

        Debug.Log("LeftFoodScoreTxt Name" + LeftFoodScore.name);
        float timer = 0.0f;
        while(timer < Duration)
        {
            currentvalue = Mathf.Lerp(0.00f, targetvalue, timer / Duration);
            yield return new WaitForSeconds(0.01f);
            timer += +0.02f;
            if(!LeftFoodScoreText.gameObject.activeSelf)
            {
                foreach (var t in textMeshProUGUIs)
                {
                    t.gameObject.SetActive(true);
                }
            }
            string DotTwoValue = currentvalue.ToString("F2");
            LeftFoodScore.text = "평균 " + DotTwoValue + "점";
            RightFoodScore.text = "평균 " + DotTwoValue + "점";
        }
        LeftFoodScore.text = "평균 " + FoodAvgScoreData[currentItem1Index].Item2.ToString("F2") + "점";
        RightFoodScore.text = "평균 " + FoodAvgScoreData[currentItem2Index].Item2.ToString("F2") + "점";
        
    }

    private IEnumerator SetScoreTxt()
    {
        yield return new WaitForSeconds(0.6f);
        Debug.Log("SetScoretxt : " + Score);
        ScoreTextMeshPro.text = Score.ToString();
        yield return new WaitForSeconds(0.8f);
        Debug.Log("FoodData Count" + FoodAvgScoreData.Count);
        CorrectClick();
        Debug.Log("FoodData Count" + FoodAvgScoreData.Count);
        newStage();
        LeftFood.interactable = true;
        RightFood.interactable = true;
    }

    private void CorrectClick()
    {
        TextMeshProUGUI LeftFoodNameText = LeftFoodImg.GetComponentInChildren<TextMeshProUGUI>();
        string LeftFoodName = LeftFoodNameText.text;
        TextMeshProUGUI RightFoodnameText = RightFoodImg.GetComponentInChildren<TextMeshProUGUI>();
        string RightFoodName = RightFoodnameText.text;
        
        FoodAvgScoreData.RemoveAll(item => item.Item3 == LeftFoodName);
        FoodAvgScoreData.RemoveAll(item => item.Item3 == RightFoodName);
    }

   

    private IEnumerator GameOverFunction()
    {
        if (gameEnding) yield break;
        gameEnding = true;
        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        LeftFood.interactable = false;
        RightFood.interactable = false;
        yield return new WaitForSeconds(1.0f);
        GameOverPanel.SetActive(true);
        ScrollRect rankingScroll = GameOverPanel.GetComponentInChildren<ScrollRect>();
        if (rankingScroll != null) rankingScroll.verticalNormalizedPosition = 1f;
        int bestScore, rank, previousScore;
        bool hadPreviousScore;
        bool saved = SetScoreData(Score, out bestScore, out rank, out hadPreviousScore, out previousScore);
        ScoreTextMeshPro.text = Score.ToString();
        if (resultText != null)
        {
            string message = "이번 점수 " + Score + "점";
            if (saved)
            {
                string status = !hadPreviousScore ? "첫 기록 등록!" : Score > previousScore && Score == bestScore ? "최고 기록 갱신!" : "기록 유지";
                message += "\n최고 기록 " + bestScore + "점 · " + status;
            }
            resultText.text = message;
        }

        List<(string, int)> GetScoreData = GetRankData();
        List<(TextMeshProUGUI, TextMeshProUGUI)> Rank_List = new List<(TextMeshProUGUI, TextMeshProUGUI)>();
        Rank_List.Clear();
        for (int i = 0; i < 7; i++)
        {
            TextMeshProUGUI[] textMeshProUGUIs;
            if (i == 0)
            {
                textMeshProUGUIs = Text_1st.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
            else if (i == 1)
            {
                textMeshProUGUIs = Text_2nd.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
            else if (i == 2)
            {
                textMeshProUGUIs = Text_3rd.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
            else if (i == 3)
            {
                textMeshProUGUIs = Text_4th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
            else if (i == 4)
            {
                textMeshProUGUIs = Text_5th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
            else if (i == 5)
            {
                textMeshProUGUIs = Text_6th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
            else if (i == 6)
            {
                textMeshProUGUIs = Text_7th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshProUGUIs[0], textMeshProUGUIs[1]));
            }
        }

        
        if (GetScoreData.Count > 0)
        {
            int count = 0;
            foreach (var item in GetScoreData)
            {
                Rank_List[count].Item1.text = item.Item1;
                Rank_List[count].Item2.text = item.Item2.ToString() + " 점";
                count++;
            }
        }

    }

    private List<(string,int)> GetRankData()
    {
        List<(string, int)> ScoreData = new List<(string, int)>();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            MySqlCommand cmd = new MySqlCommand("SELECT User.Nickname, game_scores.score FROM game_scores INNER JOIN User ON game_scores.player_id = User.id ORDER BY game_scores.score DESC LIMIT 7;", conn);
            conn.Open();
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            { 
                ScoreData.Add((reader.GetString(0), reader.GetInt32(1)));
            }
        }
        catch (Exception ex) 
        {
            Debug.Log(ex);
        }
        finally
        {
            conn.Close();
        }
        return ScoreData;
    }

    private bool SetScoreData(int finalScore, out int bestScore, out int rank, out bool hadPreviousScore, out int previousScore)
    {
        bestScore = finalScore;
        rank = 0;
        hadPreviousScore = false;
        previousScore = 0;
        if (string.IsNullOrWhiteSpace(currentID) || string.IsNullOrWhiteSpace(currentNickName))
        {
            Debug.LogWarning("UP & DOWN: 로그인한 사용자를 확인할 수 없어 점수를 저장하지 못했습니다.");
            return false;
        }
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            using (MySqlCommand previousCommand = new MySqlCommand("SELECT score FROM game_scores WHERE player_id = @player_id", conn))
            {
                previousCommand.Parameters.AddWithValue("@player_id", currentID);
                object value = previousCommand.ExecuteScalar();
                hadPreviousScore = value != null && value != DBNull.Value;
                if (hadPreviousScore) previousScore = Convert.ToInt32(value);
            }
            MySqlCommand cmd = new MySqlCommand("insert into Game_scores (player_id , score) values (@player_id, @score) on duplicate key update score = if(score < values(score) , values(score), score)",conn);
            cmd.Parameters.AddWithValue("@player_id", currentID);
            cmd.Parameters.AddWithValue("@score", finalScore);
            cmd.ExecuteNonQuery();
            using (MySqlCommand bestCommand = new MySqlCommand("SELECT score FROM game_scores WHERE player_id = @player_id", conn))
            {
                bestCommand.Parameters.AddWithValue("@player_id", currentID);
                bestScore = Convert.ToInt32(bestCommand.ExecuteScalar());
            }
            using (MySqlCommand rankCommand = new MySqlCommand("SELECT COUNT(*) FROM game_scores JOIN user ON game_scores.player_id = user.id WHERE game_scores.score > @score", conn))
            {
                rankCommand.Parameters.AddWithValue("@score", bestScore);
                rank = Convert.ToInt32(rankCommand.ExecuteScalar()) + 1;
            }
            return true;
        }
        catch(Exception ex) 
        {
            Debug.LogWarning("UP & DOWN: 점수를 저장하지 못했습니다: " + ex.Message);
            return false;
        }
        finally { conn.Close(); }

    }
}
