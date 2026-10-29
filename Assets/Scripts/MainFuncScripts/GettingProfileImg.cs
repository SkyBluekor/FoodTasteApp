using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GettingProfileImg : MonoBehaviour
{
    private string CurrentId = "";
    private static string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + System.Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD") + ";allow user variables=true;";
    private string previousText;
    private float delayTime = 0.05f;
    private bool isfirstclick = true;
    private int previousPrefabcount = 0;

    public GameObject FriendViewContent;
    public Sprite userIcon;
    public Image UserProfileImg;
    public TextMeshProUGUI SimUserId;
    public Button FriendButton;
    
    private List<(string, byte[])> IDandBlobs = new List<(string, byte[])>();

    // Start is called before the first frame update
    void Start()
    {
        CurrentId = PlayerPrefs.GetString("playerid");
        if(SimUserId != null )
        {
            previousText = SimUserId.text;
        }
        GetBLOB(SimUserId.text);
        GetFriendBLOB();

        if(FriendButton != null) 
        {
            FriendButton.onClick.AddListener(FriendBtnClick);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(SimUserId != null && !previousText.Equals(SimUserId.text)) 
        { 
            TextChanged(SimUserId.text);
            previousText = SimUserId.text;
        }    
    }

    private void TextChanged(string newText)
    {
        Debug.Log("Text changed: " + newText);
        bool exists = IDandBlobs.Any(item => item.Item1.Equals(newText,StringComparison.OrdinalIgnoreCase));
        if(exists )
        {
            Debug.Log("리스트에 이미 존재");
            
        }
        else
        {
            Debug.Log("아이디와 BLOB 리스트에 추가");
            GetBLOB(newText);

            foreach (var item in IDandBlobs)
            {
                Debug.Log("idandBlods: ID: " + item.Item1 + " BLog: " + item.Item2);
            }

        }
        var tuple = IDandBlobs.FirstOrDefault(item => item.Item1.Equals(newText,StringComparison.OrdinalIgnoreCase));
        if(tuple != default)
        {
            if(tuple.Item2 == null)
            {
                UserProfileImg.sprite = userIcon;
            }
            else
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(tuple.Item2);
                Sprite iconsprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),Vector2.zero);
                UserProfileImg.sprite = iconsprite;
            }
        }
        else
        {
            Debug.Log("newID는 리스트에 존재하지 X");
        }


    }

    private void GetBLOB(string newID)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd;
        MySqlDataReader reader;
        try
        {
            conn.Open();
            string GetBLOBQuery = "select id,proimg from user where id = @userID";
            cmd = new MySqlCommand(GetBLOBQuery,conn);
            cmd.Parameters.AddWithValue("userID", newID);
            reader = cmd.ExecuteReader();
            while (reader.Read()) 
            { 
                string ID = reader.GetString(0);
                if(!reader.IsDBNull(reader.GetOrdinal("proimg")))
                {
                    byte[] imagedata = (byte[])reader["proimg"];
                    IDandBlobs.Add((ID, imagedata));
                }
                else
                {
                    byte[] nulldata = null;
                    IDandBlobs.Add((ID, nulldata));
                }
            }

        }
        catch(MySqlException e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            conn.Close();
        }

    }

    private void GetFriendBLOB()
    {
        MySqlConnection conn = new MySqlConnection (connStr);
        MySqlCommand cmd;
        MySqlDataReader reader;
        List<string> Friendids = new List<string>();
        try
        {
            conn.Open();
            string GetBLOBQuery = "select case when user_id_1 = @currentid then user_id_2 else user_id_1 end as friend_id from friendships where(user_id_1 = @currentid or user_id_2 = @currentid) and status = 'acceptance'";
            cmd = new MySqlCommand(GetBLOBQuery,conn);
            cmd.Parameters.AddWithValue("@currentid",CurrentId);
            reader = cmd.ExecuteReader();
            while (reader.Read()) 
            { 
                Friendids.Add(reader.GetString(0));
            }
            reader.Close();

            foreach(var item in IDandBlobs)
            {
                Friendids.RemoveAll(str => str == item.Item1);
            }

            foreach(string id in Friendids)
            {
                GetBLOB(id);
            }
        }
        catch(MySqlException e)
        {
            Debug.Log(e.Message);
        }
        finally 
        { 
            conn.Close(); 
        }

    }

    private void FriendBtnClick()
    {
        if(isfirstclick || previousPrefabcount == 0 || previousPrefabcount != FriendViewContent.transform.childCount)
        {
            isfirstclick = false;
            Invoke("DelayedAction", delayTime);
            previousPrefabcount = FriendViewContent.transform.childCount;
        }
    }
    private void DelayedAction()
    {        
        for(int i = 0; i < FriendViewContent.transform.childCount; i++)
        {
            Debug.Log("아이템 추가");
            GameObject childObj = FriendViewContent.transform.GetChild(i).gameObject;
            Debug.Log(childObj.name);
            Image[] imgComponent = childObj.GetComponentsInChildren<Image>();
            string friendidtext = childObj.GetComponentInChildren<TextMeshProUGUI>().text;
            string friendid = ExtractStringInParentheses(friendidtext);
            

            var tuple = IDandBlobs.FirstOrDefault(item => item.Item1.Equals(friendid, StringComparison.OrdinalIgnoreCase));
            if (tuple != default)
            {
                if (tuple.Item2 == null)
                {
                    
                }
                else
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(tuple.Item2);
                    Sprite iconsprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    imgComponent[1].sprite = iconsprite;
                }
            }
        }
    }

    string ExtractStringInParentheses(string input)
    {
        string[] splitInput = input.Split(new char[] { '(', ')' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (splitInput.Length > 1) // 괄호 안의 문자열이 존재하는 경우에만 반환
            return splitInput[1];

        return ""; // 괄호 안의 문자열이 없는 경우 빈 문자열 반환
    }
}
