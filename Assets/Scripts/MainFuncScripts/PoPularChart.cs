using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using TMPro;

public class PoPularChart : MonoBehaviour
{
    public Button PoPularSearchBtn;
    public Button PoPularMenuBtn;
    public Button SetTodayBtn, SetWeekBtn, SetTotalBtn;
    public GameObject PanelCheck;
    private static string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private List<(string,int)> TotalSearchCount;
    private List<(string,int)> TotalEatenCount;
    private List<(string ,int)> TodaySearchCount;
    private List<(string,int)> TodayEatenCount;
    private List<(string, int)> WeekSearchCount;
    private List<(string, int)> WeekEatenCount;
    private readonly Dictionary<string, int> foodNumberByName = new Dictionary<string, int>();
    private string CurrentTag = "search";
    private string currentSetDay = "total";

    void Start()
    {
        FoodImageCatalog.FitMainPanelTitles();
        if(PanelCheck.activeInHierarchy) 
        {
            GetPoPularData();
        }
       
    }

    private void RefreshData()
    {
        TodaySearchCount = new List<(string, int)>();
        TodayEatenCount = new List<(string, int)>();
        WeekSearchCount = new List<(string, int)>();
        WeekEatenCount = new List<(string, int)>();
        TotalSearchCount = new List<(string, int)>();
        TotalEatenCount = new List<(string, int)>();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select foodname, count(*) as count from search_history where tag = 'search' GROUP BY foodname HAVING COUNT(*) >= 1 order by count desc ", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TotalSearchCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();
            cmd = new MySqlCommand("select foodname, count(*) as count from search_history where tag = 'eaten' GROUP BY foodname HAVING COUNT(*) >= 1 order by count desc", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TotalEatenCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();

            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'search' AND DATE(search_date) = CURDATE() GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TodaySearchCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();

            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'eaten' AND DATE(search_date) = CURDATE() GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TodayEatenCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();

            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'search' AND search_date >= CURDATE() - INTERVAL 7 DAY GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                WeekSearchCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();

            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'eaten' AND search_date >= CURDATE() - INTERVAL 7 DAY GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                WeekEatenCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();
        }
        catch(MySqlException e) 
        { 
            Debug.Log(e.ToString());
        }
    }

    private void GetPoPularData()
    {
        TodaySearchCount = new List<(string ,int)> ();
        TodayEatenCount = new List<(string ,int)> ();
        WeekSearchCount = new List<(string ,int)> ();
        WeekEatenCount  =new List<(string ,int)> ();
        TotalSearchCount = new List<(string,int)> ();
        TotalEatenCount = new List<(string,int)> ();
        MySqlConnection conn = new MySqlConnection(connStr);
        List<List<TextMeshProUGUI>> GameObjList = new List<List<TextMeshProUGUI>> ();
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select foodname, count(*) as count from search_history where tag = 'search' GROUP BY foodname HAVING COUNT(*) >= 1 order by count desc ", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                TotalSearchCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();
            cmd = new MySqlCommand("select foodname, count(*) as count from search_history where tag = 'eaten' GROUP BY foodname HAVING COUNT(*) >= 1 order by count desc",conn);
            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                TotalEatenCount.Add((reader.GetString(0),reader.GetInt32(1)));
            }
            reader.Close ();

            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'search' AND DATE(search_date) = CURDATE() GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC",conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TodaySearchCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close ();
            
            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'eaten' AND DATE(search_date) = CURDATE() GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC",conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TodayEatenCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();
            
            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'search' AND search_date >= CURDATE() - INTERVAL 7 DAY GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC", conn);
            reader= cmd.ExecuteReader();
            while (reader.Read())
            {
                WeekSearchCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();

            cmd = new MySqlCommand("SELECT foodname, COUNT(*) AS count FROM search_history WHERE tag = 'eaten' AND search_date >= CURDATE() - INTERVAL 7 DAY GROUP BY foodname HAVING COUNT(*) >= 1 ORDER BY count DESC", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                WeekEatenCount.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            reader.Close();

            // Resolve the current food name to its original FoodNum for photo lookup.
            foodNumberByName.Clear();
            cmd = new MySqlCommand("SELECT foodnum,foodname FROM fooddata", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(1);
                if (!foodNumberByName.ContainsKey(name))
                    foodNumberByName.Add(name, reader.GetInt32(0));
            }
            reader.Close();

            GameObjList = getButtonObj();
            SetBtntextColor(currentSetDay);
            PoPularSearchBtnAction(GameObjList);

            PoPularSearchBtn.onClick.AddListener(() =>
            {
                RefreshData();
                CurrentTag = "search";
                PoPularSearchBtnAction(GameObjList);
            });

            PoPularMenuBtn.onClick.AddListener(() => 
            {
                RefreshData();
                CurrentTag = "eaten";
                PoPularEatenBtnAction(GameObjList);
            });
            SetTodayBtn.onClick.AddListener(() => 
            {
                currentSetDay = "today";
                SetBtntextColor(currentSetDay);
                if(CurrentTag == "search")
                {
                    PoPularSearchBtnAction(GameObjList);
                }
                else if (CurrentTag == "eaten")
                {
                    PoPularEatenBtnAction(GameObjList);
                }
            });

            SetWeekBtn.onClick.AddListener(() =>
            {
                currentSetDay = "week";
                SetBtntextColor(currentSetDay);
                if (CurrentTag == "search")
                {
                    PoPularSearchBtnAction(GameObjList);
                }
                else if (CurrentTag == "eaten")
                {
                    PoPularEatenBtnAction(GameObjList);
                }
            });

            SetTotalBtn.onClick.AddListener(() => 
            {
                currentSetDay = "total";
                SetBtntextColor(currentSetDay);
                if (CurrentTag == "search")
                {
                    PoPularSearchBtnAction(GameObjList);
                }
                else if (CurrentTag == "eaten")
                {
                    PoPularEatenBtnAction(GameObjList);
                }
            });
        }
        catch(MySqlException e)
        {
            Debug.Log(e.ToString());
        }
        finally { conn.Close(); };
    }
    private List<List<TextMeshProUGUI>> getButtonObj()
    {
        List<List<TextMeshProUGUI>> ButtonObjList = new List<List<TextMeshProUGUI>> ();
       
        for(int i = 0; i < 7; i++) 
        {
            List<TextMeshProUGUI> TextMeshList = new List<TextMeshProUGUI>();
            int x = i + 1;
            if (i == 0)
            {
                GameObject obj = GameObject.Find("Popular" + x + "st");
                TextMeshProUGUI[] textMeshes = obj.GetComponentsInChildren<TextMeshProUGUI>();
                TextMeshList.AddRange(textMeshes);
                ButtonObjList.Add(TextMeshList);
            }
            else if (i == 1)
            {
                GameObject obj = GameObject.Find("Popular" + x + "nd");
                TextMeshProUGUI[] textMeshes = obj.GetComponentsInChildren<TextMeshProUGUI>();
                TextMeshList.AddRange(textMeshes);
                ButtonObjList.Add(TextMeshList);
            }
            else if (i == 2)
            {
                GameObject obj = GameObject.Find("Popular" + x + "rd");
                TextMeshProUGUI[] textMeshes = obj.GetComponentsInChildren<TextMeshProUGUI>();
                TextMeshList.AddRange(textMeshes);
                ButtonObjList.Add(TextMeshList);
            }
            else if(i  > 2 && i < 7)
            {
                Debug.Log("x의 값" + x);
                GameObject obj = GameObject.Find("Popular" + x + "th");
                TextMeshProUGUI[] textMeshes = obj.GetComponentsInChildren<TextMeshProUGUI>();
                TextMeshList.AddRange(textMeshes);
                ButtonObjList.Add(TextMeshList);
            }
        }
       
        return ButtonObjList;
    }

  
    private void SetBtntextColor(string Day)
    {
        TextMeshProUGUI[] SetTxt = new TextMeshProUGUI[3];
        SetTxt[0] = SetTodayBtn.GetComponent<TextMeshProUGUI>();
        SetTxt[1] =  SetWeekBtn.GetComponent<TextMeshProUGUI>();
        SetTxt[2] = SetTotalBtn.GetComponent<TextMeshProUGUI>();

        if (Day == "today")
        {
            SetTxt[0].color = Color.yellow;
            SetTxt[1].color = Color.white;
            SetTxt[2].color = Color.white;
        }
        else if(Day == "week" )
        {
            SetTxt[0].color = Color.white;
            SetTxt[1].color = Color.yellow;
            SetTxt[2].color = Color.white;
        }
        else if(Day == "total")
        {
            SetTxt[0].color = Color.white;
            SetTxt[1].color = Color.white;
            SetTxt[2].color = Color.yellow;
        }

    }
    private void PoPularSearchBtnAction(List<List<TextMeshProUGUI>> PlaceObj)
    {
        Debug.Log(PlaceObj.Count + "placeobj 갯수" + TotalSearchCount.Count + "searchcoun 갯수");
        CurrentTag = "search";
        List<(string, int)> CurrentList = new List<(string, int)>();

        if( currentSetDay == "today")
        {
            CurrentList.Clear();
            CurrentList = TodaySearchCount;
        }
        else if (currentSetDay == "week")
        {
            CurrentList.Clear();
            CurrentList = WeekSearchCount;
        }
        else if (currentSetDay == "total")
        {
            CurrentList.Clear();
            CurrentList = TotalSearchCount;
        }

        int PlaceObjRow = PlaceObj[0].Count;
        for( int i = 0; i< PlaceObj.Count; i++) 
        {
            if (i < CurrentList.Count)
            {
               
                for (int j = 0; j < PlaceObjRow; j++)
                {
                    PlaceObj[i][0].transform.parent.gameObject.SetActive(true);
                    if(j == 0)
                    {
                        PlaceObj[i][j].text = "검색 횟수";
                    }
                    else if(j == 1)
                    {
                        PlaceObj[i][j].text = CurrentList[i].Item1.ToString();
                    }
                    else if (j == 2)
                    {
                        PlaceObj[i][j].text = CurrentList[i].Item2.ToString();
                    }
                
                }
                ApplyPopularThumbnail(i, CurrentList[i].Item1);
            }
            else
            {
                Debug.Log("i의 값" + i);
                GameObject ForSetFalse = PlaceObj[i][0].transform.parent.gameObject;
                ForSetFalse.SetActive(false);
            }
        }
    }
    
    private void PoPularEatenBtnAction(List<List<TextMeshProUGUI>> PlaceObj)
    {
        CurrentTag = "eaten";
        int PlaceObjRow = PlaceObj[0].Count;
        List<(string,int)> CurrentList = new List<(string, int)>();
        if (currentSetDay == "today")
        {
            CurrentList.Clear();
            CurrentList = TodayEatenCount;
        }
        else if (currentSetDay == "week")
        {
            CurrentList.Clear();
            CurrentList = WeekEatenCount;
        }
        else if (currentSetDay == "total")
        {
            CurrentList.Clear();
            CurrentList = TotalEatenCount;
        }

        for (int i = 0; i < PlaceObj.Count; i++)
        {
            if (i < CurrentList.Count)
            {
                for (int j = 0; j < PlaceObjRow; j++)
                {
                    PlaceObj[i][0].transform.parent.gameObject.SetActive(true);
                    if (j == 0)
                    {
                        PlaceObj[i][j].text = "먹은 횟수";
                    }
                    else if (j == 1)
                    {
                        PlaceObj[i][j].text = CurrentList[i].Item1.ToString();
                    }
                    else if (j == 2)
                    {
                        PlaceObj[i][j].text = CurrentList[i].Item2.ToString();
                    }

                }
                ApplyPopularThumbnail(i, CurrentList[i].Item1);
            }
            else
            {
                Debug.Log("i의 값" + i);
                GameObject ForSetFalse = PlaceObj[i][0].transform.parent.gameObject;
                ForSetFalse.SetActive(false);

            }
        }
    }
    
    private void ApplyPopularThumbnail(int index, string foodName)
    {
        int foodNo;
        string ordinal = index == 0 ? "st" : index == 1 ? "nd" : index == 2 ? "rd" : "th";
        GameObject row = GameObject.Find("Popular" + (index + 1) + ordinal);
        if (row == null)
            return;
        int resolvedFoodNumber = foodNumberByName.TryGetValue(foodName, out foodNo) ? foodNo : 0;
        // Reuse the photo/rank area already present in every PopularN row.
        // A separate 78px thumbnail was squeezed by the scene's X/Y scaling.
        RectTransform rowRect = row.GetComponent<RectTransform>();
        FoodImageCatalog.RemoveLegacyThumbnail(rowRect);
        Transform slot = row.transform.Find("RankPlaceImg");
        if (slot != null)
            FoodImageCatalog.ApplyToExistingSlot(slot.GetComponent<UnityEngine.UI.Image>(), resolvedFoodNumber);
        else
            FoodImageCatalog.ApplyThumbnail(rowRect, resolvedFoodNumber, 12f, 90f);
        FoodImageCatalog.FitPopularRow(row.transform);
    }

}
