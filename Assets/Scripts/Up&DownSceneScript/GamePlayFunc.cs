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
    private void Awake()
    {
        textMeshProUGUIs = new List<TextMeshProUGUI>();
        stage = new ItemIndex();
        textMeshProUGUIs.Add(LeftFoodScoreText);
        textMeshProUGUIs.Add(LeftFoodScore);
        textMeshProUGUIs.Add(RightFoodscoreText);
        textMeshProUGUIs.Add(RightFoodScore);
    }
    private void OnEnable()
    {
        time = 60f;
        TimeTextMeshPro.text = time.ToString();
        ReplayBtn.onClick.AddListener(ReplayBtnFunc);
        GameExitBtn.onClick.AddListener(ExitGameBtnFunc);
        StopAllCoroutines();
        StartCoroutine(GetFoodDataAndStartGame());
    }
    void Start()
    {
        currentID = PlayerPrefs.GetString("playerid");
        ReplayBtn.onClick.AddListener(ReplayBtnFunc);
        GameExitBtn.onClick.AddListener(ExitGameBtnFunc);
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
        UpDownState = GetRandomOption();
        Debug.Log("UpdownState: " + UpDownState);
        newStage();

    }

    private void newStage()
    {
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
        }

        stage = new ItemIndex();
        stage.SetIndex(item1index, item2index);

        string LeftFoodImgUrl = Application.dataPath + "/Images/FoodImg/" + FoodAvgScoreData[item1index].Item1 + ".jpg";
        string RightFoodImgUrl = Application.dataPath + "/Images/FoodImg/" + FoodAvgScoreData[item2index].Item1 + ".jpg";
        FoodImgGet(LeftFoodImgUrl, "Left", FoodAvgScoreData[item1index].Item3);
        FoodImgGet(RightFoodImgUrl, "Right", FoodAvgScoreData[item2index].Item3);

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

    private void FoodImgGet(string FoodImgUrl, string Direction, string foodname)
    {

        if (File.Exists(FoodImgUrl))
        {
            
        }
        else
        {
           FoodImgUrl = FoodImgUrl.Replace(".jpg",".png");
        }
        Debug.Log("FoodImgurl " + FoodImgUrl);
        byte[] imagedata = System.IO.File.ReadAllBytes(FoodImgUrl);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imagedata);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        if(Direction.Equals("Left"))
        {
            LeftFoodImg.sprite = sprite;
            TextMeshProUGUI LeftFoodname = LeftFoodImg.GetComponentInChildren<TextMeshProUGUI>();
            Vector2 Textvector = LeftFoodname.gameObject.transform.localPosition;
            LeftFoodname.color = GetFoodTextColor(tex, Textvector);
            LeftFoodname.text = foodname;
        }
        else if(Direction.Equals("Right"))
        {
            RightFoodImg.sprite = sprite;
            TextMeshProUGUI RightFoodname = RightFoodImg.GetComponentInChildren<TextMeshProUGUI>();
            Vector2 Textvector = RightFoodname.gameObject.transform.localPosition;
            RightFoodname.color = GetFoodTextColor(tex, Textvector);
            RightFoodname.text = foodname;
        }

    }

    private Color GetFoodTextColor(Texture2D image, Vector2 Textvector)
    {
        Color ReturnColor = Color.black;
        Color imgColor = image.GetPixel((int)Textvector.x / 2, (int)Textvector.y / 2);

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
        string UPdownImgUrl;
        byte[] imagedata;
        if (option.Equals("UP"))
        {
            UPdownImgUrl = Application.dataPath + "/Images/icon/" + "UpImg.png";
            imagedata = System.IO.File.ReadAllBytes(UPdownImgUrl);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imagedata);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            UpDownImg.sprite = sprite;
        }
        else if(option.Equals("DOWN"))
        {
            UPdownImgUrl = Application.dataPath + "/Images/icon/" + "DownImg.png";
            imagedata = System.IO.File.ReadAllBytes(UPdownImgUrl);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imagedata);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            UpDownImg.sprite = sprite;
        }
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
            LeftFoodScore.text = DotTwoValue;
            RightFoodScore.text = DotTwoValue;
        }
        LeftFoodScore.text = FoodAvgScoreData[currentItem1Index].Item2.ToString("F2") +" 점";
        RightFoodScore.text = FoodAvgScoreData[currentItem2Index].Item2.ToString("F2")+ " 점";
        
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
        yield return new WaitForSeconds(1.0f);
        GameOverPanel.SetActive(true);
        SetScoreData();
        Score = 0;
        ScoreTextMeshPro.text = Score.ToString();

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
        if(ScoreData.Count > 0) 
        { 
            return ScoreData;
        }
        else
        {
            return null;
        }
    }

    private void SetScoreData()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("insert into Game_scores (player_id , score) values (@player_id, @score) on duplicate key update score = if(score < values(score) , values(score), score)",conn);
            cmd.Parameters.AddWithValue("@player_id", currentID);
            cmd.Parameters.AddWithValue("@score", Score);
            cmd.ExecuteNonQuery();
        }
        catch(Exception ex) 
        {
            Debug.Log(ex);
        }
        finally { conn.Close(); }

    }
}
