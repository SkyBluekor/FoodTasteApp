using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImgLoaderinSetting : MonoBehaviour
{
    [SerializeField]
    private GameObject panelImgViewer;
    [SerializeField]
    private Image ImgDrawTex;
    [SerializeField]
    private TextMeshProUGUI textFileData;

    public static string ImgDic;
    private float maxWidth = 320;
    private float maxHeight = 300;

    private void Awake()
    {
        GetComponent<ImgLoaderinSetting>();
    }
    public void LoadFIleinSetting(FileInfo file)
    {
        if(file.FullName.Contains(".jpg") || file.FullName.Contains(".png") || file.FullName.Contains(".PNG"))
        {
            Debug.Log(".jpg, png 파일 선택");
            OnLoadinSetting(file);
        }
        else
        {
            OffLoadinSetting();
        }

    }

    public void OnLoadinSetting(FileInfo file)
    {
        panelImgViewer.SetActive(true);
        byte[] byteTex = File.ReadAllBytes(file.FullName);

        Texture2D tex2D = new Texture2D(0, 0);
     
        if(byteTex.Length >0)
        {
            tex2D.LoadImage(byteTex);
        }

        if (tex2D.width > maxWidth && tex2D.height > maxHeight)
        {
            // 이미지의 너비와 높이가 모두 최대 크기를 초과하면 양쪽을 조정
            float widthScale = maxWidth / tex2D.width;
            float heightScale = maxHeight / tex2D.height;
            float scale = Mathf.Min(widthScale, heightScale);

            ImgDrawTex.rectTransform.sizeDelta = new Vector2(tex2D.width * scale, tex2D.height * scale);
        }
        else if (tex2D.width > maxWidth)
        {
            // 이미지의 너비가 최대 너비를 초과하면 너비를 최대 너비로 고정
            ImgDrawTex.rectTransform.sizeDelta = new Vector2(maxWidth, maxWidth / tex2D.width * tex2D.height);
        }
        else if (tex2D.height > maxHeight)
        {
            // 이미지의 높이가 최대 높이를 초과하면 높이를 최대 높이로 조정
            float scale = maxHeight / tex2D.height;
            ImgDrawTex.rectTransform.sizeDelta = new Vector2(tex2D.width * scale, maxHeight);
        }
        else
        {
            // 이미지가 최대 너비와 최대 높이를 초과하지 않으면 원본 크기 유지
            ImgDrawTex.rectTransform.sizeDelta = new Vector2(tex2D.width, tex2D.height);
        }
        Sprite sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
        ImgDrawTex.sprite = sprite;
        textFileData.text = $"{file.Name} \n ({tex2D.width} x {tex2D.height}, {file.Length}Byte)";

        ImgDic = file.FullName;
    }
    public void OffLoadinSetting()
    {
        panelImgViewer.SetActive(false);
        textFileData.text = "사용할수 없는\n파일형식입니다.";
    }

}
