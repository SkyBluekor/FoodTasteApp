using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DictionarySpawnerinSetting : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textDicName;
    [SerializeField]
    private Scrollbar VerScrollbar;
    [SerializeField]
    private GameObject panelDataPrefab;
    [SerializeField]
    private Transform parentContent;

    private DCinSetting DirController;
    private List<DataInSetting> fileList;

    public void SetupInsetting(DCinSetting controller)
    {
        DirController = controller;

        fileList = new List<DataInSetting>();

    }

    public void UpdateDictionaryinSetting(DirectoryInfo CurrentDIc)
    {
        for (int i = 0; i < fileList.Count; ++i)
        {
            Destroy(fileList[i].gameObject);
        }
        fileList.Clear();

        textDicName.text = CurrentDIc.Name;

        VerScrollbar.value = 1;
        SpawnDatainSetting("...", DataType.Directory);
        foreach (DirectoryInfo directory in CurrentDIc.GetDirectories())
        {
            SpawnDatainSetting(directory.Name, DataType.Directory);
        }

        // 현재 폴더에 존재하는 모든 파일 Text UI 생성[Files]
        foreach (FileInfo file in CurrentDIc.GetFiles())
        {
            SpawnDatainSetting(file.Name, DataType.File);
        }
        fileList.Sort((a, b) => a.FilenameinSetting.CompareTo(b.FilenameinSetting));

        for (int i = 0; i < fileList.Count; ++i)
        {
            fileList[i].transform.SetSiblingIndex(i);
            if (fileList[i].FilenameinSetting.Equals("..."))
            {
                fileList[i].transform.SetAsFirstSibling();
            }
        }
    }

    public void SpawnDatainSetting(string filename, DataType Type) 
    {
        GameObject clone = Instantiate(panelDataPrefab);
        clone.transform.SetParent(parentContent);
        clone.transform.localPosition = Vector3.one;

        DataInSetting data = clone.GetComponent<DataInSetting>();
        data.SettingSetup(DirController,filename,Type);
        fileList.Add(data);
    }

}
