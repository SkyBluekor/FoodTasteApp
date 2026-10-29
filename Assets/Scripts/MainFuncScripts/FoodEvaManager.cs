using MySql.Data.MySqlClient;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class FoodEvaManager : MonoBehaviour
{
    public GameObject FoodEvaPanel;
    public GameObject FunctionPanel;
    public GameObject FullFoodPanel;
    [SerializeField]
    private Image FoodImg;
    [SerializeField]
    private Sprite FullStarSprite, EmptyStarSprite;
    [SerializeField]
    private Button Star1,Star2,Star3,Star4,Star5;
    [SerializeField]
    private TMPro.TextMeshProUGUI FoodText, EvaMent;
    [SerializeField]
    private Button ConfirmBtn, PassBtn , ExitBtn;
    private static string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private MySqlConnection conn = new MySqlConnection(connStr);
    private string id;
    private string FoodImgUrl;
    // Start is called before the first frame update
  
    void Start()
    {
        id = PlayerPrefs.GetString("playerid");
        Debug.Log("로그인된 아이디" + id);

        Button FullExBtn = FullFoodPanel.GetComponentInChildren<Button>();
        FullExBtn.onClick.AddListener(() =>
        {
            FoodEvaPanel.SetActive(false);
            FunctionPanel.SetActive(true);
        });
        List<int> UnEvaFoodNum = new List<int>();
        FoodData foodData = new FoodData();
        foodData.FoodName = new List<string>();
        foodData.FoodNum = new List<int>();
        foodData.FoodNumName = new Dictionary<int, string>();
        foodData.FoodEvaPanel = FoodEvaPanel;
        foodData.FunctionPanel = FunctionPanel;
        foodData.FullFoodPanel = FullFoodPanel;
        foodData.SeTFoodData(id);
        Button[] StarBtns = new Button[] { Star1,Star2,Star3,Star4,Star5};
        
        List<int> keys = new List<int>(foodData.FoodNumName.Keys);

        int randomKeyIndex = UnityEngine.Random.Range(0, keys.Count);
        int randomKey = keys[randomKeyIndex];

        Debug.Log(foodData.FoodNum.Count);
        Debug.Log(foodData.FoodName.Count);
        
        foreach(int i in foodData.FoodNum)
        {
            Debug.Log("음식이름: " + i);
        }
       

        FoodImgUrl = Application.dataPath+ "/Images/FoodImg/" + randomKey+".jpg";
        if(File.Exists(FoodImgUrl))
        {
            FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + randomKey + ".jpg";
        }
        else
        {
            FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + randomKey + ".png";
        }

        Debug.Log("음식파일이름: " + FoodImgUrl);

        Texture2D Foodtex = LoadTex(FoodImgUrl);
        if( Foodtex != null)
        {
            Sprite sprite = Sprite.Create(Foodtex, new Rect(0, 0, Foodtex.width, Foodtex.height), new Vector2(0.5f, 0.5f));
            FoodImg.sprite = sprite;
            FoodText.text = foodData.FoodNumName[randomKey];
        }

        foreach(Button b in StarBtns)
        {
            b.onClick.AddListener(ChangeStar);
        }
        //ExitBtn.onClick.AddListener(ActExitBtn);

        PassBtn.onClick.AddListener(() =>
        {
            ActPassBtn(foodData.FoodNumName);
        });

        ConfirmBtn.onClick.AddListener(() =>
        {
            ActConfirmBtn(foodData.FoodNumName);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void ChangeStar()
    {
        GameObject SeletedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Image image = SeletedButton.GetComponent<Image>();
        Button CurrentButton = SeletedButton.GetComponent<Button>();
        string SpriteName = image.sprite.name;
        int FullStarCount = 0;
        Button[] Starbuttons = new Button[] { Star1, Star2 , Star3,Star4,Star5 };
               
        Debug.Log("SpriteName: " + SpriteName);
        if(SpriteName.Equals("EmptyStar"))
        {
            CurrentButton.image.sprite = FullStarSprite;
            Debug.Log("show");
        }
        else if(SpriteName.Equals("FullStar"))
        {
            CurrentButton.image.sprite = EmptyStarSprite;    
        }
        FullStarCount = GetFullStarCount();
       
        Debug.Log("FullStarCount: " + FullStarCount);
        switch(FullStarCount)
        {
            case 0:
                EvaMent.text = "최악이에요.";
                break;
            case 1:
                EvaMent.text = "별로에요.";
                break;
            case 2:
                EvaMent.text = "그저 그랬어요.";
                break;
            case 3:
                EvaMent.text = "그냥 괜찮은 정도?";
                break;
            case 4:
                EvaMent.text = "맛있어요.";
                break;
            case 5:
                EvaMent.text = "삼시세끼 이것만 먹고싶어요.";
                break;
        }
        

    }
    private int GetFullStarCount()
    {
        int FullStarCount = 0;
        Image[] ButtonsImg = new Image[]
        {
            Star1.GetComponent<Image>(),
            Star2.GetComponent<Image>(),
            Star3.GetComponent<Image>(),
            Star4.GetComponent<Image>(),
            Star5.GetComponent<Image>()
        };


        string[] ButtonsSpriteName = new string[]
        {
            ButtonsImg[0].sprite.name,
            ButtonsImg[1].sprite.name,
            ButtonsImg[2].sprite.name,
            ButtonsImg[3].sprite.name,
            ButtonsImg[4].sprite.name
        };

        foreach (string s in ButtonsSpriteName)
        {
            if (s.Equals("FullStar"))
            {
                FullStarCount = FullStarCount + 1;
            }
        }
        return FullStarCount;
    }

    public Texture2D LoadTex(string path)
    {
        Texture2D texture = null;
        byte[] fileData;

        if (File.Exists(path))
        {
            fileData = File.ReadAllBytes(path);
            texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                return texture;
            }
        }
        return null;
    }
    public struct FoodData
    {
        public List<int> FoodNum;
        public List<string> FoodName;
        public Dictionary<int, string> FoodNumName;
        public GameObject FoodEvaPanel;
        public GameObject FunctionPanel;
        public GameObject FullFoodPanel;
        public void SeTFoodData(string id)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            string getFoodnumQuery = "select foodnum,foodname from fooddata where foodnum not in ( select foodnum from foodtastescore where id = '" + id + "')";
            List<int> ReturnFoodnum = new List<int>();
            conn.Open();
            Button FullExitBtn = FullFoodPanel.GetComponentInChildren<Button>();
            

            MySqlCommand cmd;
            MySqlDataReader reader;
        try
        {
            cmd = new MySqlCommand(getFoodnumQuery, conn);
            reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FoodNum.Add(reader.GetInt32(0));
                        FoodName.Add(reader.GetString(1));
                        FoodNumName.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
                else
                {
                    FullFoodPanel.SetActive(true);
                   
                }
            reader.Close();
            cmd.Dispose();
            
        }
        catch(MySql.Data.MySqlClient.MySqlException e)
        {
            Debug.Log(e);
        }
        

       
        conn.Close();
        }
    }

    private void ActConfirmBtn(Dictionary<int, string> FoodNumName)
    {
        string InsertDBQuery = "Insert into foodtastescore(id,FoodNum,FoodScore,foodname) values (@ID,@FoodNum,@FoodScore,@foodname)";
        int FoodNum = 0;
        foreach(KeyValuePair<int,string> pair in FoodNumName)
        {
            if(pair.Value == FoodText.text)
            {
                FoodNum = pair.Key;
            }
        }
        
        try 
        {
            conn.Open();

            MySqlCommand cmd = new MySqlCommand(InsertDBQuery, conn);

            int Score = GetFullStarCount();
            if(Score == 0)
            {
                Score = 1;
            }

            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@FoodNum",FoodNum);
            cmd.Parameters.AddWithValue("@FoodScore", Score);
            cmd.Parameters.AddWithValue("@foodname", FoodText.text);

            cmd.ExecuteNonQuery();

            FoodNumName.Remove(FoodNum);

            conn.Close();
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }

        ActPassBtn(FoodNumName);


    }

    private void ActPassBtn(Dictionary<int,string> FoodNumName)
    {
        List<int> keys = new List<int>(FoodNumName.Keys);

        //UnEvaFoodNum = GetUnEvaFood(id);
        int randomKeyIndex = UnityEngine.Random.Range(0, keys.Count);
        int randomKey = keys[randomKeyIndex];

        FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + randomKey + ".jpg";
        if (File.Exists(FoodImgUrl))
        {
            FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + randomKey + ".jpg";
        }
        else
        {
            FoodImgUrl = Application.dataPath + "/Images/FoodImg/" + randomKey + ".png";
        }

        Debug.Log("음식파일이름: " + FoodImgUrl);

        Texture2D Foodtex = LoadTex(FoodImgUrl);
        if (Foodtex != null)
        {
            Sprite sprite = Sprite.Create(Foodtex, new Rect(0, 0, Foodtex.width, Foodtex.height), new Vector2(0.5f, 0.5f));
            FoodImg.sprite = sprite;
            FoodText.text = FoodNumName[randomKey];
        }

        Image[] ButtonsImg = new Image[]
        {
            Star1.GetComponent<Image>(),
            Star2.GetComponent<Image>(),
            Star3.GetComponent<Image>(),
            Star4.GetComponent<Image>(),
            Star5.GetComponent<Image>()
        };

        foreach(Image i in ButtonsImg)
        {
            i.sprite = EmptyStarSprite;
        }
        EvaMent.text = "음식 평가 멘트";

    }
    /*
    private void ActExitBtn()
    {
        FoodEvaPanel.SetActive(false);
        Debug.Log("재시작");
        FunctionPanel.SetActive(true);
        
    }*/
}
