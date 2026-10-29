using MySql.Data.MySqlClient;
using System.Xml;
using UnityEngine;

public class LoadApi : MonoBehaviour
{
    string xmlFileName = "전국통합식품영양성분정보(음식)표준데이터";
    void Start()
    {
        LoadXML(xmlFileName);
    }
    private void FixedUpdate()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
    }
    private void LoadXML(string _FileName)
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("XML/" + _FileName);
        XmlDocument xmlDoc = new XmlDocument();
        Debug.Log(txtAsset.text);
        xmlDoc.LoadXml(txtAsset.text);

        string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + System.Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD") + ";";
        MySqlConnection conn = new MySqlConnection(connStr);
        int FoodNum;
        string Food;
        string kcal;
        string Foodgram;
        MySqlCommand cmd;

        conn.Open();
        int i = 1;
        XmlNodeList all_nodes = xmlDoc.SelectNodes("dataGrid/records/record");
        foreach (XmlNode node in all_nodes)
        {
            Debug.Log(i);
            FoodNum = i;
            Food = node.SelectSingleNode("식품명").InnerText;
            kcal = node.SelectSingleNode("에너지kcal").InnerText;
            Foodgram = node.SelectSingleNode("식품중량").InnerText;

            //Debug.Log("Food : " + node.SelectSingleNode("식품명").InnerText);
            //Debug.Log("Kcal : " + node.SelectSingleNode("에너지kcal").InnerText);
            //Debug.Log("식품중량 : " + node.SelectSingleNode("식품중량").InnerText);

           /*
            string ModifyFoodTable = "alter table usertastescore add F" + i + " int default 0";
            try
            {
                cmd = new MySqlCommand(ModifyFoodTable, conn);
                cmd.ExecuteNonQuery();
                
            }
            catch(MySql.Data.MySqlClient.MySqlException e)
            {
                Debug.Log(e);
            }
            */
            
            
            string InsertFoodData = "insert into Fooddata(FoodNum,FoodName,FoodKcal,FoodGram) values (" + i + ",'"+Food+"','"+kcal+"','"+ Foodgram +"')";
            try
            {
                
                cmd = new MySqlCommand(InsertFoodData, conn);
                cmd.ExecuteNonQuery();
                i++;
            }
            catch(MySql.Data.MySqlClient.MySqlException e)
            {
                Debug.Log(e.Message);
            }
            

            
        }
        conn.Close();

        /*
        XmlNodeList Food_Table = xmlDoc.GetElementsByTagName("식품명");
        foreach(XmlNode Food in Food_Table)
        {
            Debug.Log("Food : " + Food.InnerText);
        }
        */


    }

    // Update is called once per frame
    void Update()
    {

    }
}

