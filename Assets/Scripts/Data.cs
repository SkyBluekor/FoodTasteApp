using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//파일의 속성{폴더,파일} - 아이콘 출력용
public enum DataType { Directory=0, File}

public class Data : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler , IPointerExitHandler
{
    [SerializeField]
    private Sprite[] spriteIcons; //아이콘에 적용할수 있는 Sprite 이미지

    private Image imageicon;
    private TextMeshProUGUI textDataName;
    private DataType dataType;

    private string fileName;
    public string Filename => fileName; //외부에서 확인하기 위한 get 프로퍼티

    private int maxFileNameLenght = 25; //파일 이름의 최대길이

    private DirectoryController directoryController;
    public void Setup(DirectoryController controller ,string filename, DataType dataType)
    {
        directoryController = controller;

        // 만약 PanelData 오브젝트의 Image 컴포넌트를 삭제하지 않으면 panelData가 Get된다
        imageicon = GetComponentInChildren<Image>();
        textDataName = GetComponentInChildren<TextMeshProUGUI>();

        this.fileName = filename; ;
        this.dataType = dataType;

        imageicon.sprite = spriteIcons[(int)this.dataType];
        textDataName.text = this.fileName;

        if( fileName.Length >= maxFileNameLenght)
        {
            textDataName.text = fileName.Substring(0, maxFileNameLenght);
            textDataName.text += "..";
        }
        SetTextColor();
    }
    // 마우스가 현제 텍스트 Ui 위에 있을때 호출 
    // ipointEnterHandler 인터페이스를 상속 받았을 때 구현하는 메소드
    public void OnPointerEnter(PointerEventData eventData)
    {
        textDataName.color = Color.red;
    }
    // 마우스가 현재 Text UI를 클릭 했을때 호출
    // IpointerClickHandler 인터페이스를 상속받았을 때 구혀하는 메소드
    public void OnPointerClick(PointerEventData eventData)
    {
        directoryController.UpdateInputs(fileName);
    }
    //마우스가 현재 Text Ui 위에서 벗어날 때 호출
    //IpointerExitHandler 인터페이스르 상속받았을때 구현하는 메소드
    public void OnPointerExit(PointerEventData eventData)
    {
        SetTextColor();
    }



    private void SetTextColor()
    {
        if(dataType == DataType.Directory)
        {
            textDataName.color = Color.black;
        }
        else
        {
            textDataName.color = Color.black;
        }
    }

    
}
