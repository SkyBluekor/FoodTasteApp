using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DCinSetting : MonoBehaviour
{
    private DirectoryInfo defaultDic;
    private DirectoryInfo CurrentDIc;
    private DictionarySpawnerinSetting DirSpawner;
    [SerializeField]
    private ImgLoaderinSetting ImgLoader;
    private bool functionOn = false;

    private void Awake()
    {
        Application.runInBackground = true;

        DirSpawner = GetComponent<DictionarySpawnerinSetting>();
        DirSpawner.SetupInsetting(this);

        string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        defaultDic = new DirectoryInfo(desktopFolder);
        CurrentDIc = new DirectoryInfo(desktopFolder);

        UpdateDicinSetting(CurrentDIc);

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        if (Input.GetMouseButtonDown(0))
        {

            if (point.x >= -8.8 && point.x <= -3.3 && point.y <= 5.0 && point.y >= -3.8)
                functionOn = true;
            else
            {
                functionOn = false;
            }
            Debug.Log(functionOn);
            Debug.Log(point.ToString());

        }
        //esc 바탕화면이동
        if (Input.GetKeyDown(KeyCode.Escape) && functionOn == true)
        {
            UpdateDicinSetting(defaultDic);
        }
        //back스페이스 상위폴더 이동
        if (Input.GetKeyDown(KeyCode.Backspace) && functionOn == true)
        {
            MoveToParentFolder(CurrentDIc);
        }
    }
    private void UpdateDicinSetting(DirectoryInfo Dir)
    {
        CurrentDIc = Dir;

        DirSpawner.UpdateDictionaryinSetting(CurrentDIc);
        foreach (DirectoryInfo dir in CurrentDIc.GetDirectories())
        {
            Debug.Log("확인: "+ dir.Name);
        }
    }

    private void MoveToParentFolder(DirectoryInfo Dic)
    {
        if (Dic.Parent == null) return;

        UpdateDicinSetting(Dic.Parent);
    }


    // Start is called before the first frame update

    public void UpdateInputsinSetting(string data)
    {
        if (data.Equals("..."))
        {
            MoveToParentFolder(CurrentDIc);
            return;
        }

        // 폴더면 내부 이동
        foreach (DirectoryInfo directory in CurrentDIc.GetDirectories())
        {
            if (data.Equals(directory.Name))
            {
                UpdateDicinSetting(directory);
                return;
            }
        }

        foreach(FileInfo file in CurrentDIc.GetFiles())
        {
            if(data.Equals(file.Name)) 
            {
                ImgLoader.LoadFIleinSetting(file);
            }
        }
    }
}

