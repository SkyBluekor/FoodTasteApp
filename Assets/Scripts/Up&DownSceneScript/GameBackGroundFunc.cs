using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBackGroundFunc : MonoBehaviour
{
    private static string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private string currentID = "";
    private string currentNickName = "";
    public Button GameStartBtn, RankingBtn, ExitGameBtn, ExitRankingBtn;
    public GameObject GamePlayPanel, GameRankingPanel;
    public TextMeshProUGUI Ranking_1st, Ranking_2nd, Ranking_3rd, Ranking_4th, Ranking_5th, Ranking_6th, Ranking_7th;
    // Start is called before the first frame update
    void Start()
    {
        currentID = PlayerPrefs.GetString("playerid");
        GameStartBtn.onClick.AddListener(GamestartBtnFunc);
        RankingBtn.onClick.AddListener(RankingBtnFunc);
        ExitGameBtn.onClick.AddListener(ExitGameBtnFunc);
        ExitRankingBtn.onClick.AddListener(ExitRankingFunc);
    }

#if UNITY_EDITOR
    public void AlignRankingRows()
    {
        Transform panel = GameRankingPanel.transform;
        Transform oldTitle = panel.Find("Image/OverPanelTitle");
        if (oldTitle != null) oldTitle.gameObject.SetActive(false);
        Transform existingCaption = panel.Find("RankingCaption");
        GameObject captionObject = existingCaption != null ? existingCaption.gameObject : new GameObject("RankingCaption", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform captionRect = (RectTransform)captionObject.transform;
        captionRect.SetParent(panel, false);
        captionRect.anchorMin = captionRect.anchorMax = captionRect.pivot = new Vector2(0.5f, 0.5f);
        captionRect.anchoredPosition = new Vector2(0f, 325f);
        captionRect.sizeDelta = new Vector2(420f, 45f);
        TextMeshProUGUI caption = captionObject.GetComponent<TextMeshProUGUI>();
        caption.font = Ranking_1st.font;
        caption.fontSharedMaterial = Ranking_1st.fontSharedMaterial;
        caption.fontSize = 26f;
        caption.alignment = TextAlignmentOptions.Center;
        caption.color = new Color(0.35f, 0.15f, 0.07f);
        caption.raycastTarget = false;
        caption.text = "최고 기록 기준";
        string[] ranks = { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th" };
        TextMeshProUGUI[] names = { Ranking_1st, Ranking_2nd, Ranking_3rd, Ranking_4th, Ranking_5th, Ranking_6th, Ranking_7th };
        for (int i = 0; i < ranks.Length; i++)
        {
            float y = 225f - 88f * i;
            RectTransform row = panel.Find("RankingRow_" + (i + 1)) as RectTransform;
            RectTransform badge = panel.Find(ranks[i] + "img") as RectTransform;
            if (row != null)
            {
                row.sizeDelta = new Vector2(650f, 160f);
                row.anchoredPosition = new Vector2(row.anchoredPosition.x, y);
            }
            if (badge != null) badge.anchoredPosition = new Vector2(badge.anchoredPosition.x, y);
            if (names[i] != null)
            {
                RectTransform label = names[i].rectTransform;
                label.anchoredPosition = new Vector2(label.anchoredPosition.x, y);
            }
        }
    }
#endif

    private void GamestartBtnFunc()
    {
        GamePlayPanel.SetActive(true);
    }
    private void RankingBtnFunc()
    {
        Debug.Log("Load Ranking window");
        GameRankingPanel.SetActive(true);
        RankingFunction();
    }
    private void ExitGameBtnFunc()
    {
        Debug.Log("Load MainScene");
        SceneManager.LoadScene("MainFuncScene");
    }
    
    private void ExitRankingFunc()
    {
        GameRankingPanel.SetActive(false);
    }

    private List<(string, int)> GetRankData()
    {
        List<(string, int)> ScoreData = new List<(string, int)>();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            MySqlCommand cmd = new MySqlCommand("SELECT User.Nickname, game_scores.score FROM game_scores INNER JOIN User ON game_scores.player_id = User.id ORDER BY game_scores.score DESC LIMIT 7", conn);
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
    
    private void RankingFunction()
    {
        GameRankingPanel.SetActive(true);

        List<(string, int)> GetScoreData = GetRankData();
        List<(TextMeshProUGUI, TextMeshProUGUI)> Rank_List = new List<(TextMeshProUGUI, TextMeshProUGUI)>();
        Rank_List.Clear();
        for (int i = 0; i < 7; i++)
        {
            TextMeshProUGUI[] textMeshPros;
            if (i == 0)
            {
                textMeshPros = Ranking_1st.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
            }
            else if (i == 1)
            {
                textMeshPros = Ranking_2nd.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
            }
            else if (i == 2)
            {
                textMeshPros = Ranking_3rd.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
            }
            else if (i == 3)
            {
                textMeshPros = Ranking_4th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
            }
            else if (i == 4)
            {
                textMeshPros = Ranking_5th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
            }
            else if (i == 5)
            {
                textMeshPros = Ranking_6th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
            }
            else if (i == 6)
            {
                textMeshPros = Ranking_7th.GetComponentsInChildren<TextMeshProUGUI>();
                Rank_List.Add((textMeshPros[0], textMeshPros[1]));
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

}
