using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ExcelDataReader;

public class ExcelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string filePath = @"Assets\Resources\XLSX\FoodScoreData.xlsx";
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();

                for(int i = 0; i < result.Tables.Count; i++)
                {
                    for(int j =0; j < result.Tables[i].Rows.Count; j++)
                    {
                        string data1 = result.Tables[i].Rows[j][0].ToString();
                        string data2 = result.Tables[i].Rows[j][1].ToString();
                        string data3 = result.Tables[i].Rows[j][2].ToString();
                        Debug.Log("data1 : " +data1);
                        Debug.Log("data2 : " + data2);
                        Debug.Log("data3 : " + data3);
                       
                    }
                }
            }
            stream.Close();
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
