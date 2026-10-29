using Google.Protobuf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto.Agreement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class For_Search_history : MonoBehaviour
{
    private string connStr =  "Server=localhost;database=tasteapp;userid=root;password=" + System.Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD") + ";allow user variables=true;";
    private string CurrentId = "dtdt5486";
    private string todaydate = DateTime.Now.ToString("yyyy-MM-dd");
    private string AvgFoodScore, TotalFoodSearchCount, TodaySearchCount;
    public GameObject FoodContentPanel , FunctionPanel;
    public TextMeshProUGUI FoodNametxt, RateTxt, SearchCountTxt, TodaySearchTxt;
    public TMP_InputField ReplyInput;
    public Image FoodContentFoodimg;
    public Transform ReplyScroll;
    public GameObject ReplyPreFab , ContentMessage;
    private List<GameObject> ReplyPreFabList = new List<GameObject>();

    private void Start()
    {
        CurrentId = PlayerPrefs.GetString("playerid");
    }
    public void ForPrePanelObjFunction()
    {
        GameObject ThisGameObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        int CurrentFoodNum = 0;

        List<TextMeshProUGUI> textmeshes = new List<TextMeshProUGUI>();
        Transform[] children = ThisGameObj.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            TextMeshProUGUI textmesh = child.GetComponent<TextMeshProUGUI>();
            if (textmesh != null && textmesh.text !="예상 점수" && textmesh.text != "검색 횟수" && textmesh.text != "먹은 횟수")  
            { 
                textmeshes.Add(textmesh);
                Debug.Log(textmesh.text);
            }
        }
        
        MySqlConnection conn = new MySqlConnection(connStr);

        
        MySqlCommand cmd = new MySqlCommand();
       
        try
        {
            conn.Open();
            cmd = new MySqlCommand("select foodnum from fooddata where foodname = @foodname", conn);
            cmd.Parameters.AddWithValue("@foodname", textmeshes[0].text);

            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CurrentFoodNum = reader.GetInt32("foodnum");
            }
            reader.Close();
            string checkQuery = "select count(*) from search_history where tag = @tag and user_id = @user_id and foodname = @foodname and search_date = @currentTIme";
            cmd = new MySqlCommand(checkQuery, conn);
            cmd.Parameters.AddWithValue("@user_id", CurrentId);
            cmd.Parameters.AddWithValue("@foodname", textmeshes[0].text);
            cmd.Parameters.AddWithValue("@currentTime", todaydate);
            cmd.Parameters.AddWithValue("@tag", "search");


            int count = Convert.ToInt32(cmd.ExecuteScalar());
            Debug.Log("COunt: " +count);
            if (count > 0)
            {
                
            }
            else
            {
                cmd = new MySqlCommand("insert ignore into search_history(user_id,tag,foodname,search_date) values(@user_id, 'search' , @foodname, @currentTime)", conn);
                cmd.Parameters.AddWithValue("@user_id", CurrentId);
                cmd.Parameters.AddWithValue("@foodname", textmeshes[0].text);
                cmd.Parameters.AddWithValue("@currentTime", todaydate);

                cmd.ExecuteNonQuery();
            }

            FunctionPanel.SetActive(false);
            SetFoodContentPanel(CurrentFoodNum, textmeshes[0].text);
            FoodContentPanel.SetActive(true);
            ShowReplyContent();
            Button ReplyInputBtn = GameObject.Find("ReplyInputBtn").GetComponent<Button>();
            ReplyInput.onEndEdit.AddListener((string value) => SubmitForm(value, ReplyInputBtn));


        }
        catch (Exception e) 
        { 
            Debug.Log(e.ToString());
        }
        finally { conn.Close(); }



        Debug.Log("현재 음식 번호: " +CurrentFoodNum + " 현재 음식 이름: " + textmeshes[0].text);

    }

    private void SetFoodContentPanel(int CurrentFoodnum, string Foodname)
    {
        string FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + CurrentFoodnum + ".jpg";
        if(File.Exists(FoodImgUrl)) 
        {
            FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + CurrentFoodnum + ".jpg";
        }
        else
        {
            FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + CurrentFoodnum + ".png";
        }
        byte[] imagedata = System.IO.File.ReadAllBytes(FoodImgUrl);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imagedata);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        FoodContentFoodimg.sprite = sprite;
        
        FoodNametxt.text = Foodname;

        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select foodnum, round(avg(foodscore), 1) as avg_score from foodtastescore where foodnum = @foodnum group by foodnum", conn);
            cmd.Parameters.AddWithValue("@foodnum", CurrentFoodnum);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                AvgFoodScore = reader.GetDouble("avg_score").ToString();
            }
            reader.Close();

            cmd = new MySqlCommand("select count(*) from search_history where foodname = @foodname and tag = @tag",conn);
            cmd.Parameters.AddWithValue("@foodname", Foodname);
            cmd.Parameters.AddWithValue("@tag", "search");

            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                TotalFoodSearchCount = reader.GetInt32(0).ToString();
            }
            reader.Close();

            cmd = new MySqlCommand("select count(*) from search_history where foodname = @foodname and tag = @tag and search_date = @today",conn);
            cmd.Parameters.AddWithValue("@foodname", Foodname);
            cmd.Parameters.AddWithValue("@tag", "search");
            cmd.Parameters.AddWithValue("@today", todaydate);

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TodaySearchCount = reader.GetInt32(0).ToString();
            }
            reader.Close();


        }
        catch (MySqlException e) 
        { 
            Debug.Log(e);
        }
        finally { conn.Close(); }
        
        RateTxt.text = "평균 점수\n" +AvgFoodScore +"점";
        TodaySearchTxt.text = "오늘 검색 횟수\n" + TodaySearchCount + "번";
        SearchCountTxt.text = "총 검색 횟수\n" + TotalFoodSearchCount + "번";
    }
    private void ShowReplyContent()
    {
        foreach (GameObject g in ReplyPreFabList)
        {
            Destroy(g);
        }
        ReplyPreFabList.Clear();
        string FoodName = FoodNametxt.text;
        MySqlConnection conn = new MySqlConnection(connStr);
        List<(string,DateTime,string,int,int)> ReplyList = new List<(string, DateTime, string, int,int)>();
        Dictionary<string, byte[]> UserProDic = new Dictionary<string, byte[]>();
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select c.id, c.replydate, c.content, fts.foodscore, c.comment_id, u.proimg from comments c left join foodtastescore fts on c.foodname = fts.foodname and c.id = fts.id left join user u on c.id = u.id where c.foodname = @foodname order by c.replydate desc",conn);
            
            cmd.Parameters.AddWithValue("@foodname", FoodName);
            MySqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read()) 
            {
                int foodscore = 0;
                string userid = reader.GetString("id");
                if(!reader.IsDBNull(reader.GetOrdinal("foodscore")))
                {
                    foodscore = reader.GetInt32("foodscore");
                }
                ReplyList.Add((userid, reader.GetDateTime("replydate"), reader.GetString("content"),foodscore,reader.GetInt32("comment_id")));
                int proimgindex = reader.GetOrdinal("proimg");
                long imgsize = reader.GetBytes(proimgindex, 0, null, 0, 0);
                byte[] imgbytes = new byte[imgsize];
                reader.GetBytes(proimgindex, 0, imgbytes, 0, (int)imgsize);
                if (!UserProDic.ContainsKey(userid))
                {
                    UserProDic.Add(userid, imgbytes);
                }
                

            }
            reader.Close();


        }
        catch (MySqlException ex) 
        {
            Debug.Log(ex);
        }
        finally { conn.Close(); }

        List<TextMeshProUGUI> textmeshes = new List<TextMeshProUGUI>();
        if(ReplyList.Count > 0) 
        { 
            for( int i = 0; i< ReplyList.Count; i++)
            {
                 
                GameObject item = Instantiate(ReplyPreFab);
                Image contentproImg = item.GetComponentsInChildren<Image>().LastOrDefault();
                Button DeleteBtn = item.GetComponentInChildren<Button>();
                item.transform.SetParent(ReplyScroll,false);
                item.transform.localPosition = new Vector3(0,-i * 100, 0);
                DeleteBtn.onClick.AddListener(DeleteReply);
                textmeshes.AddRange(item.GetComponentsInChildren<TextMeshProUGUI>());
                ReplyPreFabList.Add(item);

                foreach (TextMeshProUGUI items in textmeshes)
                {
                    Debug.Log("textmeshpro 이름: " + items.name);
                }
                if (ReplyList[i].Item1 != CurrentId)
                {
                    DeleteBtn.gameObject.SetActive(false);
                }
                textmeshes[0].text = ReplyList[i].Item1; //아이디
                textmeshes[1].text = GetTimeAgo(ReplyList[i].Item2); // 게시날짜
                textmeshes[2].text = ReplyList[i].Item3.ToString(); // 내용
                textmeshes[3].text = ReplyList[i].Item4.ToString() +"점"; // 점수
                if (textmeshes[3].text == "0점")
                {
                    textmeshes[3].text = "미평가";
                }
                textmeshes[4].text = ReplyList[i].Item5.ToString(); // 댓글ID
                byte[] proimg = UserProDic[ReplyList[i].Item1];
                if(proimg.Length > 0)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(proimg);
                    Sprite sprtie = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),Vector2.one *0.5f);
                    contentproImg.sprite = sprtie;
                }
                textmeshes.Clear();

            }

           

        }

    }


    public void InsertReply()
    {
        string InsertReply = ReplyInput.text;
        
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("insert into comments (foodname,id,replydate,content) values (@foodname,@id,@replydate,@content)",conn);
            cmd.Parameters.AddWithValue("foodname",FoodNametxt.text);
            cmd.Parameters.AddWithValue("id",CurrentId);
            cmd.Parameters.AddWithValue("replydate",DateTime.Now);
            cmd.Parameters.AddWithValue("content", InsertReply);
            if(InsertReply != "") 
            {
                cmd.ExecuteNonQuery();
            }
            else
            {
                ContentMessage.transform.SetAsLastSibling();
                ContentMessage.SetActive(true);
                StartCoroutine(DeactivateAfterDelay(2f));
            }
            ReplyInput.text = "";
            
            
            ShowReplyContent();
        }
        catch (MySqlException ex) 
        {
            Debug.Log(ex);
        }
        finally 
        { 
            conn.Close(); 
        }
    }
    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ContentMessage.SetActive(false);
    }

    private void SubmitForm(string value, Button InputBtn)
    {
        if(Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            InputBtn.onClick.Invoke();
        }
    }

    public void DeleteReply()
    {
        Button DeleteBtn = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        GameObject ParentObj = DeleteBtn.transform.parent.gameObject;
        List<TextMeshProUGUI> TextList = new List<TextMeshProUGUI>();
        TextList.AddRange(ParentObj.GetComponentsInChildren<TextMeshProUGUI>());
        int CommentId =  int.Parse(TextList[TextList.Count - 1].text);
        Debug.Log("댓글 번호: "+CommentId);

        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("delete from comments where comment_id = @commentid", conn);
            cmd.Parameters.AddWithValue("commentid", CommentId);
            cmd.ExecuteNonQuery();

            ShowReplyContent();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
        finally { conn.Close(); }

    }

    private string GetTimeAgo(DateTime time)
    {
        TimeSpan timeSince = DateTime.Now.Subtract(time);

        if (timeSince.TotalMinutes < 1)
        {
            return "방금 전";
        }
        else if (timeSince.TotalMinutes < 60)
        {
            return $"{(int)timeSince.TotalMinutes}분 전";
        }
        else if (timeSince.TotalHours < 24)
        {
            return $"{(int)timeSince.TotalHours}시간 전";
        }
        else if (timeSince.TotalDays > 7)
        {
            return $"{(int)timeSince.TotalDays}일 전";
        }
        else
        {
            return $"{(int)(timeSince.TotalDays / 7)}주 전";
        }
    }
    public void ContentPanelExitBtn()
    {
        FoodContentPanel.SetActive(false);
        FunctionPanel.SetActive(true);
    }


}
