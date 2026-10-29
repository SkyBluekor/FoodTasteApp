using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject panelImageViewer; // 이미지 정보를 출력하는 image

    [SerializeField]
    private Image imageDrawTexture; //파일이 나타내는 이미지 출력
    [SerializeField]
    private TextMeshProUGUI textFileData; // 파일이름 ,해상도 용량

    public static string ImgDirectory;
    private float maxWidth = 980;
    private float maxHeight = 800;

    private void Awake()
    {
        GetComponent<ImageLoader>();
    }

    public void OnLoad(FileInfo file)
    {
        Debug.Log("동작되냐");
        panelImageViewer.SetActive(true);

        byte[] byteTexture = File.ReadAllBytes(file.FullName);

        //byteTexture 에 있는 byte 배열 정보를 바탕으로 Texture2D 이미지파일 데이터 생성
        Texture2D texture2D = new Texture2D(0, 0);
        if(byteTexture.Length>0)
        {
            texture2D.LoadImage(byteTexture);
        }

        //이미지를 출력하는 Image UI의 크기 설정
        // 원본 텍스처의 width 크기가 Image UI의 최대 width 크기보다 크면
        if (texture2D.width > maxWidth && texture2D.height > maxHeight)
        {
            // 이미지의 너비와 높이가 모두 최대 크기를 초과하면 양쪽을 조정
            float widthScale = maxWidth / texture2D.width;
            float heightScale = maxHeight / texture2D.height;
            float scale = Mathf.Min(widthScale, heightScale);

            imageDrawTexture.rectTransform.sizeDelta = new Vector2(texture2D.width * scale, texture2D.height * scale);
        }
        else if (texture2D.width > maxWidth)
        {
            // 이미지의 너비가 최대 너비를 초과하면 너비를 최대 너비로 고정
            imageDrawTexture.rectTransform.sizeDelta = new Vector2(maxWidth, maxWidth / texture2D.width * texture2D.height);
        }
        else if (texture2D.height > maxHeight)
        {
            // 이미지의 높이가 최대 높이를 초과하면 높이를 최대 높이로 조정
            float scale = maxHeight / texture2D.height;
            imageDrawTexture.rectTransform.sizeDelta = new Vector2(texture2D.width * scale, maxHeight);
        }
        else
        {
            // 이미지가 최대 너비와 최대 높이를 초과하지 않으면 원본 크기 유지
            imageDrawTexture.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
        }
        /*
        if (texture2D.width >maxWidth)
        {
            imageDrawTexture.rectTransform.sizeDelta = new Vector2(maxWidth, maxWidth / texture2D.width * texture2D.height);
        }
        // 원본 텍스처의 height 크기가 Image UI의 최대 width 크기보다 크면
        else if (texture2D.height > maxHeight)
        {
            imageDrawTexture.rectTransform.sizeDelta = new Vector2(maxHeight / texture2D.height * texture2D.width, maxHeight);
        }
        else
        {
            imageDrawTexture.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
        }
        */
        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        imageDrawTexture.sprite = sprite;
        textFileData.text = $"{file.Name} \n ({texture2D.width} x {texture2D.height}, {file.Length}Byte)";
        
        ImgDirectory = file.FullName;
        Debug.Log("Fullname"+ImgDirectory);

    }
    public void LoadFile(FileInfo file)
    {
        Debug.Log("start");


        if (file.FullName.Contains(".jpg") || file.FullName.Contains(".png") || file.FullName.Contains(".PNG"))
        {
            Debug.Log("Image Test");

            OnLoad(file);
        }
        else
        {
            OffLoad();
        }


    }
    public void OffLoad()
    {
        panelImageViewer.SetActive(false);
    }
}
