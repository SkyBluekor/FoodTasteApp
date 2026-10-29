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
        if (ScoreData.Count > 0)
        {
            return ScoreData;
        }
        else
        {
            return null;
        }
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
                Rank_List[count].Item2.text = item.Item2.ToString() + " ��";
                count++;
            }
        }


    } 

}
