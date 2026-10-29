using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyInfoPanelManager : MonoBehaviour
{ 
    public GameObject SettingPanel;
    public Button SettingBtn, ExitPanelBtn, DeletePanelOpenBtn, ChangePassWordBtn;
    public TMPro.TextMeshProUGUI MyId, MyNickName, MyAvgScore, MyFoodCount;
    
    private string currentID = "dtdt5486";
    private string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";

    public GameObject FunctionPanel;
    private List<(string,int)> MyFoodDataList;
    public Transform FoodScrollViewTransform;
    public GameObject FoodInfoPrefab,PasswordChangePanel;
    private List<GameObject> FoodInfoList;
    public TMPro.TMP_InputField CurrentPassword, CurrentPasswordCheck, ChangePassword;
    public Button PassWordChangeConfirmBtn, PasswordChangePanelExitBtn;

    public TMPro.TMP_InputField NickNamechangePanelPassword, NewNickName;
    public Button NickNameChangeFuncBtn, NickNameChangePanelExitBtn, NickNameChangePanelOpenBtn;
    public GameObject NickNameChangePanel;
    private List<GameObject> OpenPanelGameObjList = new List<GameObject>();

    public Image ProfileImg;
    public GameObject DeleteIdPanel;
    public Button DeleteIdBtn, DeleteIdPanelExitBtn;
    public TMPro.TextMeshProUGUI DeleteMessage;
    void Start()
    {
        currentID = PlayerPrefs.GetString("playerid");
        OpenPanelGameObjList.Add(PasswordChangePanel);
        OpenPanelGameObjList.Add(NickNameChangePanel);
        OpenPanelGameObjList.Add(DeleteIdPanel);
        CurrentPassword.characterLimit = 15;
        CurrentPasswordCheck.characterLimit = 15;
        ChangePassword.characterLimit = 15;
        SettingBtn.onClick.AddListener(SettingBtnFunc);
        ChangePassWordBtn.onClick.AddListener(PasswordBtnFunc);
        PasswordChangePanelExitBtn.onClick.AddListener(PasswordBtnFunc);
        PassWordChangeConfirmBtn.onClick.AddListener(PasswordChangeBtnFunc);
        NickNameChangePanelOpenBtn.onClick.AddListener(NickNameChangePanelOpen);
        NickNameChangePanelExitBtn.onClick.AddListener(NickNameChangePanelOpen);
        NickNameChangeFuncBtn.onClick.AddListener(NickNameChangeBtnFunc);
        DeletePanelOpenBtn.onClick.AddListener(DeleteIdPanelOpenFunc);
        DeleteIdPanelExitBtn.onClick.AddListener(DeleteIdPanelOpenFunc);
        DeleteIdBtn.onClick.AddListener(DeleteIdFunc);
        ExitPanelBtn.onClick.AddListener(() => 
        {
            SettingPanel.SetActive(false);
            FunctionPanel.SetActive(true);
        });

    }

    private void Update()
    {
        if(CurrentPassword.isFocused || CurrentPasswordCheck.isFocused || ChangePassword.isFocused)
        {
            Input.imeCompositionMode = IMECompositionMode.Off;
        }
        else
        {
            Input.imeCompositionMode= IMECompositionMode.On;
        }
    }

    private void SettingBtnFunc()
    {
        FunctionPanel.SetActive(false);
        SettingPanel.SetActive(true);
        GetMyInfoData();
        int count = 0;
        foreach (var fooddata in MyFoodDataList)
        {
            SpawnMyFoodPrefab(fooddata.Item1, fooddata.Item2, count);
            count++;
        }
    }

    private void GetMyInfoData()
    {
        MyFoodDataList = new List<(string,int)> ();
        FoodInfoList = new List<GameObject> ();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select nickname, proimg from user where id = @currentid",conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            MySqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                MyNickName.text = reader.GetString(0);
                if(!reader.IsDBNull(1))
                {
                    long imgsize = reader.GetBytes(1, 0, null, 0, 0);
                    byte[] imgbytes = new byte[imgsize];
                    reader.GetBytes(1,0,imgbytes,0,(int)imgsize);
                    if(imgbytes.Length > 0)
                    {
                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(imgbytes);
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
                        ProfileImg.sprite = sprite;

                    }
                }
            }
            reader.Close();

            cmd = new MySqlCommand("select count(foodtastescore.foodnum), round(avg(foodtastescore.foodscore), 2), user.nickname from foodtastescore join user on foodtastescore.id = user.id where foodtastescore.id = @currentID",conn);
            cmd.Parameters.AddWithValue("@currentID", currentID);
            reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                MyId.text = currentID;
                MyFoodCount.text = reader.GetInt32(0).ToString() +" 개";
                if(!reader.IsDBNull(1))
                {
                    MyAvgScore.text = reader.GetDouble(1).ToString() + " 점";
                }
                else
                {
                    MyAvgScore.text = "0 점";
                }
                //MyNickName.text = reader.GetString(2);
            }
            reader.Close();

            cmd = new MySqlCommand("select foodname, foodscore from foodtastescore where id = @currentID ", conn);
            cmd.Parameters.AddWithValue("@currentID", currentID);
            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                string foodname = reader.GetString(0);
                int foodscore = reader.GetInt32(1);
                MyFoodDataList.Add((foodname, foodscore));
            }
            reader.Close();

        }
        catch(MySqlException e) 
        { 
            Debug.Log(e);
        }
        finally { conn.Close(); }
    }
    
    private void SpawnMyFoodPrefab(string foodname,int foodscore ,int count)
    {
        int yvalue = 0;
        GameObject clone = Instantiate(FoodInfoPrefab, FoodScrollViewTransform);
        FoodInfoList.Add(clone);
        clone.transform.localPosition = new Vector3(0, yvalue - (count * 100), 0);
        TextMeshProUGUI[] texts = clone.GetComponentsInChildren<TextMeshProUGUI>();
        texts[0].text = foodname;
        texts[1].text = foodscore.ToString() + " 점";
        TMPro.TMP_Dropdown dropdown = clone.GetComponentInChildren<TMP_Dropdown>();
        Button btn = clone.GetComponentInChildren<Button>();
        btn.onClick.AddListener(() =>
        {
            TMP_Dropdown.OptionData selectedOption = dropdown.options[dropdown.value];
            string selectedText = selectedOption.text;
            UpdateMyFoodScore(foodname,foodscore , selectedText, texts[1]);
            int totalscore = 0;
            foreach(GameObject g  in FoodInfoList)
            {
                TextMeshProUGUI[] t = g.GetComponentsInChildren<TextMeshProUGUI>();
                totalscore += int.Parse( t[1].text.Replace("점",""));
            }
            Debug.Log("총점 " + totalscore);
            Debug.Log("음식갯수: " +double.Parse(MyFoodCount.text.Replace("개", "")));
            double avg = totalscore / double.Parse(MyFoodCount.text.Replace("개",""));
            Debug.Log("avg :" + avg);
            double setavg = Math.Round(avg,2);
            Debug.Log("setavg: " + setavg);
            MyAvgScore.text = setavg.ToString()+" 점";
        });
    }
    
    private void UpdateMyFoodScore(string foodname,int PreviousFoodscore, string selectedText, TextMeshProUGUI FoodScoretextMeshProUgui)
    {
        int number;
        if (int.TryParse(selectedText, out number))
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("update foodtastescore set foodscore = @foodscore where id = @currentID and foodname = @foodname", conn);
                cmd.Parameters.AddWithValue("@foodscore", number);
                cmd.Parameters.AddWithValue("@currentID", currentID);
                cmd.Parameters.AddWithValue("@foodname", foodname);
                cmd.ExecuteNonQuery();

                FoodScoretextMeshProUgui.text = number.ToString() +" 점";
                
            }
            catch(MySqlException e)
            {
                Debug.Log(e.ToString());
            }
            finally { conn.Close(); }
        }
    }
    

    private void PasswordBtnFunc()
    {
        foreach(GameObject g in OpenPanelGameObjList)
        {
            Debug.Log("g.name: "+g.name);
            if( g.name == "PasswordChangePanel")
            {
                if(g.activeSelf)
                {
                    g.SetActive(false);
                }
                else
                {
                    g.SetActive(true);
                }
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    private void DeleteIdPanelOpenFunc()
    {
        foreach(GameObject g in OpenPanelGameObjList)
        {
            if(g.name == "DeleteIdPanel")
            {
                if (g.activeSelf)
                {
                    g.SetActive(false);
                }
                else { g.SetActive(true); }
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    private void NickNameChangePanelOpen()
    {
        foreach(GameObject g in OpenPanelGameObjList)
        {
            if(g.name == "NickNameChangePanel")
            {
                if(g.activeSelf)
                {
                    g.SetActive(false);
                }
                else
                {
                    g.SetActive(true);
                }
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    private async void PasswordChangeBtnFunc()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            string currentIDpwd = "";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select pwd from user where id = @currentid",conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            { 
                currentIDpwd = reader.GetString(0);
            }
            reader.Close();

            string currentpwd = CurrentPassword.text;
            string currentpwdCheck = CurrentPasswordCheck.text;
            
            if (currentpwd != currentpwdCheck) 
            {
                CurrentPassword.contentType = TMP_InputField.ContentType.Standard;
                CurrentPassword.text = "두 비밀번호가 일치하지 않습니다.";

                await Task.Delay(1000);

                CurrentPassword.contentType = TMP_InputField.ContentType.Password;
                CurrentPassword.text = "";
                CurrentPasswordCheck.text = "";
                ChangePassword.text = "";
            }
            else if (currentpwd != currentIDpwd || currentpwdCheck != currentIDpwd)
            {
                CurrentPassword.contentType = TMP_InputField.ContentType.Standard;
                CurrentPassword.text = "비밀번호가 틀렸습니다.";

                await Task.Delay(1000);

                CurrentPassword.contentType = TMP_InputField.ContentType.Password;
                CurrentPassword.text = "";
                CurrentPasswordCheck.text = "";
                ChangePassword.text = "";

            }
            else if ( currentpwd == currentIDpwd  ) 
            {
                cmd = new MySqlCommand("update user set pwd = @pwd where id = @currentid ", conn);
                cmd.Parameters.AddWithValue("@pwd", ChangePassword.text);
                cmd.Parameters.AddWithValue("@currentid", currentID);
                cmd.ExecuteNonQuery();

                CurrentPassword.contentType = TMP_InputField.ContentType.Standard;
                CurrentPassword.text = "비밀번호가 변경되었습니다.";

                await Task.Delay(1000);

                CurrentPassword.contentType = TMP_InputField.ContentType.Password;
                CurrentPassword.text = "";
                CurrentPasswordCheck.text = "";
                ChangePassword.text = "";

                PasswordChangePanel.SetActive(false);

            }
            
        }
        catch (MySqlException e) 
        {
            Debug.Log(e);
        }
        finally { conn.Close(); }
    }

    private async void NickNameChangeBtnFunc()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            string currentIDpwd = "";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select pwd from user where id = @currentid", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);

            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                currentIDpwd = reader.GetString(0);
            }
            reader.Close();

            string ForCheckPassword = NickNamechangePanelPassword.text;
            string NewNickNameText = NewNickName.text;

            if(ForCheckPassword != currentIDpwd)
            {
                NickNamechangePanelPassword.contentType = TMP_InputField.ContentType.Standard;
                NickNamechangePanelPassword.text = "비밀번호가 틀렸습니다.";
                await Task.Delay(1000);

                NickNamechangePanelPassword.contentType = TMP_InputField.ContentType.Password;
                NickNamechangePanelPassword.text = "";
                NewNickName.text = "";
            }
            else
            {
                cmd = new MySqlCommand("update user set nickname = @nickname where id = @currentid",conn);
                cmd.Parameters.AddWithValue("@nickname",NewNickNameText);
                cmd.Parameters.AddWithValue("@currentid", currentID);
                cmd.ExecuteNonQuery();

                NickNamechangePanelPassword.contentType = TMP_InputField.ContentType.Standard;
                NickNamechangePanelPassword.text = "닉네임이 변경되었습니다. ";
                await Task.Delay(1000);

                NickNamechangePanelPassword.contentType = TMP_InputField.ContentType.Password;
                NickNamechangePanelPassword.text = "";
                NewNickName.text = "";
                NickNameChangePanel.SetActive(false);
                MyNickName.text = NewNickNameText;

            }
        }
        catch (MySqlException e) 
        {
            Debug.Log(e);
            if (e.Message.Contains("Duplicate entry"))
            {
                NewNickName.text = "이미 존재하는 닉네임 입니다.";
                await Task.Delay(1000);

                NewNickName.text = "";

            }
        }
        finally
        {
            conn.Close();
        }
         
    }

    private async void DeleteIdFunc()
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("delete from comments where id = @currentid", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from foodtastescore where id = @currentid",conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from search_history where user_id = @currentid",conn);
            cmd.Parameters.AddWithValue("@currentid",currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from friend_requests where sender_id = @currentid or receiver_id = @currentid ", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from friendships where user_id_1 = @currentid or user_id_2 = @currentid", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from chat where sender_id = @currentid or receiver_id = @currentid", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from messages where sender_id = @currentid or receiver_id = @currentid", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("delete from user where id = @currentid", conn);
            cmd.Parameters.AddWithValue("@currentid", currentID);
            cmd.ExecuteNonQuery();


            DeleteMessage.text = "그동안 이용해주셔서 감사합니다.";
            ChangePassWordBtn.interactable = false;
            DeletePanelOpenBtn.interactable = false;
            ExitPanelBtn.interactable = false;
            NickNameChangePanelOpenBtn.interactable = false;
            DeleteIdBtn.interactable = false;
            DeleteIdPanelExitBtn.interactable = false;

            await Task.Delay(1000);
            
            Application.Quit();
        }
        catch(Exception e) 
        {
            Debug.Log(e);
        }
        finally { conn.Close(); }
    }

}
