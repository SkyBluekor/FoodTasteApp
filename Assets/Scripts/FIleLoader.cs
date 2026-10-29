using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class FIleLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject ImagePanel;

    [SerializeField]
    private TextMeshProUGUI textFileName;
    [SerializeField]
    private TextMeshProUGUI textFileSize;
    [SerializeField]
    private TextMeshProUGUI textCreationTime;
    [SerializeField]
    private TextMeshProUGUI textLastWriteTime;
    [SerializeField]
    private TextMeshProUGUI textDirectory;
    [SerializeField]
    private TextMeshProUGUI textFullName;

    private FileInfo fileInfo;

    public void OnLoad(FileInfo file)
    {
        ImagePanel.SetActive(true);

        fileInfo = file;

        textFileName.text = $"파일 이름 : {fileInfo.Name}";
        textFileSize.text = $"파일 크기 : {fileInfo.Length} Bytes";
        textDirectory.text = $"파일 경로 : {fileInfo.Directory}";
    }

    public void OpenFile()
    {
        Application.OpenURL("file:///" + fileInfo.FullName);
    }

    public void OffLoad()
    {
        ImagePanel.SetActive(false);
    }

}
