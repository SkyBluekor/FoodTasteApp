using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DataInSetting : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private Sprite[] spriteicon;

    private Image imageicon;
    private TextMeshProUGUI textDataName;
    private DataType DataTypeinSetting;

    private string fileName;
    public string FilenameinSetting => fileName;

    private int maxFiledNameLength = 25;
    private DCinSetting dcinsetting;
    
    public void SettingSetup(DCinSetting dCinSetting, string filename, DataType dataTypeinSetting)
    {
        dcinsetting = dCinSetting;

        imageicon = GetComponentInChildren<Image>();
        textDataName = GetComponentInChildren<TextMeshProUGUI>();

        this.fileName = filename;
        this.DataTypeinSetting = dataTypeinSetting;

        imageicon.sprite = spriteicon[(int)this.DataTypeinSetting];
        textDataName.text = this.fileName;

        if(fileName.Length >= maxFiledNameLength)
        {
            textDataName.text = fileName.Substring(0, maxFiledNameLength);
            textDataName.text += "..";
        }
        SetTextColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textDataName.color = Color.red;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        dcinsetting.UpdateInputsinSetting(fileName);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        SetTextColor();
    }


    private void SetTextColor()
    {
        if(DataTypeinSetting == DataType.Directory)
        {
            textDataName.color = Color.black;
        }
        else
        {
            textDataName.color = Color.black;
        }
    }

}
