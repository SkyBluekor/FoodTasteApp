using MySql.Data.MySqlClient;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SearchingFunc : MonoBehaviour
{
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
    GameObject clone;
    [SerializeField]
    GameObject content;
    [SerializeField]
    GameObject RelatedSearch;
    private string previousInput;

    // Start is called before the first frame update
    void Start()
    {
        GetFoodName();
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
        Transform[] childLIst = content.GetComponentsInChildren<Transform>();
        if(childLIst.Length > 0)
        {
            RelatedSearch.SetActive(true);
            for( int z =1 ; z <childLIst.Length; z++ )
            {
                if (childLIst[z] != transform)
                {
                    Destroy(childLIst[z].gameObject);
                }
            }
        }
        Debug.Log("°Ë»ö: " +Searching);
      
        KmpValue = new List<string>();

        char[] CompareKmp;
        char[] pat = Searching.ToCharArray();
        Debug.Log(pat.Length);
        string kmpinput = "";


        foreach(string name in FoodName)
        {
            CompareKmp = name.ToCharArray();
            
            if(!string.IsNullOrEmpty(Searching))    
            {
                Debug.Log(CompareKmp.Length);
                kmpinput = KMPAlgo(CompareKmp, pat);
            }
            else
            {
                break;
            }
           
            if (!string.IsNullOrEmpty(kmpinput))
            {
                
                KmpValue.Add(kmpinput);
            }
        }

        
        Debug.Log(KmpValue.Count);

        int i = 0;
        foreach (string kmp in KmpValue)
        {
            SpawnSearchData(kmp,i);
            i++;
        }
        
        if(KmpValue.Count == 0)
        {
            RelatedSearch.SetActive(false);
        }



    }

    private string KMPAlgo(char[] name, char[] pat)
    {
        Debug.Log(name);
        int n = name.Length;
        int m = pat.Length;
        string kmpvalue = "";

        int[] lps = new int[m];

        LPSArray(pat,m,lps);

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
                    Debug.Log("foodname : " + new string(name));
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

    void LPSArray(char[] pat, int m, int[] lps)
    {
        

        int len = 0;
        int i = 1;
        lps[0] = 0;

        while (i < m)
        {
            Debug.Log(lps.Length);
            Debug.Log(i);
            if (pat[i] == pat[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else
            {
                if(len != 0)
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
        string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";";
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
            
            while(reader.Read())
            {
                FoodNum.Add(reader.GetString(0));
                FoodName.Add(reader.GetString(1));
                i++;
            }
            reader.Close();
        }
        catch(MySql.Data.MySqlClient.MySqlException e)
        {
            Debug.Log(e.Message);
        }
        conn.Close();
    }

    public void SpawnSearchData(string FoodName , int i)
    {
        Debug.Log("i= " + i);
        int yvalue = 50;

        clone = Instantiate(RelatedPrefab,new Vector3(0,(yvalue*i),0),Quaternion.identity);
        clone.GetComponentInChildren<TextMeshProUGUI>().text = FoodName;
        Debug.Log(FoodName);
        clone.transform.SetParent(SearchingContent);
        clone.transform.localScale = Vector3.one;
    }

}
