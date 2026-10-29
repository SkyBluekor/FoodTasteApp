using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendshipFunc : MonoBehaviour
{
    private string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private string currentID;
    private List<(string, string)> CurrentMyfriends = new List<(string, string)>();
    private List<(string, string, string)> RequestInfoList;
    [SerializeField]
    private Transform RequestScrollViewTransform;
    [SerializeField]
    private Transform FriendListScrollViewTransform;
    private List<GameObject> FriendList = new List<GameObject>();
    private List<GameObject> RequestFrinedList = new List<GameObject>();
    private int previousFriCount = 0;


    public Button FriendShipBtn;
    public Button FriendListBtn;
    public Button FriendRequestListBtn;
    public Button FriendRequestBtn;

    public GameObject FriendPanel;
    public GameObject FriendListPanel;
    public GameObject FriendRequestPanel;
    public GameObject FriendRequestObj;
    public Button RequestExitBtn;
    public Button RequestBtn;
    public TMPro.TMP_InputField RequestIDinput;
    public GameObject FriendPrefab;
    public GameObject FriendRequestInfoPrefab;
    public GameObject RequestListPanel;

    public TMPro.TMP_InputField ChatContent;
    public Button ChatEnterBtn;
    public GameObject FriendChatPanel;
    [SerializeField]
    private Transform FriendChatScrollViewTransform;
    public GameObject MyMessagePrefab;
    public GameObject YourMessagePrefab;
    private List<GameObject> ChatPrefabList = new List<GameObject>();
    private List<Message> messages = new List<Message>();
    public ScrollRect ChatScrollRect;
    private Coroutine fetchMessageCoroutine;
    private Coroutine refreshMessageCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        currentID = PlayerPrefs.GetString("playerid");
        ChatContent.characterLimit = 45;
        FriendRequestBtn.onClick.AddListener(FriendRequestListBtnFunc);
        RequestExitBtn.onClick.AddListener(() =>
        {
            FriendRequestObj.SetActive(false);
        });
        RequestBtn.onClick.AddListener(RequestBtnFunc);
        FriendRequestListBtn.onClick.AddListener(RequestLIstBtnFunc);

        FriendShipBtn.onClick.AddListener(friendShipPanelFunc);
        FriendListBtn.onClick.AddListener(friendListBtnFunc);

    }

    private class Message
    {
        public string senderID;
        public string messageText;
        public DateTime send_at;
    }

    private void friendListBtnFunc()
    {
        if(RequestListPanel.activeSelf)
        {
            RequestListPanel.SetActive(false);
        }
        if (FriendChatPanel.activeSelf)
        {
            FriendChatPanel.SetActive(false);
            if (fetchMessageCoroutine != null)
            {
                StopCoroutine(fetchMessageCoroutine);
            }
            if (refreshMessageCoroutine != null)
            {
                StopCoroutine (refreshMessageCoroutine);
            }
        }
        FriendListShowFunc();
        FriendListPanel.SetActive(true);
        

    }
    private void friendShipPanelFunc()
    {
        if(FriendPanel.activeSelf)
        {
            FriendPanel.SetActive(false);
        }
        else
        {
            if(FriendRequestPanel.activeSelf)
            {
                FriendRequestPanel.SetActive(false);
            }
            if (FriendChatPanel.activeSelf)
            {
                FriendChatPanel.SetActive(false);
                if (fetchMessageCoroutine != null)
                {
                    StopCoroutine(fetchMessageCoroutine);
                }
                if (refreshMessageCoroutine != null)
                {
                    StopCoroutine(refreshMessageCoroutine);
                }
            }
            if(FriendRequestObj.activeSelf)
            {
                FriendRequestObj.SetActive(false);
            }
            FriendPanel.SetActive(true);
            FriendListShowFunc();
            FriendListPanel.SetActive(true);
        }
        
    }
    private void FriendListShowFunc()
    {
        GetFreindDataBase();
        if(previousFriCount == 0 || previousFriCount != CurrentMyfriends.Count)
        {
            foreach (GameObject g in FriendList)
            {
                Destroy(g);
            }
            FriendList.Clear();
            int count = 0;
            foreach (var g in CurrentMyfriends)
            {
                SpawnFriendListPrefab(g.Item1, g.Item2, count);
                count++;
            }
            previousFriCount = CurrentMyfriends.Count;
        }
        
    }
    private void FriendRequestListBtnFunc()
    {
        if(FriendRequestObj.activeSelf)
        {
            FriendRequestObj.SetActive(false);
        }
        else
        {
            FriendRequestObj.SetActive(true);
        }

        if(FriendChatPanel.activeSelf)
        {
            FriendChatPanel.SetActive(false);
            if(fetchMessageCoroutine != null)
            {
                StopCoroutine(fetchMessageCoroutine);
            }
            if (refreshMessageCoroutine != null)
            {
                StopCoroutine(refreshMessageCoroutine);
            }

        }

    }
    private void GetFreindDataBase()
    {
        CurrentMyfriends = new List<(string, string)>();
        RequestInfoList = new List<(string, string, string)>();
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd;
        try
        {
            conn.Open();
            cmd = new MySqlCommand("SELECT u.id AS friend_id, u.nickname AS friend_nickname FROM user AS u JOIN friendships AS f ON (u.id = f.user_id_1 OR u.id = f.user_id_2) WHERE (f.user_id_1 = @currentID OR f.user_id_2 = @currentID) AND f.status = 'acceptance' AND u.id != @currentID;", conn);
            cmd.Parameters.AddWithValue("@currentID", currentID);

            MySqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read()) 
            { 
                string myfriendid = reader.GetString(0);
                string nickname = reader.GetString(1);
                CurrentMyfriends.Add((myfriendid,nickname));
            }
            reader.Close();


            cmd = new MySqlCommand("SELECT fr.sender_id, fr.status, u.nickname FROM friend_requests fr JOIN user u ON fr.sender_id = u.id WHERE fr.receiver_id = @currentID", conn);
            cmd.Parameters.AddWithValue("@currentID", currentID);
            
            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                string senderid = reader.GetString(0);
                string status = reader.GetString(1);
                string nickname = reader.GetString(2);
                RequestInfoList.Add((senderid, status, nickname));
            }
            reader.Close();
        }
        catch (MySqlException e)
        {
            Debug.LogException(e);
        }
        finally
        {
            conn.Close();
        }
    }

    private void RequestBtnFunc()
    {
        GetFreindDataBase();

        string requestID = RequestIDinput.text;

        if(string.IsNullOrEmpty(requestID) ) 
        {
            RequestIDinput.text = "유효한 사용자 ID를 입력해주세요.";
            StartCoroutine(DisableFriendRequesObj(1));
            return;
        }

        bool containsItem = CurrentMyfriends.Exists(item => item.Item1.Contains(requestID));

        if(containsItem)
        {
            RequestIDinput.text = "이미 등록된 친구입니다.";
            StartCoroutine (DisableFriendRequesObj(1));
            return;
        }
        /* 수락 - acceptance
               거절 - rejection
               대기중 - pending
            */

        foreach (var receiver_id in RequestInfoList)
        {
            if(receiver_id.Item1 == requestID)
            {
                string Getstatus = receiver_id.Item2;
                if(Getstatus == "acceptance")
                {
                    RequestIDinput.text = "이미 수락한 친구입니다.";
                    StartCoroutine(DisableFriendRequesObj(1));
                    return;
                }
                else if( Getstatus == "rejection")
                {
                    RequestIDinput.text = "요청이 거절되었습니다.";
                    StartCoroutine(DisableFriendRequesObj(1));
                    return;
                }
                else if( Getstatus == "pending")
                {
                    RequestIDinput.text = "친구 수락을 대기중입니다.";
                    StartCoroutine(DisableFriendRequesObj(1));
                    return;
                }

            }
        }

        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select count(*) from user where id = @receiver_id ", conn);
            cmd.Parameters.AddWithValue("@receiver_id", RequestIDinput.text);
            MySqlDataReader reader = cmd.ExecuteReader();
            int Existid = 0;
            while (reader.Read()) 
            {
                Existid = reader.GetInt32(0);
            }
            reader.Close();

            Debug.Log("아이디 존재여부: " + Existid);
            if (Existid == 1)
            {
                cmd = new MySqlCommand("insert into friend_requests (sender_id, receiver_id, status) values (@sender_id, @receiver_id, @status)", conn);
                cmd.Parameters.AddWithValue("@sender_id", currentID);
                cmd.Parameters.AddWithValue("@receiver_id", RequestIDinput.text);
                cmd.Parameters.AddWithValue("@status", "pending");
                cmd.ExecuteNonQuery();
                RequestIDinput.text = "친구 요청을 보냈습니다.";
                StartCoroutine(DisableFriendRequesObj(1));
                return;
            }
            else 
            {
                RequestIDinput.text = "존재하지 않는 ID입니다.";
                StartCoroutine (DisableFriendRequesObj(1));
                return;
            }
        }
        catch(MySqlException e) 
        { 
            Debug.Log(e);
            if(e.Number == 1062)
            {
                RequestIDinput.text = "이미 친구등록이 된 상태입니다.";
                StartCoroutine(DisableFriendRequesObj(1));

            }
        }
        finally 
        { 
            conn.Close(); 
        }
    }

    private void RequestLIstBtnFunc()
    {
        if (FriendChatPanel.activeSelf)
        {
            FriendChatPanel.SetActive(false);
            if(fetchMessageCoroutine != null)
            {
                StopCoroutine(fetchMessageCoroutine);
            }
            
            if (refreshMessageCoroutine != null)
            {
                StopCoroutine(refreshMessageCoroutine);
            }
        }
        else
        {
            RequestListPanel.SetActive(true);
        }
        if(FriendListPanel.activeSelf) 
        { 
            FriendListPanel.SetActive(false);
        }
        else
        {
            RequestListPanel.SetActive(true);
        }
        
        GetFreindDataBase();
        RequestListPanel.SetActive(true);
        foreach(GameObject g in RequestFrinedList)
        {
            Destroy(g);
        }

        RequestFrinedList.Clear();

        int count = 0;
        foreach(var item in RequestInfoList)
        {
            if(item.Item2 == "pending")
            {
                SpawnRequestListPrefab(item, count);
                count++;
            }
        }
    }

    private void SpawnFriendListPrefab(string FriendID,string nickname ,int count)
    {
        int yValue = -35;
        GameObject clone = Instantiate(FriendPrefab,FriendListScrollViewTransform);
        FriendList.Add (clone);
        clone.transform.localPosition = new Vector3(0, yValue- (count*70), 0);
        TextMeshProUGUI text = clone.GetComponentInChildren<TextMeshProUGUI>();
        text.text = nickname +" (" + FriendID +")";
        Button btn = clone.GetComponentInChildren<Button>();
        btn.onClick.AddListener(() => 
        {
            //친구와의 채팅창으로 입장하는 버튼 Listener
            ChatRoomFunction(FriendID);
        });
    }

    private void ChatSendButtonFunc(string friendID)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            if (ChatContent.text.Length > 0)
            {
                MySqlCommand cmd = new MySqlCommand("insert into messages (sender_id, receiver_id, message, send_at) values (@sender_id,@receiver_id,@message,NOW())", conn);
                cmd.Parameters.AddWithValue("@sender_id", currentID);
                cmd.Parameters.AddWithValue("@receiver_id", friendID);
                cmd.Parameters.AddWithValue("@message", ChatContent.text);
                cmd.ExecuteNonQuery();

                Message m = new Message();
                m.senderID = currentID;
                m.messageText = ChatContent.text;
                m.send_at = DateTime.Now;
                messages.Add(m);
                GameObject clone = Instantiate(MyMessagePrefab, FriendChatScrollViewTransform);
                int count = ChatPrefabList.Count;
                ChatPrefabList.Add(clone);
                clone.transform.localPosition = new Vector3(0, (-70 * count), 0);
                TextMeshProUGUI[] texts = clone.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = ChatContent.text;
                texts[1].text = GetMessageTime(DateTime.Now);
            }
            Canvas.ForceUpdateCanvases();
            ChatScrollRect.verticalNormalizedPosition = 0f;
            ChatContent.text = "";
            

        }
        catch (MySqlException e) 
        {
            Debug.Log(e);
        }
        finally { conn.Close(); }
    }

    private void ChatInput(string input)
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
        {
            ChatEnterBtn.onClick.Invoke();
            ChatContent.Select();
            ChatContent.ActivateInputField();
        }
    }

    private void ChatRoomFunction(string friendID)
    {
        Debug.Log("ChatRoomFunc 메소드 진입");
        if(FriendListPanel.activeSelf || FriendRequestPanel.activeSelf) 
        { 
            FriendListPanel.SetActive(false);
            FriendRequestPanel.SetActive(false);
        }
        FriendChatPanel.SetActive(true);

        ChatEnterBtn.onClick.RemoveAllListeners();
        ChatContent.onEndEdit.RemoveAllListeners();

        ChatEnterBtn.onClick.AddListener(() =>
        {
            Debug.Log("ChatEnter버튼 기능 삽입");
            ChatSendButtonFunc(friendID);
        });

        Debug.Log("inputField 기능 삽입");
        ChatContent.onEndEdit.AddListener(ChatInput);

        
        GetFriendChatData(friendID);
        fetchMessageCoroutine = StartCoroutine(FetchMessagesRoutine(friendID));
        refreshMessageCoroutine = StartCoroutine(RefreshMessages());
    }

    private void GetFriendChatData(string friendID)
    {
        foreach (GameObject g in ChatPrefabList)
        {
            Destroy(g);
        }
        messages.Clear();
        ChatPrefabList.Clear();
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand( "Select * from messages where (sender_id = @currentID and receiver_id = @friendID) or (sender_id = @friendID and receiver_id = @currentID)" ,conn);
            cmd.Parameters.AddWithValue("@currentID", currentID);
            cmd.Parameters.AddWithValue("@friendID", friendID);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Message m = new Message();
                string sender_id = reader.GetString("sender_id");
                string message = reader.GetString("message");
                DateTime datetime = reader.GetDateTime("send_at");
                m.senderID = sender_id;
                m.messageText = message;
                m.send_at = datetime;
                messages.Add(m);
            }
            foreach(Message m in messages)
            {
                Debug.Log(m.senderID + "전송자 " + m.messageText);
            }

            messages.Sort((m1,m2) => m1.send_at.CompareTo(m2.send_at));
            DisPlayMessage(messages);
        }
        catch(MySqlException ex) 
        {
            Debug.Log(ex);
        }
        finally { conn.Close(); }
    }

    private void DisPlayMessage(List<Message> GetMessages)
    {
        int count = 0;
        foreach(Message m in GetMessages)
        {
            SpawnMessages(m,count);
            count++;
        }
    }

    private void SpawnMessages(Message m,int count)
    {
        int yvalue = 0;
        if(m.senderID == currentID)
        {
            GameObject clone = Instantiate(MyMessagePrefab,FriendChatScrollViewTransform);
            ChatPrefabList.Add(clone);
            yvalue = (-70 * count);
            clone.transform.localPosition = new Vector3(0, yvalue, 0);
            TextMeshProUGUI[] texts = clone.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = m.messageText;
            texts[1].text = GetMessageTime(m.send_at);
        }
        else
        {
            GameObject clone = Instantiate(YourMessagePrefab,FriendChatScrollViewTransform);
            ChatPrefabList.Add(clone);
            yvalue = (-70 * count);
            clone.transform.localPosition = new Vector3(0,yvalue, 0);
            TextMeshProUGUI[] texts = clone.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = m.messageText;
            texts[1].text = GetMessageTime(m.send_at);
        }
        Canvas.ForceUpdateCanvases();
        ChatScrollRect.verticalNormalizedPosition = 0f;
    }

    private String GetMessageTime(DateTime datetime)
    {
        TimeSpan timeDiff = DateTime.Now.Subtract(datetime);

        int minuteDiff = (int)timeDiff.TotalMinutes;
        int hoursDiff = (int)timeDiff.TotalHours;
        int daysDiff = (int)timeDiff.TotalDays;

        string relativeTime = string.Empty;
        if (minuteDiff < 60)
        {
            relativeTime = $"{minuteDiff}분 전";
        }
        else if (hoursDiff < 24)
        {
            relativeTime = $"{hoursDiff}시간 전";
        }
        else
        {
            relativeTime = $"{daysDiff}일 전";
        }
        return relativeTime;
    }

    private void SpawnRequestListPrefab((string, string, string) data, int count)
    {
        int yValue = -35;
        GameObject clone = Instantiate(FriendRequestInfoPrefab, RequestScrollViewTransform);
        RequestFrinedList.Add(clone);
        clone.transform.localPosition = new Vector3(0, yValue - (count * 70), 0);
        TextMeshProUGUI texts = clone.GetComponentInChildren<TextMeshProUGUI>();
        texts.text = data.Item3 + " (" + data.Item1 + ")";
        Button[] buttons = clone.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(() =>
        {
            RequestPrefabAcceptBtn(data.Item1, clone);
        });
        buttons[1].onClick.AddListener(() =>
        {
            RequestPrefabRejectBtn(data.Item1, clone);
        });
    }

    private void RequestPrefabAcceptBtn(string senderID , GameObject clone)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("update friend_requests set status = @statusStr where sender_id = @sender_id and receiver_id = @receiver_id ",conn);
            cmd.Parameters.AddWithValue("statusStr", "acceptance");
            cmd.Parameters.AddWithValue("sender_id", senderID);
            cmd.Parameters.AddWithValue("receiver_id", currentID);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand("insert into friendships (user_id_1, user_id_2, status) values(@receiver_id, @sender_id, @statusStr)",conn);
            cmd.Parameters.AddWithValue("receiver_id", currentID);
            cmd.Parameters.AddWithValue("sender_id", senderID);
            cmd.Parameters.AddWithValue("statusStr", "acceptance");
            cmd.ExecuteNonQuery();
            conn.Close();

            Destroy(clone);
            RequestFrinedList.Remove(clone);

        }
        catch(MySqlException e) 
        { 
            Debug.Log(e);
        }
        finally { conn.Close(); }

    }
    private void RequestPrefabRejectBtn(string senderID, GameObject clone)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("update friend_requests set status = @statusStr where sender_id = @sender_id and receiver_id = @receiver_id",conn);
            cmd.Parameters.AddWithValue("@statusStr", "rejection");
            cmd.Parameters.AddWithValue("@sender_id", senderID);
            cmd.Parameters.AddWithValue("@receiver_id", currentID);
            cmd.ExecuteNonQuery();
            conn.Close();

            Destroy(clone);
            RequestFrinedList.Remove(clone);

        }
        catch(MySqlException e)
        {
            Debug.Log(e);
        }
        finally { conn.Close(); }
    }
    private IEnumerator DisableFriendRequesObj(float delay)
    {
        yield return new WaitForSeconds(delay);
        FriendRequestObj.SetActive(false);
        RequestIDinput.text = string.Empty;
    }
    
    private IEnumerator FetchMessagesRoutine(string FriendID)
    {
        while (true) 
        {
            Debug.Log("Fetch 동작중");
            string selectQuery = "select * from messages where sender_id = @friendid and receiver_id = @currentid";
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand cmd;
            try
            {
                conn.Open();
                cmd = new MySqlCommand(selectQuery, conn);
                cmd.Parameters.AddWithValue("@friendid", FriendID);
                cmd.Parameters.AddWithValue("@currentid", currentID);

                Debug.Log("Reader 전");

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) 
                {
                    Debug.Log("Reader 중");
                    string message = reader.GetString("message");
                    string sender_id = reader.GetString("sender_id");
                    DateTime send_at = reader.GetDateTime("send_at");
                    Message newMessage = new Message();
                    newMessage.senderID = sender_id;
                    newMessage.messageText = message;
                    newMessage.send_at = send_at;
                    AddMessage(newMessage);

                }
                reader.Close();

                Debug.Log("Reader 후");
            }
            catch(MySqlException e) 
            {
                Debug.Log(e.Message);
            }
            finally
            { conn.Close(); };

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator RefreshMessages()
    {
        while (true) 
        {
            int count = 0;
            foreach(GameObject p in ChatPrefabList)
            {
                TextMeshProUGUI[] t = p.GetComponentsInChildren<TextMeshProUGUI>();
                t[1].text = GetMessageTime(messages[count].send_at);
                count++;
            }

            yield return new WaitForSeconds(60f);
        }

        
    }

    private void AddMessage(Message newMessage)
    {
        Debug.Log("ADDmessage");
        bool hasDuplicateDateTime  = false;
        foreach(Message message in messages) 
        { 
            if(message.send_at == newMessage.send_at) 
            { 
                hasDuplicateDateTime = true;
                break;
            }
        }

        if(!hasDuplicateDateTime) 
        {
            Debug.Log("추가됐냐");
            messages.Add(newMessage);
            GameObject clone = Instantiate(YourMessagePrefab, FriendChatScrollViewTransform);
            int count = ChatPrefabList.Count;
            ChatPrefabList.Add(clone);
            clone.transform.localPosition = new Vector3(0, (-150 * count), 0);
            TextMeshProUGUI[] texts = clone.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = newMessage.messageText;
            texts[1].text = GetMessageTime(newMessage.send_at);

            Canvas.ForceUpdateCanvases();
            ChatScrollRect.verticalNormalizedPosition = 0f;
        }

    }



}
