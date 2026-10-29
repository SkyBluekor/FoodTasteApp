using UnityEngine;
using System;
using System.IO;

public class DirectoryController: MonoBehaviour
{
    private DirectoryInfo defaultDirectory; // 기본폴더
    private DirectoryInfo currentDirectory;
    private DictionarySpawner dictionarySpawner;
    [SerializeField]
    private ImageLoader imageLoader;
    private bool functionOn = false;

    private void Awake()
    {
        Application.runInBackground = true;

        dictionarySpawner = GetComponent<DictionarySpawner>();
        dictionarySpawner.Setup(this);

        //최초 경로를 바탕화면으로 설정
        // Environment.GetFolderPath() : 윈도우에 존재하는 폴더경로를 얻어오는 메소드
        // Environment.SpecialFolder : 윈도우에 존재하는 특수 폴더 열기형
        string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        defaultDirectory = new DirectoryInfo(desktopFolder);
        currentDirectory = new DirectoryInfo(desktopFolder);

        UpdateDirectory(currentDirectory);

    }
    private void Update()
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        if(Input.GetMouseButtonDown(0))
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
        if( Input.GetKeyDown(KeyCode.Escape) && functionOn == true)
        {
            UpdateDirectory(defaultDirectory);
        }
        //back스페이스 상위폴더 이동
        if (Input.GetKeyDown(KeyCode.Backspace) && functionOn == true)
        {
            MoveToParentFolder(currentDirectory);
        }
    }

    private void UpdateDirectory(DirectoryInfo directory)
    {
        currentDirectory = directory;

        Debug.Log($"현재 폴더명 :  {currentDirectory.Name}");

        dictionarySpawner.UpdateDictionary(currentDirectory);
      
        // 현재 폴더에 존재하는 모든 폴더이름 출력
        /*
        foreach( DirectoryInfo dir in currentDirectory.GetDirectories())
        {
            Debug.Log(dir.Name);
        }

        //현재 폴더에 존재하는 모든 파일 이름 출력
        foreach( FileInfo file in currentDirectory.GetFiles())
        {
            Debug.Log(file.Name);

        }
        */
    }
  
    private void MoveToParentFolder(DirectoryInfo directory)
    {
        if (directory.Parent == null) return;

        UpdateDirectory(directory.Parent);
    }

    public void UpdateInputs(string data)
    {
        // ...이면 상위폴더 이동
        if( data.Equals("..."))
        {
            MoveToParentFolder(currentDirectory);
            return;
        }

        // 폴더면 내부 이동
        foreach(DirectoryInfo directory in currentDirectory.GetDirectories())
        {
            if(data.Equals(directory.Name))
            {
                UpdateDirectory(directory);
                return;
            }
        }

        //파일이면 확장자에따라 처리
        foreach(FileInfo file in currentDirectory.GetFiles())
        {
            if(data.Equals(file.Name))
            {
                Debug.Log($"선택한 파일의 이름: { file.FullName}");
                imageLoader.LoadFile(file);
            }
        }
    }

}
