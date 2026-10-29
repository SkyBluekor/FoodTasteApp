using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class RegionDropdwon : MonoBehaviour
{
    public TMPro.TMP_Dropdown DoRegionDrop, CityGuRegionDrop;
    string[] DoStr;
    // Start is called before the first frame update
    void Start()
    {
        
        string filePath = Path.Combine(Application.dataPath, "Text/DoRegion.txt");
        string str = ReadTxt(filePath);
        DoStr = str.Split("\n");
        Debug.Log(DoStr.Length);
        int i = 0;
        foreach(string word in DoStr)
        {

            TMPro.TMP_Dropdown.OptionData option = new TMPro.TMP_Dropdown.OptionData();
            option.text = word;
            DoRegionDrop.options.Add(option);
        }
        DoRegionDrop.onValueChanged.AddListener(delegate {Function_Dropdown(DoRegionDrop);});
        Debug.Log(i);
        

    }
 
    
    private void Function_Dropdown(TMPro.TMP_Dropdown DoRegion)
    {
        int menuindex = DoRegion.GetComponent<TMPro.TMP_Dropdown>().value;
        List<TMPro.TMP_Dropdown.OptionData> menuoption = DoRegion.GetComponent<TMPro.TMP_Dropdown>().options;
        string value = menuoption[menuindex].text;
        string pick = DoRegion.options[DoRegion.value].text;
        
        Debug.Log("Pick:" + pick+ "value: " +value);
        Debug.Log("서울특별시\n".Length);
        string[] SiguStr;
        string sigufilePath = null;
        Debug.Log(string.Equals(pick, DoStr[0]));
        
        switch (Array.IndexOf(DoStr,pick))
        {
            case 0:
                Debug.Log("서울");
                sigufilePath = Path.Combine(Application.dataPath, "Text/SeoulRegion.txt");
                break;
            case 1:
                Debug.Log("부산");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Busan.txt");
                break;
            case 2:
                Debug.Log("대구");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Daegu.txt");
                break;
            case 3:
                Debug.Log("인천");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Incheon.txt");
                break;
            case 4:
                Debug.Log("광주");
                sigufilePath = Path.Combine(Application.dataPath, "Text/GwangJu.txt");
                break;
            case 5:
                Debug.Log("대전");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Dajun.txt");
                break;
            case 6:
                Debug.Log("울산");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Ulsan.txt");
                break;
            case 7:
                Debug.Log("세종");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Sejong.txt");
                break;
            case 8:
                Debug.Log("경기도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/gyeonggi-do.txt");
                break;
            case 9:
                Debug.Log("강원도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Gangwon-do.txt");
                break;
            case 10:
                Debug.Log("충청북도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Chung-cheong bukdo.txt");
                break;
            case 11:
                Debug.Log("충청남도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Chungcheongnam-do.txt");
                break;
            case 12:
                Debug.Log("전라북도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Jeollabuk-do.txt");
                break;
            case 13:
                Debug.Log("전라남도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Jeollanam-do.txt");
                break;
            case 14:
                Debug.Log("경상북도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Gyeongsangbuk-do.txt");
                break;
            case 15:
                Debug.Log("경상남도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Gyeongsangnam-do.txt");
                break;
            case 16:
                Debug.Log("제주도");
                sigufilePath = Path.Combine(Application.dataPath, "Text/Jeju.txt");
                break;
        }
        Debug.Log("sigufilepath: " + sigufilePath);
        if(pick.Equals("도 선택"))
        {
            Debug.Log("도 선택");
            CityGuRegionDrop.ClearOptions();
        }
        if (sigufilePath != null)
        {
            string SiguPathStr = ReadTxt(sigufilePath);
            SiguStr = SiguPathStr.Split("\n");
            CityGuRegionDrop.ClearOptions();
            foreach(string word in SiguStr)
            {
                TMPro.TMP_Dropdown.OptionData option = new TMPro.TMP_Dropdown.OptionData();
                option.text = word;
                CityGuRegionDrop.options.Add(option);
            }
            CityGuRegionDrop.value = 0;
            CityGuRegionDrop.Select();
            CityGuRegionDrop.RefreshShownValue();
        }
    }

    string ReadTxt(string filepath)
    {
        FileInfo fileinfo = new FileInfo(filepath);
        string value = "";

        if (fileinfo.Exists)
        {
            StreamReader reader = new StreamReader(filepath);
            value = reader.ReadToEnd();
            reader.Close();
        }
        else
            value = "파일이 없습니다.";
        return value;
    }

}
