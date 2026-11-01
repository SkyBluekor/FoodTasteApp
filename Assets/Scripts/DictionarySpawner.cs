using System.Collections.Generic;
using System.Collections;
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
    private Coroutine scrollReset;

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

        // Defer scroll reset until the dynamic row layout has been rebuilt.
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

        // After changing folders, show the first row rather than retaining a
        // stale scroll offset from the previously displayed directory.
        if (scrollReset != null)
            StopCoroutine(scrollReset);
        scrollReset = StartCoroutine(ResetScrollAfterLayout());
    }

    private IEnumerator ResetScrollAfterLayout()
    {
        // Destroy(old rows), dynamic GridLayoutGroup and ContentSizeFitter
        // are processed in the following UI frame, not at SpawnData time.
        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = parentContent as RectTransform;
        ScrollRect scrollRect = parentContent.GetComponentInParent<ScrollRect>();
        if (contentRect != null && scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            Canvas.ForceUpdateCanvases();
            scrollRect.StopMovement();

            // This Content has a top pivot (0, 1). y=0 is its first row.
            var position = contentRect.anchoredPosition;
            position.y = 0f;
            contentRect.anchoredPosition = position;
            scrollRect.verticalNormalizedPosition = 1f;
        }
        else if (verticalScrollbar != null)
        {
            verticalScrollbar.value = 1f;
        }

        scrollReset = null;
    }

    public void SpawnData(string fileName, DataType type)
    {
        // Keep the row on the canvas plane. Preserving world coordinates here
        // can put it in front of the camera-space canvas and outside the mask.
        GameObject clone = Instantiate(panelDataPrefab, parentContent, false);

        Data data = clone.GetComponent<Data>();
        data.Setup(directoryController, fileName, type);

        // The row itself needs a raycastable graphic: its text does not receive
        // raycasts, so otherwise only the small icon responds to clicks.
        Image hitArea = clone.GetComponent<Image>();
        if (hitArea == null)
            hitArea = clone.AddComponent<Image>();
        hitArea.color = Color.clear;
        hitArea.raycastTarget = true;
        fileList.Add(data);
    }

}
