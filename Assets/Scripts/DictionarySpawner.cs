using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DictionarySpawner : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textDirectoryName; //현재 경로 이름을 나타내는 TextUi
    [SerializeField]
    private Scrollbar verticalScrollbar;

    [SerializeField]
    private GameObject panelDataPrefab;
    [SerializeField]
    private Transform parentContent;

    private DirectoryController directoryController;
    private List<Data> fileList;

    public void Setup(DirectoryController controller)
    {
        directoryController = controller;

        fileList = new List<Data>();
    }

    public void UpdateDictionary(DirectoryInfo currentDirectory)
    {
        for(int i = 0; i< fileList.Count; ++i)
        {
            Destroy(fileList[i].gameObject);
        }
        fileList.Clear();

        textDirectoryName.text = currentDirectory.Name;

        verticalScrollbar.value = 1;
        SpawnData("...", DataType.Directory);
        foreach(DirectoryInfo directory in currentDirectory.GetDirectories())      
        {
            SpawnData(directory.Name, DataType.Directory);
        }

        // 현재 폴더에 존재하는 모든 파일 Text UI 생성[Files]
        foreach(FileInfo file in currentDirectory.GetFiles())
        {
            SpawnData(file.Name, DataType.File);
        }
        fileList.Sort((a, b) => a.Filename.CompareTo(b.Filename));

        for(int i = 0; i < fileList.Count; ++i)
        {
            fileList[i].transform.SetSiblingIndex(i);
            if (fileList[i].Filename.Equals("..."))
            {
                fileList[i].transform.SetAsFirstSibling();
            }
        }
    }

    public void SpawnData(string fileName, DataType type)
    {
        GameObject clone = Instantiate(panelDataPrefab);

        clone.transform.SetParent(parentContent);
        clone.transform.localScale = Vector3.one;

        Data data = clone.GetComponent<Data>();
        data.Setup(directoryController, fileName, type);
        fileList.Add(data);
    }

}
