using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ForSearching : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    TMP_InputField SearchingInput;
    MySqlConnection conn;
    List<string> FoodNum;
    List<string> FoodName;
    List<string> KmpValue;
    [SerializeField]
    private GameObject RelatedPrefab;
    [SerializeField]
    private Transform SearchingContent;
    [SerializeField]
    private int maxSearchResults = 10; // 최대 검색 결과 개수
    private List<GameObject> searchResultObjects; // 검색 결과 객체들
    private Queue<GameObject> objectPool; // 객체 풀
    private bool isSearching; // 검색 중인지 여부
    public GameObject RelatedSearch;
    // Start is called before the first frame update
    void Start()
    {
        InitializeDatabase();
        InitializeObjectPool();
        SearchingInput.onValueChanged.AddListener(KMPReturn);
    }

    // Update is called once per frame
    void Update()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
        
    }

    private void KMPReturn(string Searching)
    {
        if (isSearching)
            return;

        if (!string.IsNullOrEmpty(Searching))
        {
            StartCoroutine(SearchAndDisplayResults(Searching));
        }
        else
        {
            ClearSearchResults();
            RelatedSearch.SetActive(false);
        }
    }

    private IEnumerator SearchAndDisplayResults(string searchString)
    {
        isSearching = true;

        ClearSearchResults();

        KmpValue = new List<string>();

        char[] pat = searchString.ToCharArray();

        foreach (string name in FoodName)
        {
            char[] compareKmp = name.ToCharArray();

            string kmpInput = KMPAlgo(compareKmp, pat);

            if (!string.IsNullOrEmpty(kmpInput))
            {
                KmpValue.Add(kmpInput);
            }

            if (KmpValue.Count >= maxSearchResults)
                break;
        }

        yield return null; // 한 프레임 대기하여 검색 결과가 화면에 표시되도록 함

        DisplaySearchResults();

        isSearching = false;
    }

    private string KMPAlgo(char[] name, char[] pat)
    {
        int n = name.Length;
        int m = pat.Length;
        string kmpValue = "";

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
                    kmpValue = new string(name);
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
        return kmpValue;
    }

    void LPSArray(char[] pat, int m, int[] lps)
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

    private void InitializeDatabase()
    {
        string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + System.Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD") + ";";
        conn = new MySqlConnection(connStr);
        conn.Open();

        MySqlCommand cmd;
        FoodName = new List<string>();
        FoodNum = new List<string>();

        string selectFoodName = "SELECT foodnum, FoodName FROM Fooddata";

        try
        {
            cmd = new MySqlCommand(selectFoodName, conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                FoodNum.Add(reader.GetString(0));
                FoodName.Add(reader.GetString(1));
            }

            reader.Close();
        }
        catch (MySqlException e)
        {
            Debug.Log(e.Message);
        }

        conn.Close();
    }

    private void InitializeObjectPool()
    {
        searchResultObjects = new List<GameObject>();
        objectPool = new Queue<GameObject>();
    }

    private void ClearSearchResults()
    {
        foreach (var obj in searchResultObjects)
        {
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        searchResultObjects.Clear();
    }

    private void DisplaySearchResults()
    {
        int count = Mathf.Min(KmpValue.Count, maxSearchResults);

        for (int i = 0; i < count; i++)
        {
            string kmp = KmpValue[i];
            GameObject resultObject = GetObjectFromPool();
            resultObject.GetComponentInChildren<TextMeshProUGUI>().text = kmp;
            resultObject.transform.SetParent(SearchingContent, false);
            resultObject.SetActive(true);
            RelatedSearch.SetActive(true);
            searchResultObjects.Add(resultObject);
        }
    }

    private GameObject GetObjectFromPool()
    {
        GameObject obj;

        if (objectPool.Count > 0)
        {
            obj = objectPool.Dequeue();
        }
        else
        {
            obj = Instantiate(RelatedPrefab);
            obj.transform.SetParent(SearchingContent, false);
        }

        return obj;
    }
}

