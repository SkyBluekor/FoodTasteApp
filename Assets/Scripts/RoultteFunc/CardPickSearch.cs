using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

public class CardPickSearch : MonoBehaviour
{
    private CardMoveFunc cardMoveFunc;
    public GameObject cardMoveObj;
    public Button ShakeBtn, ExitBtn, RandomPickBtn, RefreshBtn;
    public GameObject[] CardPreFab = new GameObject[5]; 
    private List<Vector3> CardVec3 = new List<Vector3>();
    private GameObject[] CardSpawnPrefabs;
    public GameObject ScrollView;
    public TMP_Dropdown TmpDropdown;
    public int maxPrefabCount = 9;
    private int currentPrefabCount = 0;
    public GameObject FoodTrayPreFab;
    public List<Vector3> spawnVec3 = new List<Vector3>();
    private GameObject[] spawnedPrefabs;
    public GameObject OptionPanel, CardPanel;
    [SerializeField]
    TMP_InputField SearchingInput;
    private Dictionary<string, string> FoodDataDic = new Dictionary<string, string>();
    private Dictionary<string, float> FoodScoreDic = new Dictionary<string, float>();
    List<string> FoodNum, FoodName, KmpValue;
    [SerializeField]
    private GameObject ContentPrefab;
    [SerializeField]
    private Transform SearchingContent;
    GameObject clone;
    [SerializeField]
    GameObject content;
    [SerializeField]
    GameObject RelatedSearch;
    private Transform[] childList = null;
    private List<int> KmpNumValue;
  
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;

        cardMoveFunc = cardMoveObj.GetComponent<CardMoveFunc>();
        SetSpawnTransform();
        spawnedPrefabs = new GameObject[maxPrefabCount];
        CardSpawnPrefabs = new GameObject[maxPrefabCount];
        TmpDropdown.onValueChanged.AddListener(OnValueChanged);

        GetFoodName();
        SearchingInput.onValueChanged.AddListener(KMPReturn);
        foreach(GameObject card in CardSpawnPrefabs)
        {
           if(card != null)
            {
                card.AddComponent<Card>();
            }
        }

        RefreshBtn.onClick.AddListener(RefreshBtnFunc);
        RandomPickBtn.onClick.AddListener(RandomPickFunc);
        ExitBtn.onClick.AddListener(ExitBtnFunc);

    }

    private void SetSpawnTransform()
    {
        float posX,posY,posZ;
        posX = -450;
        posY = 200;
        posZ = 0;
        for(int i = 0; i < 3; i++ )
        {
            posX = -450;
            posY = 200;
            posY = posY - (200 * i);
            Vector3 position = new Vector3(posX,posY,posZ);
            spawnVec3.Add(position);
            posX = 0;
            position = new Vector3(posX, posY,posZ);
            spawnVec3.Add(position);
            posX = 450;
            position = new Vector3(posX, posY, posZ);
            spawnVec3.Add(position);
        }

        float CardPosX, CardPosY, CardPosZ;
        
        for (int i = 0; i < 5; i++)
        {
            CardPosX = -700;
            CardPosY = 300;
            CardPosZ = 0;
            CardPosX = CardPosX + (370 * i);
            Vector3 position = new Vector3(CardPosX, CardPosY, CardPosZ);
            CardVec3.Add(position);
        }

        for(int i = 0; i < 4; i++)
        {
            CardPosX = -630;
            CardPosY = -200;
            CardPosZ = 0;
            CardPosX = CardPosX + (400 * i);
            if (i == 2)
            {
                CardPosX = 230;
            }
            if(i == 3)
            {
                CardPosX = 630;
            }
            Vector3 position = new Vector3(CardPosX,CardPosY,CardPosZ);
            CardVec3.Add(position);
        }
        

    }
    private void RefreshBtnFunc()
    {
        int count = TmpDropdown.value+1;

        for(int i = 0; i < spawnedPrefabs.Length; i++) 
        {
            Destroy(spawnedPrefabs[i]);
            Destroy(CardSpawnPrefabs[i]);
        }

        for(int i = 0; i < count; i++)
        {
            spawnedPrefabs[i] = Instantiate(FoodTrayPreFab, spawnVec3[i], Quaternion.Euler(0, 0, 0), OptionPanel.transform);
            spawnedPrefabs[i].transform.localPosition = spawnVec3[i];

            GameObject prefab = CardPreFab[Random.Range(0, CardPreFab.Length)];
            CardSpawnPrefabs[i] = Instantiate(prefab, CardVec3[i], Quaternion.Euler(0, 0, 0), CardPanel.transform);
            CardSpawnPrefabs[i].transform.localPosition = CardVec3[i];
        }

        cardMoveFunc.StopAllCoroutines();
        cardMoveFunc.CancelInvoke("DestroyCardObj");
        cardMoveFunc.cardList = new List<Transform>();

        currentPrefabCount = count;

    }
    private void OnValueChanged(int index)
    {
        for(int i = 0; i < spawnedPrefabs.Length; i++)
        {
            if (spawnedPrefabs[i] != null)
                Destroy(spawnedPrefabs[i]);
            if (CardSpawnPrefabs[i] != null)
                Destroy(CardSpawnPrefabs[i]);
        }

        int count = index+1;

        if (count == currentPrefabCount)
            return;

        for(int i = count; i < currentPrefabCount; i++ )
        {   
            
            if (spawnedPrefabs[i] != null)
                Destroy(spawnedPrefabs[i]);
            if (CardSpawnPrefabs[i] != null)
                Destroy(CardSpawnPrefabs[i]);
           
        }

        for(int i = 0; i < count ; i++ ) 
        {
            spawnedPrefabs[i] = Instantiate(FoodTrayPreFab, spawnVec3[i], Quaternion.Euler(0, 0, 0), OptionPanel.transform);
            spawnedPrefabs[i].transform.localPosition = spawnVec3[i];

            GameObject prefab = CardPreFab[Random.Range(0,CardPreFab.Length)];
            CardSpawnPrefabs[i] = Instantiate(prefab, CardVec3[i],Quaternion.Euler(0,0,0),CardPanel.transform);
            CardSpawnPrefabs[i].transform.localPosition = CardVec3[i];
        }
        cardMoveFunc.StopAllCoroutines();
        cardMoveFunc.CancelInvoke("DestroyCardObj");
        cardMoveFunc.cardList = new List<Transform>();

        currentPrefabCount = count;
    }
    private void KMPReturn(string Searching)
    {
        ScrollView.SetActive(true);
        childList = content.GetComponentsInChildren<Transform>();

        if (childList.Length > 0)
        {
            RelatedSearch.SetActive(true);
            for (int z = 1; z < childList.Length; z++)
            {
                if (childList[z] != transform)
                {
                    Destroy(childList[z].gameObject);
                }
            }
        }
        Debug.Log("검색: " + Searching);

        KmpValue = new List<string>();
        KmpNumValue = new List<int>();
        
        char[] CompareKmp;
        char[] pat = Searching.ToCharArray();
        
        string kmpinput = new string(name);

        for(int x = 0; x < FoodName.Count; x++) 
        {
            CompareKmp = FoodName[x].ToCharArray();
            if (!string.IsNullOrEmpty(Searching))
            {
                kmpinput = KMPAlgo(CompareKmp, pat);
                
            }
            else
            {
                break;
            }

            if (!string.IsNullOrEmpty(kmpinput))
            {

                KmpValue.Add(kmpinput);
                KmpNumValue.Add(int.Parse(FoodNum[x]));
            }
        }

       
        int i = 0;
        foreach (string kmp in KmpValue)
        {
            SpawnSearchData(kmp, i);
            i++;
        }

        if (KmpValue.Count == 0)
        {
            RelatedSearch.SetActive(false);
        }


    }
    private string KMPAlgo(char[] name, char[] pat)
    {
        int n = name.Length;
        int m = pat.Length;
        string kmpvalue = "";

        int[] lps = new int[m];

        LPSArray(pat, m, lps);

        int i = 0;
        int j = 0;

        while (i < n)
        {
            if (pat[j] == name[i])
            {
                i++;
                j++;
                if (j == m)
                {
                    kmpvalue = new string(name);
                    j = lps[j - 1];
                }
            }
            else
            {
                if (j != 0)
                    j = lps[j - 1];
                else
                    i++;
            }

        }
        return kmpvalue;

    }
    private void LPSArray(char[] pat, int m, int[] lps)
    {
        int len = 0;
        int i = 1;
        lps[0] = 0;

        while (i < m)
        {
            if (pat[i] == pat[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else
            {
                if (len != 0)
                    len = lps[len - 1];
                else
                {
                    lps[i] = 0;
                    i++;
                }

            }
        }
    }
    private void GetFoodName()
    {
        string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + System.Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD") + ";";
        MySqlConnection conn = new MySqlConnection(connStr);
        conn.Open();
        MySqlCommand cmd;
        FoodName = new List<string>();
        FoodNum = new List<string>();

        string SelectFoodName = "select foodnum,FoodName from Fooddata";
        int i = 0;
        try
        {
            cmd = new MySqlCommand(SelectFoodName, conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string num = reader.GetString(0);
                string name = reader.GetString(1);
                FoodNum.Add(num);
                FoodName.Add(name);
                FoodDataDic.Add(num, name);
                i++;
            }
            reader.Close();

            cmd = new MySqlCommand("select fooddata.foodname, ROUND(IFNULL(AVG(foodtastescore.foodscore), 0), 1) as avg_score from fooddata left join foodtastescore on fooddata.foodname = foodtastescore.foodname group by fooddata.foodname", conn);
            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                string name = reader.GetString(0);
                float score = reader.GetFloat(1);
                FoodScoreDic.Add(name, score);
            }
            reader.Close();
        }
        catch (MySql.Data.MySqlClient.MySqlException e)
        {
            Debug.Log(e.Message);
        }
        conn.Close();
    }
    public void SpawnSearchData(string SpawnFoodName, int i)
    {
        int yvalue = 50;
        clone = Instantiate(ContentPrefab, new Vector3(0, (yvalue * i), 0), Quaternion.identity);
        clone.GetComponentInChildren<TextMeshProUGUI>().text = SpawnFoodName;
        clone.transform.SetParent(SearchingContent);
        clone.transform.localScale = Vector3.one;
        SetPrefabFunc(clone);
    }

    private void SetPrefabFunc(GameObject Prefab)
    {
        UnityEngine.UI.Button PrefabButton = Prefab.GetComponentInChildren<UnityEngine.UI.Button>();

        PrefabButton.onClick.AddListener(() =>
        {
            SearchingInput.onValueChanged.RemoveAllListeners();
            SearchingInput.text = Prefab.GetComponentInChildren<TextMeshProUGUI>().text;
            for (int i = 0; i <= TmpDropdown.value; i++)
            {
                if (spawnedPrefabs[i] == null)
                {
                    SearchingInput.text = null;
                    break;
                }
                TextMeshProUGUI TrayText = spawnedPrefabs[i].GetComponentInChildren<TextMeshProUGUI>();
                TextMeshProUGUI[] cardtexts = CardSpawnPrefabs[i].GetComponentsInChildren<TextMeshProUGUI>();
                Image[] CardPrefabsImage = CardSpawnPrefabs[i].GetComponentsInChildren<Image>();
                int getFoodNum;
                if(TrayText.text.Equals("음식 이름"))
                {
                    
                    Debug.Log("트레이 이름이 비었음");
                    spawnedPrefabs[i].GetComponentInChildren<TextMeshProUGUI>().text = SearchingInput.text;
                    //CardSpawnPrefabs[i].GetComponentInChildren<TextMeshProUGUI>().text = SearchingInput.text;
                    cardtexts[0].text = SearchingInput.text;
                    float score = 0.0f;
                    if (FoodScoreDic.TryGetValue(SearchingInput.text, out score))
                    {
                        cardtexts[1].text = "평균 점수: " + score.ToString() + "점";
                    }

                    for (int x = 0; x < KmpValue.Count; x++)
                    {
                        if (TrayText.text.Equals(KmpValue[x]))
                        {
                            getFoodNum = KmpNumValue[x];

                            CardPrefabsImage[1].sprite = SetCardImgSprite(getFoodNum);
                            // 아래에 음식의 평균 점수 까지 고려해 추가하는 기능 요망
                        }
                    }
                    SearchingInput.text = null;
                    break;
                }
                
            }
            RelatedSearch.SetActive(false);
            ScrollView.SetActive(false);
            SearchingInput.onValueChanged.AddListener(KMPReturn);
        });
    }
    
    private Sprite SetCardImgSprite(int GetFoodnum)
    {
        string Imagepath = "";
        string[] imgExtensions = { ".jpg", ".png" };
        Sprite retSprtie = null;
        foreach(string ext in imgExtensions) 
        {
            Imagepath = "Assets/Images/FoodImg/" + GetFoodnum + ext;
            if (System.IO.File.Exists(Imagepath))
                break;
        }
        Texture2D tex = null;
        byte[] fileData;
        Debug.Log("이미지path " + Imagepath);
        if(System.IO.File.Exists(Imagepath))
        {
            fileData = System.IO.File.ReadAllBytes(Imagepath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        retSprtie = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),Vector2.zero);


        return retSprtie;
    }

    private void RandomPickFunc()
    {
        int ItemCount = currentPrefabCount;
        int StartIndex = 0;

        if (currentPrefabCount > 0)
        {
            if (spawnedPrefabs[currentPrefabCount - 1].GetComponentInChildren<TextMeshProUGUI>().text != "음식 이름")
            {
                StartIndex = 0;
            }
            for (int i = 0; i < currentPrefabCount; i++)
            {
                if (spawnedPrefabs[i].GetComponentInChildren<TextMeshProUGUI>().text == "음식 이름")
                {
                    StartIndex = i;
                    break;
                }
            }
        }
        ItemCount = ItemCount - StartIndex;
        Debug.Log("itemcount " + ItemCount);
        Debug.Log("프리팹갯수: " + spawnedPrefabs.Length);
        int spawnedPreCount = 0;
        foreach(GameObject g in CardSpawnPrefabs)
        {
            if(g != null)
            {
                spawnedPreCount = spawnedPreCount + 1;
            }
        }
        Debug.Log("spawnprecount 갯수 "+spawnedPreCount);

        if (currentPrefabCount > 0 &&  spawnedPreCount > 1)
        {
            List<KeyValuePair<string, string>> GetRandItemsList = GetRandomItems(FoodDataDic, ItemCount);

            foreach (KeyValuePair<string, string> item in GetRandItemsList)
            {
                string key = item.Key;
                string value = item.Value;
                Debug.Log("랜덤 키-값: " + key + value);
                TextMeshProUGUI TrayText = spawnedPrefabs[StartIndex].GetComponentInChildren<TextMeshProUGUI>();
                TextMeshProUGUI[] cardtexts = CardSpawnPrefabs[StartIndex].GetComponentsInChildren<TextMeshProUGUI>();

                //TextMeshProUGUI CardText = CardSpawnPrefabs[StartIndex].GetComponentInChildren<TextMeshProUGUI>();
                Image[] CardPrefabsImage = CardSpawnPrefabs[StartIndex].GetComponentsInChildren<Image>();
                spawnedPrefabs[StartIndex].GetComponentInChildren<TextMeshProUGUI>().text = value;
                //CardSpawnPrefabs[StartIndex].GetComponentInChildren<TextMeshProUGUI>().text = value;
                cardtexts[0].text = value;
                float score = 0.0f;
                if(FoodScoreDic.TryGetValue(value, out score)) 
                {
                    cardtexts[1].text = "평균 점수: "+ score.ToString() +"점"; 
                }
                
                CardPrefabsImage[1].sprite = SetCardImgSprite(int.Parse(key));

                StartIndex = StartIndex + 1;
            }
        }
    }

    private List<KeyValuePair<string,string>> GetRandomItems(Dictionary<string,string> dict,int itemcount)
    {
        List<string> keys = new List<string>(dict.Keys);
        
        List<KeyValuePair<string,string>> randomItems = new List<KeyValuePair<string,string>>();

        for(int i = 0; i < itemcount; i++) 
        { 
            int randomindex = UnityEngine.Random.Range(0,keys.Count);

            string randomkey = keys[randomindex];
            string randomvalue = dict[randomkey];

            randomItems.Add(new KeyValuePair<string,string>(randomkey, randomvalue));

            keys.RemoveAt(randomindex);
        }

        return randomItems;
    }

    private void ExitBtnFunc()
    {
        SceneManager.LoadScene("MainFuncScene");
    }
}