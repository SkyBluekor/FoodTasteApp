using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class CSVManager : MonoBehaviour
{
    private List<string[]> rowData = new List<string[]>();

    // Start is called before the first frame update
    void Start()
    {
        Write();
    }

    void Write()
    {
        string[] rowDataTemp = new string[1221];

        rowDataTemp[0] = "ID";

        int j = 1;
        for(int i = 1; i<rowDataTemp.Length-1; i++)
        {

            string num = i.ToString();
            rowDataTemp[i] = num;
            
            

        }
        rowData.Add(rowDataTemp);

        
        string[][] output = new string[rowData.Count][];

        for(int i = 0; i<output.Length;i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ","; 

        StringBuilder sb = new StringBuilder();

        for(int index = 0; index < length;index++)
        {
            sb.AppendLine(string.Join(delimiter, output[index]));
        }

        string filePath = getPath();

        StreamWriter outstream = System.IO.File.CreateText(filePath);
        
        outstream.WriteLine(sb);
        outstream.Close();
    }

    private string getPath()
    {
#if UNITY_EDITOR
        return Application.dataPath+ "/Resources" + "/CSV/"+"FoodScoreData.csv";
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
