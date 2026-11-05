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
    private string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
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
    private bool showingOldest;
    private const float FoodRowHeight = 82f;
    private const float FoodRowSpacing = 8f;
    public GameObject FoodCheckMessage, ScrollView;
    // Start is called before the first frame update
    void Start()
    {
        CurrentId = PlayerPrefs.GetString("playerid");
        ConfigureFoodList();
        ConfigureFoodCheckMessage();
        TextMeshProUGUI GetTextCount = TotalCountBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCount.color = new Color(0.65f, 0.25f, 0.03f);
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
        showingOldest = false;
        HeadText.text = "내가 자주먹은 음식";
        TextMeshProUGUI GetTextCount = TotalCountBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCount.color = new Color(0.65f, 0.25f, 0.03f);
        TextMeshProUGUI GetTextCheck = CheckEatenDayBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCheck.color = Color.black;
        ShowEatenPreFab(RefreshEatenCount());
        ScrollRect scrollRect = ScrollView.GetComponentInChildren<ScrollRect>();
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0);
    }
    private void CheckEatenDayBtnFunc()
    {
        showingOldest = true;
        HeadText.text = "먹은지 오래된 음식";
        TextMeshProUGUI GetTextCount = TotalCountBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCount.color = Color.black;
        TextMeshProUGUI GetTextCheck = CheckEatenDayBtn.GetComponentInChildren<TextMeshProUGUI>();
        GetTextCheck.color = new Color(0.65f, 0.25f, 0.03f);
        ShowCheckEatenPrefab(RefreshCheckEatenList());
        ScrollRect scrollRect = ScrollView.GetComponentInChildren<ScrollRect>();
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0);
    }

    private void ShowCheckEatenPrefab( List<(string,string)> CheckEatenList)
    {
        ResizeFoodList(CheckEatenList.Count);
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
                PositionFoodRow(item, i);
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
        ResizeFoodList(countEatenList.Count);
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
                PositionFoodRow(item, i);
                textmeshes.AddRange(item.GetComponentsInChildren<TextMeshProUGUI>());
                EatenFoodPrefabList.Add(item);

                textmeshes[0].text = countEatenList[i].Item1;
                textmeshes[1].text = countEatenList[i].Item2.ToString() + "회";

                textmeshes.Clear();
            }
        }
    }

    private void ConfigureFoodList()
    {
        var scroll = ScrollView.GetComponent<ScrollRect>();
        // AutoHideAndExpandViewport drives the viewport rect back to full size,
        // overwriting the insets needed to keep rows inside the artwork.
        scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        var viewport = scroll.viewport;
        var mask = viewport.GetComponent<Mask>();
        if (mask != null) mask.enabled = false;
        if (viewport.GetComponent<RectMask2D>() == null)
            viewport.gameObject.AddComponent<RectMask2D>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(65, 60);
        viewport.offsetMax = new Vector2(-65, -105);

        var content = (RectTransform)PreFabParentTransform;
        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;
        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout != null) layout.enabled = false;
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
    }

    public void ConfigureFoodCheckMessage()
    {
        Transform header = FoodCheckMessage.transform.Find("Image");
        Transform body = FoodCheckMessage.transform.Find("Text (TMP)");
        if (header != null)
        {
            Image headerBackground = header.GetComponent<Image>();
            if (headerBackground != null) headerBackground.enabled = false;
            var rect = (RectTransform)header;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(75, 26);
            rect.sizeDelta = new Vector2(470, 38);
            var title = header.GetComponentInChildren<TextMeshProUGUI>(true);
            if (title != null)
            {
                title.color = new Color(.35f, .17f, .07f, 1f);
                title.alignment = TextAlignmentOptions.Center;
                title.enableAutoSizing = true;
                title.fontSizeMin = 16;
                title.fontSizeMax = 29;
                var titleRect = title.rectTransform;
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.one;
                titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
            }
        }
        if (body != null)
        {
            var rect = (RectTransform)body;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(75, -22);
            rect.sizeDelta = new Vector2(470, 65);
            var text = body.GetComponent<TextMeshProUGUI>();
            if (text != null) { text.enableWordWrapping = true; text.alignment = TextAlignmentOptions.Center; }
        }
    }

    private void ResizeFoodList(int count)
    {
        var content = (RectTransform)PreFabParentTransform;
        content.sizeDelta = new Vector2(0, Mathf.Max(count * (FoodRowHeight + FoodRowSpacing), 1f));
    }

    private static void PositionFoodRow(GameObject item, int index)
    {
        var row = (RectTransform)item.transform;
        row.anchorMin = new Vector2(0, 1);
        row.anchorMax = new Vector2(1, 1);
        row.pivot = new Vector2(.5f, .5f);
        row.sizeDelta = new Vector2(-12, FoodRowHeight);
        row.anchoredPosition = new Vector2(0, -FoodRowHeight * .5f - index * (FoodRowHeight + FoodRowSpacing));
        FitFoodRowLabel(row.Find("EatenFoodName") as RectTransform, .22f, .70f);
        FitFoodRowLabel(row.Find("CountEatenFood") as RectTransform, .70f, .97f);
    }

    private static void FitFoodRowLabel(RectTransform label, float left, float right)
    {
        if (label == null) return;
        label.anchorMin = new Vector2(left, 0);
        label.anchorMax = new Vector2(right, 1);
        label.offsetMin = new Vector2(4, 12);
        label.offsetMax = new Vector2(-4, -12);
        var text = label.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.enableWordWrapping = false;
            text.enableAutoSizing = true;
            text.fontSizeMin = 14;
            text.fontSizeMax = 25;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    private void RefreshCurrentList()
    {
        if (showingOldest) ShowCheckEatenPrefab(RefreshCheckEatenList());
        else ShowEatenPreFab(RefreshEatenCount());
    }
    private void SettingBackGround()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd = new MySqlCommand();
        List<(string,int)> CountEatenList = new List<(string,int)>();
        List<(string, string)> TodayEaten = new List<(string, string)>();
        List<(string, string)> CheckDayEatenList = new List<(string, string)>();
        MorningImg = MorningInputBtn.GetComponent<Image>();
        AfternoonImg = AfterNoonInputBtn.GetComponent<Image>();
        NightImg = NightInputBtn.GetComponent<Image>();
        try 
        { 
            conn.Open();
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

            cmd = new MySqlCommand("select foodname,eaten_tag from search_history where user_id = @currentid and tag = 'eaten' and search_date = current_date() and eaten_tag in ('morning', 'afternoon', 'night') order by field(eaten_tag, 'morning', 'afternoon', 'night')", conn);
            cmd.Parameters.AddWithValue("@currentid", CurrentId);
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
            if(TryResolveFoodName(MorningText))
            {
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand("insert into search_history(user_id,tag,foodname,search_date,eaten_tag) values (@user_id, 'eaten',@foodname,@currentTime,'morning')",conn);
                    cmd.Parameters.AddWithValue("@user_id", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname",MorningText.text);
                    cmd.Parameters.AddWithValue("@currentTime",todaydate);
                    cmd.ExecuteNonQuery();
                    RefreshCurrentList();

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
            if (TryResolveFoodName(AfterNoonText))
            {
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand("insert into search_history(user_id,tag,foodname,search_date,eaten_tag) values (@user_id, 'eaten',@foodname,@currentTime,'afternoon')", conn);
                    cmd.Parameters.AddWithValue("@user_id", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", AfterNoonText.text);
                    cmd.Parameters.AddWithValue("@currentTime", todaydate);
                    cmd.ExecuteNonQuery();
                    RefreshCurrentList();

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
            if (TryResolveFoodName(NightText))
            {
                try
                {
                    conn.Open();
                    cmd = new MySqlCommand("insert into search_history(user_id,tag,foodname,search_date,eaten_tag) values (@user_id, 'eaten',@foodname,@currentTime,'night')", conn);
                    cmd.Parameters.AddWithValue("@user_id", CurrentId);
                    cmd.Parameters.AddWithValue("@foodname", NightText.text);
                    cmd.Parameters.AddWithValue("@currentTime", todaydate);
                    cmd.ExecuteNonQuery();
                    RefreshCurrentList();

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

                    RefreshCurrentList();
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
                    RefreshCurrentList();

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
                    RefreshCurrentList();

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
    private bool updatingFoodOptions;

    private bool TryResolveFoodName(TMP_InputField input)
    {
        string entered = input.text.Trim();
        string match = FoodOptions.Find(option =>
            string.Equals(option.Trim(), entered, StringComparison.OrdinalIgnoreCase));
        if (match == null) return false;
        input.SetTextWithoutNotify(match);
        return true;
    }

    private void UpdateFoodDropDownOptions(TMP_InputField input, TMP_Dropdown dropdown)
    {
        updatingFoodOptions = true;
        try
        {
            string query = input.text.Trim();
            var matches = FoodOptions.FindAll(option =>
                option.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
            dropdown.ClearOptions();
            dropdown.options.Add(new TMP_Dropdown.OptionData("음식을 선택해주세요."));
            dropdown.AddOptions(matches);
            dropdown.SetValueWithoutNotify(0);
            dropdown.RefreshShownValue();
        }
        finally { updatingFoodOptions = false; }
    }

    private void UpdateMorningDropDownOptions(TMP_InputField input, TMP_Dropdown dropdown)
    {
        UpdateFoodDropDownOptions(input, dropdown);
    }
    private void UpdateAfterNoonDropDownOptions(TMP_InputField input, TMP_Dropdown dropdown)
    {
        UpdateFoodDropDownOptions(input, dropdown);
    }
    private void UpdateNightDropDownOptions(TMP_InputField input, TMP_Dropdown dropdown)
    {
        UpdateFoodDropDownOptions(input, dropdown);
    }
    private void DropDownFunc(TMP_Dropdown dropdown)
    {
        if (updatingFoodOptions || dropdown.value <= 0 || dropdown.value >= dropdown.options.Count)
            return;
        TMP_InputField input = dropdown == MorningDropDown ? MorningText :
            dropdown == AfterNoonDropDown ? AfterNoonText : NightText;
        input.SetTextWithoutNotify(dropdown.options[dropdown.value].text);
        dropdown.gameObject.SetActive(false);
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
