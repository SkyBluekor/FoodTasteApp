using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class FunctionPanelMaanger : MonoBehaviour
{
    public GameObject FunctionPanel;
    public GameObject FoodEvaPanel , EatenPanel;
    [SerializeField]
    private UnityEngine.UI.Button ChEvaPanelBtn;
    [SerializeField]
    private UnityEngine.UI.Button ChRoultteBtn;
    [SerializeField]
    private UnityEngine.UI.Button EatenPanelBtn;
    [SerializeField]
    private UnityEngine.UI.Button UPDownBtn;
    
    public RawImage ChFront, ChBack;
    public RawImage Morning, AfterNoon, Night;
    private RawImage[] EatenImages;
    private float rotationSpeed = 60.0f;
    private int previousQuotient = 0;
    private int currentImageIndex = 0;
    private Quaternion initialRotation;

    // Start is called before the first frame update
    void Start()
    {
        initialRotation = Morning.transform.localRotation;
        EatenImages = new RawImage[3];
        EatenImages[0] = Morning;
        EatenImages[1] = AfterNoon;
        EatenImages[2] = Night;

        ChEvaPanelBtn.onClick.AddListener(() => 
        {
            FunctionPanel.SetActive(false);
            FoodEvaPanel.SetActive(true);
        });

        ChRoultteBtn.onClick.AddListener(() =>
        {
            string playerid;
            playerid = PlayerPrefs.GetString("playerid");
            Debug.Log(playerid);

            LoadRoultteScene();
        });

        EatenPanelBtn.onClick.AddListener(() =>
        {
            string playerid;
            playerid = PlayerPrefs.GetString("playerid");
            Debug.Log(playerid);

            FunctionPanel.SetActive(false);
            EatenPanel.SetActive(true);
        });

        UPDownBtn.onClick.AddListener(() =>
        {
            string playerid;

            playerid = PlayerPrefs.GetString("playerid");
            LoadUpdownScene();
        });
    }

    private void Update()
    {
        float rotationAngle = rotationSpeed * Time.deltaTime;
        ChFront.rectTransform.Rotate(Vector3.up * rotationAngle);
        ChBack.rectTransform.Rotate(Vector3.up * rotationAngle);
        Morning.rectTransform.Rotate(Vector3.up * rotationAngle);
        AfterNoon.rectTransform.Rotate(Vector3.up * rotationAngle);
        Night.rectTransform.Rotate(Vector3.up * rotationAngle);

        if (ChFront.rectTransform.localEulerAngles.y > 90 && ChFront.rectTransform.localEulerAngles.y < 270)
        {
            ChFront.gameObject.SetActive(false);
            ChBack.gameObject.SetActive(true);
        }
        else
        {
            ChFront.gameObject.SetActive(true);
            ChBack.gameObject.SetActive(false);
        }

        Quaternion currentRotation = Morning.transform.localRotation;

        if(Quaternion.Angle(initialRotation, currentRotation) >= 90.0f)
        {
            currentImageIndex = (currentImageIndex + 1) % EatenImages.Length;

            // 모든 이미지를 비활성화
            foreach (RawImage image in EatenImages)
            {
                image.gameObject.SetActive(false);
            }

            // 현재 이미지를 활성화
            EatenImages[currentImageIndex].gameObject.SetActive(true);

            // 현재 회전을 초기 회전으로 업데이트
            initialRotation = currentRotation;
        }

    }

    public void LoadRoultteScene()
    {
        SceneManager.LoadScene("RouletteScene");
    }
    
    public void LoadUpdownScene()
    {
        SceneManager.LoadScene("UP&DownGameScene");
    }
    
}
