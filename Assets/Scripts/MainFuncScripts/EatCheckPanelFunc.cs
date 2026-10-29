using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class EatCheckPanelFunc : MonoBehaviour
{
    private string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + System.Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD") + ";allow user variables=true;";
    public TMPro.TMP_Dropdown MorningDropDown,AfterNoonDropDown,NightDropDown;
    public TMP_InputField MorningText, AfterNoonText, NightText;
    public TextMeshProUGUI HeadText;
    private string CurrentId = "";
    private string todaydate = DateTime.Now.ToString("yyyy-MM-dd");
    private List<string> FoodOptions;
    public UnityEngine.UI.Button MorningInputBtn, MorningDeleteBtn, AfterNoonInputBtn, AfterNoonDeleteBtn , NightInputBtn, NightDeleteBtn;
    private UnityEngine.UI.Image MorningImg, AfternoonImg, NightImg;
    public GameObject EatenFoodPrefab, EatenPanel, FunctionPanel;
    public Transform PreFabParentTransform;
    public UnityEngine.UI.Button ExitBtn , TotalCountBtn, CheckEatenDayBtn;
    private List<GameObject> EatenFoodPrefabList = new List<GameObject>();
    public GameObject FoodCheckMessage, ScrollView;
    // Start is called before the first frame update
    void Start()
    {
        CurrentId = PlayerPrefs.GetString("playerid");
        TextMeshProUGUI GetTextCount = TotalCountBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCount.color = Color.yellow;
        SettingBackGround();
        GetDropdownOption();

        MorningText.onValueChanged.AddListener(MorningFocus);
        AfterNoonText.onValueChanged.AddListener(AfternoonFocus);
        NightText.onValueChanged.AddListener(NightFocus);

        MorningDropDown.onValueChanged.AddListener(delegate { DropDownFunc(MorningDropDown); });
        AfterNoonDropDown.onValueChanged.AddListener(delegate { DropDownFunc(AfterNoonDropDown); });
        NightDropDown.onValueChanged.AddListener(delegate { DropDownFunc(NightDropDown); });

        MorningInputBtn.onClick.AddListener(() => EatenBtnClckHandler("MorningInput"));
        AfterNoonInputBtn.onClick.AddListener(() => EatenBtnClckHandler("AfterNoonInput"));
        NightInputBtn.onClick.AddListener(() => EatenBtnClckHandler("NightInput"));
        
        MorningDeleteBtn.onClick.AddListener(() => DeleteBtnClickHandler("MorningDelete"));
        AfterNoonDeleteBtn.onClick.AddListener(() => DeleteBtnClickHandler("AfterNoonDelete"));
        NightDeleteBtn.onClick.AddListener(() => DeleteBtnClickHandler("NightDelete"));

        TotalCountBtn.onClick.AddListener(TotalEatenBtnFunc);
        CheckEatenDayBtn.onClick.AddListener(CheckEatenDayBtnFunc);


        ExitBtn.onClick.AddListener(ExitBtnFunc);
    }
    private void ExitBtnFunc()
    {
        EatenPanel.SetActive(false);
        FunctionPanel.SetActive(true);
    }

    private void TotalEatenBtnFunc()
    {
        HeadText.text = "내가 자주먹은 음식";
        TextMeshProUGUI GetTextCount = TotalCountBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCount.color = Color.yellow;
        TextMeshProUGUI GetTextCheck = CheckEatenDayBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCheck.color = Color.black;
        ShowEatenPreFab(RefreshEatenCount());
        ScrollRect scrollRect = ScrollView.GetComponentInChildren<ScrollRect>();
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0);
    }
    private void CheckEatenDayBtnFunc()
    {
        HeadText.text = "먹은지 오래된 음식";
        TextMeshProUGUI GetTextCount = TotalCountBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCount.color = Color.black;
        TextMeshProUGUI GetTextCheck = CheckEatenDayBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCheck.color = Color.yellow;
        ShowCheckEatenPrefab(RefreshCheckEatenList());
        ScrollRect scrollRect = ScrollView.GetComponentInChildren<ScrollRect>();
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0);
    }

    private void ShowCheckEatenPrefab( List<(string,string)> CheckEatenList)
    {
        foreach (GameObject g in EatenFoodPrefabList)
        {
            Destroy(g);
        }
        EatenFoodPrefabList.Clear();

        List<TextMeshProUGUI> textmeshes = new List<TextMeshProUGUI>();
        if(CheckEatenList.Count > 0)
        {
            for(int i = 0; i < CheckEatenList.Count; i++) 
            {
                GameObject item = Instantiate(EatenFoodPrefab);
                item.transform.SetParent(PreFabParentTransform, false);
                item.transform.localPosition = new Vector3(0, -i * 100, 0);
                textmeshes.AddRange(item.GetComponentsInChildren<TextMeshProUGUI>());
                EatenFoodPrefabList.Add(item);

                textmeshes[0].text = CheckEatenList[i].Item1;
                textmeshes[1].text = CheckEatenList[i].Item2;

                textmeshes.Clear();
            }
        }
    }

    private void ShowEatenPreFab (List<(string,int)> countEatenList)
    {
        foreach (GameObject g in EatenFoodPrefabList)
        {
            Destroy(g);
        }
        EatenFoodPrefabList.Clear();

        List<TextMeshProUGUI> textmeshes = new List<TextMeshProUGUI>();
        if(countEatenList.Count > 0) 
        {
            for (int i = 0; i < countEatenList.Count; i++)
            {
                GameObject item = Instantiate(EatenFoodPrefab);
                item.transform.SetParent(PreFabParentTransform, false);
                item.transform.localPosition = new Vector3(0, -i * 100, 0);
                textmeshes.AddRange(item.GetComponentsInChildren<TextMeshProUGUI>());
                EatenFoodPrefabList.Add(item);

                textmeshes[0].text = countEatenList[i].Item1;
                textmeshes[1].text = textmeshes[1].text + countEatenList[i].Item2.ToString()+"회";

                textmeshes.Clear();
            }
        }
    }
    private void SettingBackGround()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd = new MySqlCommand();
        conn.Open();
        List<(string,int)> CountEatenList = new List<(string,int)>();
        List<(string, string)> TodayEaten = new List<(string, string)>();
        List<(string, string)> CheckDayEatenList = new List<(string, string)>();
        MorningImg = MorningInputBtn.GetComponent<Image>();
        AfternoonImg = AfterNoonInputBtn.GetComponent<Image>();
        NightImg = NightInputBtn.GetComponent<Image>();
        try 
        { 
            cmd = new MySqlCommand("select foodname, count(*) as count from search_history where user_id = @currentid and  tag = 'eaten' group by foodname order by count desc",conn);
            cmd.Parameters.AddWithValue("@currentid", CurrentId);
            MySqlDataReader reader = cmd.ExecuteReader();
           
            while (reader.Read()) 
            {
                string foodname = reader.GetString(0);
                int count = reader.GetInt32(1);
                CountEatenList.Add((foodname,count));
            }
            ShowEatenPreFab(CountEatenList);
            reader.Close();

            cmd = new MySqlCommand("select foodname,eaten_tag from search_history where search_date = current_date() and eaten_tag in ('morning', 'afternoon', 'night') order by field(eaten_tag, 'morning', 'afternoon', 'night')", conn);
            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                string foodname = reader.GetString(0);
                string eaten_tag = reader.GetString(1);
                if(foodname != null && eaten_tag != null)
                {
                    TodayEaten.Add((foodname, eaten_tag));
                }
            }
            reader.Close();

            if (TodayEaten.Count > 0)
            {
                foreach (var item in TodayEaten)
                {
                    string foodname = item.Item1;
                    string eaten_tag = item.Item2;

                    if (eaten_tag == "morning")
                    {
                        MorningText.text = foodname;
                        MorningText.readOnly = true;
                        MorningInputBtn.interactable = false;
                        MorningInputBtn.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                    }
                    else if(eaten_tag == "afternoon")
                    {
                        AfterNoonText.text = foodname;
                        AfterNoonText.readOnly = true;
                        AfterNoonInputBtn.interactable = false;
                        AfterNoonInputBtn.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                    }
                    else if(eaten_tag == "night")
                    {
                        NightText.text = foodname;
                        NightText.readOnly = true;
                        NightInputBtn.interactable = false;
                        NightInputBtn.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                    }

            }
            }
        }
        catch (Exception ex) 
        { 
            Debug.Log(ex);
        }
        finally { conn.Close(); }


    }
    private void EatenBtnClckHandler(string BtnName)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd = new MySqlCommand();
        if (BtnName == "MorningInput")
        {
            if(FoodOptions.Contains(MorningText.text))
            {
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand("insert into search_history(user_id,tag,foodname,search_date,eaten_tag) values (@user_id, 'eaten',@foodname,@currentTime,'morning')",conn);
                    cmd.Parameters.AddWithValue("@user_id", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname",MorningText.text);
                    cmd.Parameters.AddWithValue("@currentTime",todaydate);
                    cmd.ExecuteNonQuery();
                    ShowEatenPreFab(RefreshEatenCount());

                    MorningText.readOnly = true;
                    MorningInputBtn.interactable = false;
                    MorningImg.color = new Color(0.5f, 0.5f, 0.5f);
                    
                }
                catch(MySqlException e )
                {
                    Debug.Log(e);
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                StartCoroutine(FoodCheckMessageSpawn(FoodCheckMessage));
                Debug.Log("선택 상자에서 존재하는 음식만 가능합니다.");
            }
        }
        else if (BtnName == "AfterNoonInput")
        {
            if (FoodOptions.Contains(AfterNoonText.text))
            {
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand("insert into search_history(user_id,tag,foodname,search_date,eaten_tag) values (@user_id, 'eaten',@foodname,@currentTime,'afternoon')", conn);
                    cmd.Parameters.AddWithValue("@user_id", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", AfterNoonText.text);
                    cmd.Parameters.AddWithValue("@currentTime", todaydate);
                    cmd.ExecuteNonQuery();
                    ShowEatenPreFab(RefreshEatenCount());

                    AfterNoonText.readOnly = true;
                    AfterNoonInputBtn.interactable = false;
                    AfternoonImg.color = new Color(0.5f, 0.5f, 0.5f);

                }
                catch (MySqlException e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                StartCoroutine(FoodCheckMessageSpawn(FoodCheckMessage));
                Debug.Log("선택 상자에서 존재하는 음식만 가능합니다.");
            }
        }
        else if (BtnName == "NightInput")
        {
            if (FoodOptions.Contains(NightText.text))
            {
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand("insert into search_history(user_id,tag,foodname,search_date,eaten_tag) values (@user_id, 'eaten',@foodname,@currentTime,'night')", conn);
                    cmd.Parameters.AddWithValue("@user_id", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", NightText.text);
                    cmd.Parameters.AddWithValue("@currentTime", todaydate);
                    cmd.ExecuteNonQuery();
                    ShowEatenPreFab(RefreshEatenCount());

                    NightText.readOnly = true;
                    NightInputBtn.interactable = false;
                    NightImg.color = new Color(0.5f, 0.5f, 0.5f);

                }
                catch (MySqlException e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                StartCoroutine(FoodCheckMessageSpawn(FoodCheckMessage));
                Debug.Log("선택 상자에서 존재하는 음식만 가능합니다.");
            }
        }

    }

    private List<(string,string)> RefreshCheckEatenList()
    {
        List<(string,string)> CheckDayEatenList = new List<(string,string)> ();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select foodname, MAX(search_date) as latest_search_date from search_history where tag = 'eaten' and user_id = @currentid Group by foodname order by latest_search_date asc", conn);
            cmd.Parameters.AddWithValue("@currentid", CurrentId);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string foodname = reader.GetString(0);
                DateTime datefromDB = reader.GetDateTime(1);
                DateTime CurrentDate = DateTime.Now.Date;

                TimeSpan diff = CurrentDate - datefromDB;
                int daysDiff = diff.Days;

                CheckDayEatenList.Add((foodname, daysDiff.ToString() + " 일전"));
            }
            reader.Close();
        }
        catch(MySqlException e)
        {
            Debug.Log(e);
        }
        finally { conn.Close(); }

        return CheckDayEatenList;
    }

    private List<(string,int)> RefreshEatenCount()
    {
        List<(string,int)> RefreshEatenFoodList = new List<(string,int)>();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select foodname, count(*) as count from search_history where user_id = @currentid and  tag = 'eaten' group by foodname order by count desc", conn);
            cmd.Parameters.AddWithValue("@currentid", CurrentId);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string foodname = reader.GetString(0);
                int count = reader.GetInt32(1);
                RefreshEatenFoodList.Add((foodname, count));
            }
            reader.Close();

        }
        catch(MySqlException e)
        {
            Debug.Log(e);
        }
        finally { conn.Close(); }

        return RefreshEatenFoodList;
    }
    private void DeleteBtnClickHandler(string BtnName)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd = new MySqlCommand();
        try
        {
            conn.Open();
            if (BtnName == "MorningDelete")
            {
                bool MorningReadonly = MorningText.readOnly;
                if (MorningReadonly)
                {
                    cmd = new MySqlCommand("delete from search_history where user_id = @currentid and foodname = @foodname and search_date = @today and eaten_tag = 'morning' ", conn);
                    cmd.Parameters.AddWithValue("@currentid", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", MorningText.text);
                    cmd.Parameters.AddWithValue("@today", todaydate);
                    cmd.ExecuteNonQuery();

                    ShowEatenPreFab(RefreshEatenCount());
                    MorningText.text = "";
                    MorningText.readOnly = false;
                    MorningInputBtn.interactable = true;
                    MorningImg.color = new Color(1f, 1f, 1f);

                }
                else
                {
                    MorningText.text = "";
                    Debug.Log("Check Log Morning");
                }
            }
            else if (BtnName == "AfterNoonDelete")
            {
                bool AfterReadonly = AfterNoonText.readOnly;
                if (AfterReadonly)
                {
                    cmd = new MySqlCommand("delete from search_history where user_id = @currentid and foodname = @foodname and search_date = @today and eaten_tag = 'afternoon'", conn);
                    cmd.Parameters.AddWithValue("@currentid", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", AfterNoonText.text);
                    cmd.Parameters.AddWithValue("@today", todaydate);
                    cmd.ExecuteNonQuery();
                    ShowEatenPreFab(RefreshEatenCount());

                    AfterNoonText.text = "";
                    AfterNoonText.readOnly = false;
                    AfterNoonInputBtn.interactable = true;
                    AfternoonImg.color = new Color(1f, 1f, 1f);
                }
                else
                {
                    Debug.Log("Check log AfterNoon");
                    AfterNoonText.text = "";   
                }
            }
            else if (BtnName == "NightDelete")
            {
                bool NightReadonly = NightText.readOnly;
                if (NightReadonly)
                {
                    cmd = new MySqlCommand("delete from search_history where user_id = @currentid and foodname = @foodname and search_date = @today and eaten_tag = 'night'", conn);
                    cmd.Parameters.AddWithValue("@currentid", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", NightText.text);
                    cmd.Parameters.AddWithValue("@today", todaydate);
                    cmd.ExecuteNonQuery();
                    ShowEatenPreFab(RefreshEatenCount());

                    NightText.text = "";
                    NightText.readOnly = false;
                    NightInputBtn.interactable = true;
                    NightImg.color = new Color(1f, 1f, 1f);
                }
                else
                {
                    NightText.text = "";
                    Debug.Log("Check log Night");
                }
            }
        }
        catch (MySqlException e)
        {
            Debug.Log(e);
        }
        finally
        {
            conn.Close();
        }
        
    }
    private bool IsDropDownClicked()
    {
        if(EventSystem.current != null) 
        { 
            if(EventSystem.current.currentSelectedGameObject != null)
            {
                TMP_Dropdown clickedDropdown = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_Dropdown>();
                if(clickedDropdown != null && clickedDropdown.IsExpanded) 
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void HideOption(string selText)
    {
        MorningDropDown.gameObject.SetActive(false);
        AfterNoonDropDown.gameObject.SetActive(false);
        NightDropDown.gameObject.SetActive(false);
    }
    private void MorningFocus(string selText)
    {
        if(string.IsNullOrEmpty(selText)) return;
        UpdateMorningDropDownOptions(MorningText, MorningDropDown);
        MorningDropDown.gameObject.SetActive(true);
        AfterNoonDropDown.gameObject.SetActive(false);
        NightDropDown.gameObject.SetActive(false);
    }
    private void AfternoonFocus(string selText)
    {
        if (string.IsNullOrEmpty(selText)) return;
        UpdateAfterNoonDropDownOptions(AfterNoonText, AfterNoonDropDown);
        MorningDropDown.gameObject.SetActive(false);
        AfterNoonDropDown.gameObject.SetActive(true);
        NightDropDown.gameObject.SetActive(false);
    }
    private void NightFocus(string selText)
    {
        if (string.IsNullOrEmpty(selText)) return;
        UpdateNightDropDownOptions(NightText, NightDropDown);
        MorningDropDown.gameObject.SetActive(false);
        AfterNoonDropDown.gameObject.SetActive(false);
        NightDropDown.gameObject.SetActive(true);
    }
    private void UpdateMorningDropDownOptions(TMP_InputField inputField, TMP_Dropdown dropdown)
    {
        string inputText = inputField.text.ToLower();
        dropdown.ClearOptions();

        List<string> FilterOption = FoodOptions.FindAll(option => option.ToLower().Contains(inputText));
        MorningDropDown.options.Insert(0, new TMP_Dropdown.OptionData("음식을 선택해주세요."));
        MorningDropDown.AddOptions(FilterOption);
    }
    private void UpdateAfterNoonDropDownOptions(TMP_InputField inputField, TMP_Dropdown dropdown)
    {
        string inputText = inputField.text.ToLower();
        dropdown.ClearOptions();

        List<string> FilterOption = FoodOptions.FindAll(option => option.ToLower().Contains(inputText));
        AfterNoonDropDown.options.Insert(0, new TMP_Dropdown.OptionData("음식을 선택해주세요."));
        AfterNoonDropDown.AddOptions(FilterOption);
    }
    private void UpdateNightDropDownOptions(TMP_InputField inputField, TMP_Dropdown dropdown)
    {
        string inputText = inputField.text.ToLower();
        dropdown.ClearOptions();

        List<string> FilterOption = FoodOptions.FindAll(option => option.ToLower().Contains(inputText));
        NightDropDown.options.Insert(0, new TMP_Dropdown.OptionData("음식을 선택해주세요."));
        NightDropDown.AddOptions(FilterOption);
    }
    private void DropDownFunc(TMP_Dropdown CurrentDropdown)
    {
        if(CurrentDropdown == MorningDropDown) 
        {
            MorningText.text = MorningDropDown.options[MorningDropDown.value].text;
            MorningDropDown.gameObject.SetActive(false);
        }
        else if(CurrentDropdown == AfterNoonDropDown)
        {
            AfterNoonText.text = AfterNoonDropDown.options[AfterNoonDropDown.value].text;
            AfterNoonDropDown.gameObject.SetActive(false) ;
        }
        else if(CurrentDropdown == NightDropDown) 
        {
            NightText.text = NightDropDown.options[NightDropDown.value].text;
            NightDropDown.gameObject.SetActive(false) ;

        }

    }

    private void GetDropdownOption()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd = new MySqlCommand();
        FoodOptions = new List<string>();
        try
        {
            conn.Open();
            cmd = new MySqlCommand("select foodname from fooddata", conn);

            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            { 
                FoodOptions.Add(reader.GetString(0));
            }
        }
        catch(MySqlException e) 
        {
            Debug.Log(e);
        }
        finally
        {
            conn.Close();
        }
        MorningDropDown.AddOptions(FoodOptions);
        AfterNoonDropDown.AddOptions(FoodOptions);
        NightDropDown.AddOptions(FoodOptions);

    }

    private IEnumerator FoodCheckMessageSpawn(GameObject g)
    {
        g.SetActive(true);

        yield return new WaitForSeconds(1f);

        g.SetActive(false);
    }

}
