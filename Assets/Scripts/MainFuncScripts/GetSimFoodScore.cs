using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;

public class GetSimFoodScore : MonoBehaviour
{
    private static string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private string currentID = "";

    private List<int> GetSimUserFoodNum = new List<int>();
    private List<GameObject> PrefabList = new List<GameObject>();
    private Dictionary<int, HashSet<string>> FoodUserDic = new Dictionary<int, HashSet<string>>();
    private Dictionary<string,Dictionary<int,int>> UserRatingsDic = new Dictionary<string,Dictionary<int,int>>();
    private Dictionary<int, string> FoodNumtoName = new Dictionary<int, string>();
    private Dictionary<string,double> cosineSimDic = new Dictionary<string,double>();
    public Transform SimUserScoreDataParent;
    public GameObject SimUserFoodsPrefab;
    public GameObject PanelCheck;

    public Button FoodEvaExitBtn;
    public GameObject FoodEvaPanel, FuntionPanel, MoreFoodEvaMessage;

    // MYSQL DB에서 취향 데이터를 가져오는 함수
   
    // Start is called before the first frame update
    void Start()
    {
        currentID = PlayerPrefs.GetString("playerid");
        FoodEvaExitBtn.onClick.AddListener(() => 
        {
            FoodEvaPanel.SetActive(false);
            FuntionPanel.SetActive(true);
            if (PanelCheck.activeInHierarchy)
            {
                GetDataBaseData();


                if (CheckMinUserFood(currentID, 10))
                {
                    List<(string, string, double)> RecArr = GetRecommFoodList(9);

                    SetNameScore(RecArr);
                    for (int i = 0; i < RecArr.Count; i++)
                    {
                        Debug.Log("비교한 아이디: " + RecArr[i].Item1 + " 추천 음식: " + RecArr[i].Item2 + ", 예상 점수: " + RecArr[i].Item3);
                    }
                    SetSimUserPanel(currentID);
                }
                else
                {
                    Debug.Log("실패");
                    ShowMoreEvaMessage();
                }
            }
        }
        );
        if (PanelCheck.activeInHierarchy)
        {
            GetDataBaseData();


            if (CheckMinUserFood(currentID,10))
            {
                List<(string, string, double)> RecArr = GetRecommFoodList(9);

                SetNameScore(RecArr);
                for (int i = 0; i < RecArr.Count; i++)
                {
                    Debug.Log("비교한 아이디: " + RecArr[i].Item1 + " 추천 음식: " + RecArr[i].Item2 + ", 예상 점수: " + RecArr[i].Item3);
                }
                SetSimUserPanel(currentID);
            }
            else
            {
                Debug.Log("실패");
                ShowMoreEvaMessage();
            }
        }
    }

    public void GetDataBaseData()
    {
        string ID;
        int FoodNum, Score;
        int SetFoodNumData;
        string SetFoodNameData;
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select * from foodtastescore", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            UserRatingsDic = new Dictionary<string, Dictionary<int, int>>();
            FoodUserDic = new Dictionary<int, HashSet<string>>();
            FoodNumtoName = new Dictionary<int, string>();
            while (reader.Read())
            {
                ID = reader.GetString("id");
                FoodNum = reader.GetInt32("foodnum");
                Score = reader.GetInt32("foodscore");
                if (!UserRatingsDic.ContainsKey(ID))
                {
                    UserRatingsDic.Add(ID, new Dictionary<int, int>());

                }
                UserRatingsDic[ID].Add(FoodNum, Score);

                if (!FoodUserDic.ContainsKey(FoodNum))
                {
                    FoodUserDic.Add(FoodNum, new HashSet<string>());
                }

                FoodUserDic[FoodNum].Add(ID);

            }
            reader.Close();
            cmd = new MySqlCommand("select foodnum,foodname from fooddata", conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SetFoodNumData = reader.GetInt32("foodnum");
                SetFoodNameData = reader.GetString("foodname");
                FoodNumtoName.Add(SetFoodNumData, SetFoodNameData);
            }
            reader.Close();


        }
        catch (MySqlException e)
        {
            Debug.Log(e.ToString());
        }
        finally { conn.Close(); };

    }

    private bool CheckMinUserFood(string userid, int minfoodcount)
    {
        if(!UserRatingsDic.ContainsKey(userid))
        {
            Debug.Log("평가데이터가 부족합니다. 더 많음 음식을 평가해주세요.");
            return false;
        }

        int myFoodcount = UserRatingsDic[userid].Count;

        if(myFoodcount < minfoodcount)
        {
            Debug.Log("평가데이터가 부족합니다. 더 많음 음식을 평가해주세요.");
            return false;
        }

        Dictionary<string,int> userRatingCount = new Dictionary<string, int>();

        foreach(var kvp in UserRatingsDic)
        {
            string otheruserid = kvp.Key;
            if( otheruserid != userid)
            {
                int commonRatingsCount = kvp.Value.Keys.Intersect(UserRatingsDic[userid].Keys).Count();
                userRatingCount[otheruserid] = commonRatingsCount;

                if(userRatingCount.Values.Count(count => count >= 2) >=2)
                {
                    return true;
                }

            }
        }

        Debug.Log("평가데이터가 부족합니다. 더 많음 음식을 평가해주세요.");
        return false;
    }

    private async void ShowMoreEvaMessage()
    {
        MoreFoodEvaMessage.SetActive(true);
        await Task.Delay(1000);
        MoreFoodEvaMessage.SetActive(false);
    }
    public List<(string, string, double)> GetRecommFoodList(int n)
    {
        (string, double)[] recommarr = new (string, double)[n];
        List<(string, string, double)> chkarr = new List<(string, string, double)>();
        List<string> list = new List<string>();
        List<double> PreScoreList = new List<double>();
        List<string> simUsers = GetTopUsers(currentID, 10);
        Dictionary<int, double> weightedScored = new Dictionary<int, double>();
        foreach (string userid in simUsers)
        {
            foreach (KeyValuePair<int, int> kvp in UserRatingsDic[userid])
            {
                int foodnum = kvp.Key;
                int score = kvp.Value;

                if (!weightedScored.ContainsKey(foodnum))
                {
                    weightedScored.Add(foodnum, 0);
                    list.Add(userid);
                }
                weightedScored[foodnum] = score * cosineSimDic[userid];

                Debug.Log("test score: "+ score + "test cosinesimDic" + cosineSimDic[userid] + "weightscore" + weightedScored[foodnum]);

            }
        }
        int i = 0;
        foreach (var kvp in weightedScored)
        {
            string cvtFoodnum = FoodNumtoName[kvp.Key];
            double PreScore = Math.Round(kvp.Value, 2);
            chkarr.Add((list[i], cvtFoodnum, PreScore));
            i++;
        }
        
        var sorteWeightedScores = chkarr.OrderByDescending(x => x.Item3).ToList();

        var GetHighScoreData = new List<(string, string, double)>();

        foreach (var item in sorteWeightedScores.Take(4))
        {
            GetHighScoreData.Add(item);
        }

        HashSet<int> randomkeys = new HashSet<int>();
        System.Random rand = new System.Random();

        while (randomkeys.Count < n - 4)
        {
            int randomKey = rand.Next(4,sorteWeightedScores.Count);
            if(!randomkeys.Contains(randomKey)) 
            { 
                randomkeys.Add(randomKey);
            }
        }
        Debug.Log("randomkey 갯수: "+randomkeys.Count);
        foreach (var randkey in randomkeys)
        {
            GetHighScoreData.Add(sorteWeightedScores[randkey]);
        }

        GetHighScoreData = GetHighScoreData.OrderByDescending(x => x.Item3).ToList();

        return GetHighScoreData;
    }

    private List<string> GetTopUsers(string userId, int n)
    {
        List<string> UserList = new List<string>();
        cosineSimDic = new Dictionary<string, double>();
        foreach (KeyValuePair<string, Dictionary<int, int>> kvp in UserRatingsDic)
        {
            if (kvp.Key != userId)
            {
                double cosineSim = CalculateCosineSim(UserRatingsDic[userId], kvp.Value, 2);
                Debug.Log("id " + userId + " 비교id " + kvp.Key + "유사도: " + cosineSim);
                if (cosineSim != 0)
                {
                    cosineSimDic.Add(kvp.Key, cosineSim);
                }
            }
        }
        var sortedCosineSim = cosineSimDic.OrderByDescending(x => x.Key);
        foreach (var item in sortedCosineSim.Take(n))
        {
            UserList.Add(item.Key);
        }


        return UserList;
    }

    private double CalculateCosineSim(Dictionary<int, int> DicA, Dictionary<int, int> DIcB, int minCom)
    {
        List<int> commonKey = DicA.Keys.Intersect(DIcB.Keys).ToList();
        if (commonKey.Count < minCom)
        {
            return 0;
        }
        // 각 유저가 평가한 음식의 평균
        double avgRatingA = DicA.Values.Average();
        double avgRatingB = DIcB.Values.Average();

        double dividend = 0;
        double divisorA = 0;
        double divisorB = 0;

        // Adjusted 코사인을 이용해 유사도를 계산
        foreach (KeyValuePair<int, int> kvp in DicA)
        {
            int key = kvp.Key;
            int RatingA = kvp.Value;
            int RatingB;
            if (DIcB.TryGetValue(key, out RatingB))
            {
                //두사람이 같이 평가한 음식의 평점에서 각 유저의 평가점수의 평균을 빼고 그 값들을 곱한 값을 계속해서 더해준다
                double adjRatingA = RatingA - avgRatingA;
                double adjRatingB = RatingB - avgRatingB;

                dividend += adjRatingA * adjRatingB;
                // 평가한 음식의 평점을 각각의 유저의 평균값으로 A 다음 그 값을 제곱하고 모두 합해준다
                divisorA += Math.Pow(adjRatingA, 2);
                divisorB += Math.Pow(adjRatingB, 2);
            }
        }
        // 위의 제곱한 각 사용자들로부터 가져온 DivisorA,B 값을 루트를 씌운다음 곱해준 값을 divideend를 나누는데 사용함.
        double sim = (dividend / (Math.Sqrt(divisorA) * Math.Sqrt(divisorB))+1)/2;

        return sim;
    }


    //내가 좋아할만한 음식의 이름과 예상점수를 TextUI로 출력
    private void SetNameScore(List<(string,string,double)> RecArr)
    {
        GameObject Image;
        
        for (int i = 0; i < RecArr.Count;i++)
        {
            int x = i+1;
            if (i == 0)
            {
                Image = GameObject.Find(x + "st");
            }
            else if( i == 1)
            {
                Image = GameObject.Find(x + "nd");
            }
            else if( i == 2)

            {
                Image = GameObject.Find(x + "rd");
            }
            else
            {
                Image = GameObject.Find(x + "th");
            }
            
            TextMeshProUGUI preScoreText = Image.transform.Find("PreScoreText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI PreFoodNameText = Image.transform.Find("PreFoodNameText").GetComponent<TextMeshProUGUI>();

            PreFoodNameText.text = RecArr[i].Item2;
            preScoreText.text = RecArr[i].Item3.ToString();


        }


    }
    private void SetSimUserPanel(string currentid)
    {
        Button UpButton = GameObject.Find("UpButton").GetComponent<Button>();
        Button DownButton = GameObject.Find("DownButton").GetComponent<Button>();
        Button LeftButton = GameObject.Find("LeftButton").GetComponent<Button>();
        Button RightButton = GameObject.Find("RightButton").GetComponent<Button>();

        GameObject SimUserId = GameObject.Find("SimUserId");
        TextMeshProUGUI SimUserIdText = SimUserId.GetComponent<TextMeshProUGUI>();
        GameObject SimUserScore = GameObject.Find("SimUserScore");
        TextMeshProUGUI SimUserScoreText = SimUserScore.GetComponent<TextMeshProUGUI>();
        Dictionary<string,int> CompareIdSotredScore = new Dictionary<string,int>();

        List<(string, int)> SimUsersFoodData = new List<(string, int)>();
       
        // 나와 비교하는 대상의 ID를 구분하는 index
        int Currentindex = 0;
        // 나와 비교하는 대상이 좋아하는 음식데이터를 가진 List의 Index
        int CurrentScoreIndx = 0;

        Debug.Log("CosineSimDic.count = " +cosineSimDic.Count);

        var sortedCosineSim = cosineSimDic.OrderByDescending(x => x.Value);
        List<(string, string)> CosinSimList = new List<(string, string)>();

        foreach(KeyValuePair<string,double> kvp in sortedCosineSim)
        {
            CosinSimList.Add((kvp.Key,Math.Round(kvp.Value*100,0).ToString()+"%"));

        }

        //처음 시작시 유사도가 제일 높은 사람의 음식정보 출력
        SimUserIdText.text = CosinSimList[0].Item1;
        SimUserScoreText.text = CosinSimList[0].Item2;
        CompareIdSotredScore = PickRandHighScore(UserRatingsDic[CosinSimList[0].Item1]);


        foreach(var item in CompareIdSotredScore)
        {
            SimUsersFoodData.Add((item.Key,item.Value));
            Debug.Log("정렬된 9개의 음식 이름: " +item.Key + " 점수: " +item.Value);
        }
       
        SetSimUserScorePreFabs(CurrentScoreIndx,SimUsersFoodData);


        // 위 화살표 버튼 클릭시 나와 유사도를 갖고 현재 출력되는 유사도 보다 높은사람의 음식정보 출력
        UpButton.onClick.AddListener(() =>
        {
            CurrentScoreIndx = 0;
            CompareIdSotredScore.Clear();
            
            Debug.Log("현재 인덱스: " + Currentindex);
            if (Currentindex == 0)
            {
                return;
            }
            else if (Currentindex >= 1)
            {
                SimUsersFoodData = new List<(string, int)>();

                Currentindex = Currentindex - 1;
                SimUserIdText.text = CosinSimList[Currentindex].Item1;
                SimUserScoreText.text = CosinSimList[Currentindex].Item2;
                CompareIdSotredScore = PickRandHighScore(UserRatingsDic[CosinSimList[Currentindex].Item1]);
                foreach (var item in CompareIdSotredScore)
                {
                    SimUsersFoodData.Add((item.Key, item.Value));
                    Debug.Log(CosinSimList[Currentindex].Item1 + "아이디 " + "음식이름: " + item.Key + "음식점수: " + item.Value);
                }
                SetSimUserScorePreFabs(CurrentScoreIndx, SimUsersFoodData);
            } 
        });
        // 아래버튼 클릭시 나와 유사도를 갖지만 현재 출력되는 유사도 보다 낮은사람의 음식정보 출력
        DownButton.onClick.AddListener(() =>
        {
            CurrentScoreIndx = 0;
            CompareIdSotredScore.Clear();
            Debug.Log("현재 인덱스: " + Currentindex);

            if (Currentindex >= 0 && Currentindex != CosinSimList.Count-1)
            {
                SimUsersFoodData = new List<(string, int)>();
                Currentindex = Currentindex + 1;

                SimUserIdText.text = CosinSimList[Currentindex].Item1;
                SimUserScoreText.text = CosinSimList[Currentindex].Item2;
                CompareIdSotredScore = PickRandHighScore(UserRatingsDic[CosinSimList[Currentindex].Item1]);

                foreach( var item in CompareIdSotredScore ) 
                {
                    SimUsersFoodData.Add((item.Key, item.Value));
                    Debug.Log(CosinSimList[Currentindex].Item1 + "아이디 " + "음식이름: " +item.Key + "음식점수: "+item.Value);
                }

                SetSimUserScorePreFabs(CurrentScoreIndx, SimUsersFoodData);
            }
            else if (Currentindex == CosinSimList.Count - 1) 
            {
                return;
            }
        });

        LeftButton.onClick.AddListener(() =>
        {
            Debug.Log("점수 인덱스: " + CurrentScoreIndx);
            if (CurrentScoreIndx == 0)
            {
                
            }
            else if(CurrentScoreIndx == 4 ) 
            {
                CurrentScoreIndx = CurrentScoreIndx - 4;
                SetSimUserScorePreFabs(CurrentScoreIndx, SimUsersFoodData);
            }
            else if(CurrentScoreIndx == 8)
            {
                CurrentScoreIndx = CurrentScoreIndx - 4;
                SetSimUserScorePreFabs(CurrentScoreIndx, SimUsersFoodData);
            }
        });

        RightButton.onClick.AddListener(() =>
        {
            Debug.Log("점수 인덱스: " + CurrentScoreIndx);
            if (CurrentScoreIndx == 0)
            {
                CurrentScoreIndx = CurrentScoreIndx +4;
                if(SimUsersFoodData.Count <= 4)
                {
                    CurrentScoreIndx = 0;
                }
                SetSimUserScorePreFabs(CurrentScoreIndx, SimUsersFoodData);
            }
            else if (CurrentScoreIndx == 4)
            {
                CurrentScoreIndx = CurrentScoreIndx +4;
                if(SimUsersFoodData.Count <= 8)
                {
                    CurrentScoreIndx = 4;
                }
                else if(SimUsersFoodData.Count <= 4)
                {
                    CurrentScoreIndx = 0;
                }

                SetSimUserScorePreFabs(CurrentScoreIndx, SimUsersFoodData);
            }
            else if (CurrentScoreIndx == 8)
            {

            }
        });


    }
    // Prefab 생성 메서드
    private void SetSimUserScorePreFabs(int idx , List<(string,int)> values)
    {
        List<(string, int)> FirstItems = new List<(string, int)>();
        List<(string, int)> SeCondItems = new List<(string, int)>();
        List<(string, int)> ThirdItems = new List<(string, int)>();

        List<int> FirstFoodNumList = new List<int> ();
        List<int> SecondFoodNumList = new List<int>();
        List<int> ThirdFoodNumList = new List<int>();

        for (int i = 0; i < values.Count; i++)
        {

            if (i < 4)
            {
                FirstItems.Add(values[i]);
                FirstFoodNumList.Add(GetSimUserFoodNum[i]);
            }
            if (i < 8 && i >= 4)
            {
                SeCondItems.Add(values[i]);
                SecondFoodNumList.Add(GetSimUserFoodNum[i]);
            }
            if (i < 12 && i >= 8)
            {
                ThirdItems.Add(values[i]);
                ThirdFoodNumList.Add(GetSimUserFoodNum[i]);
            }
        }
        if(idx  == 0) 
        {
            foreach (GameObject prefab in PrefabList)
            {
                Destroy(prefab);
            }
            PrefabList.Clear();
            ShowSimUserScorePreFabs(FirstItems,FirstFoodNumList);
        }
        else if(idx == 4)
        {
            foreach (GameObject prefab in PrefabList)
            {
                Destroy(prefab);
            }
            PrefabList.Clear();
            ShowSimUserScorePreFabs(SeCondItems, SecondFoodNumList);
        }
        else if(idx == 8)
        {
            foreach (GameObject prefab in PrefabList)
            {
                Destroy(prefab);
            }
            PrefabList.Clear();
            ShowSimUserScorePreFabs(ThirdItems, ThirdFoodNumList);
        }
    }

    private void ShowSimUserScorePreFabs(List<(string, int)> values , List<int> FoodNums)
    {
        float x = 0;
        float y = 240;
        if (values.Count == 0)
        {
            return;
        }

        for (int i = 0; i < values.Count; i++)
        {
            y = 240;
            GameObject obj = Instantiate(SimUserFoodsPrefab, SimUserScoreDataParent);
            PrefabList.Add(obj);
            obj.transform.localPosition = new Vector3(x,y-(170*i),0);
            TextMeshProUGUI FoodName = obj.transform.Find("FoodName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI FoodScore = obj.transform.Find("FoodScore").GetComponent<TextMeshProUGUI> ();
            Image FoodImg = obj.transform.Find("FoodImg").GetComponent<Image>();

            Debug.Log("사진에 나올 Food 번호: " + FoodNums[i]);
            string FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + FoodNums[i] + ".jpg";
            if(File.Exists(FoodImgUrl))
            {
                FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + FoodNums[i] + ".jpg";
            }
            else
            {
                FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + FoodNums[i] + ".png";
            }
            byte[] imagedata = System.IO.File.ReadAllBytes(FoodImgUrl);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imagedata);

            Sprite sprite = Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),Vector2.zero);
            FoodImg.sprite = sprite;


            FoodName.text = values[i].Item1.ToString();
            FoodScore.text = values[i].Item2.ToString() +"점";
        }

    }


    // 나와 유사한 사람이 평가한 음식정보중 점수가 4점 이상인 음식 중 12개를 랜덤으로 뽑아 Dictionary로 반환하는 메서드
    private Dictionary<string,int> PickRandHighScore(Dictionary<int,int> SimUserRatingDic)
    {
        GetSimUserFoodNum = new List<int>();
        Dictionary<int,int> ForOrderby = new Dictionary<int,int>();
        //평가한 유저의 음식점수가 4점이상인 음식만 선택
        List<int> SelectedKeys = SimUserRatingDic.Where(x => x.Value >= 4).Select(x => x.Key).ToList();
        
        //리스트에서 랜덤으로 12개의 key 값 선택
        List<int> RanDomKeys = SelectedKeys.OrderBy(x => Guid.NewGuid()).Take(12).ToList();

        Dictionary<string, int> ResultDic = new Dictionary<string, int>();
        foreach(int key in RanDomKeys)
        {
            ForOrderby.Add(key, SimUserRatingDic[key]);
            ResultDic.Add(FoodNumtoName[key], SimUserRatingDic[key]);
        }

        //ResultDic의 Value 값을 내림차순으로 정렬하고, 그 결과를 다시 Dictionary로 변환하여 ResultDic에 저장
        ResultDic = ResultDic.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        GetSimUserFoodNum = ForOrderby.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

        return ResultDic;
    }

   
 
    
}
