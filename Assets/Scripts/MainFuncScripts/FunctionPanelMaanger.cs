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
    private float mealRotation;
    private int mealHalfTurn;

    // Start is called before the first frame update
    void Start()
    {
        EatenImages = new RawImage[3];
        EatenImages[0] = Morning;
        EatenImages[1] = AfterNoon;
        EatenImages[2] = Night;
        for (int i = 0; i < EatenImages.Length; i++)
            EatenImages[i].gameObject.SetActive(i == 0);

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

        mealRotation += rotationAngle;
        int halfTurn = Mathf.FloorToInt((mealRotation + 90f) / 180f);
        if (halfTurn != mealHalfTurn)
        {
            mealHalfTurn = halfTurn;
            currentImageIndex = halfTurn % EatenImages.Length;
            for (int i = 0; i < EatenImages.Length; i++)
                EatenImages[i].gameObject.SetActive(i == currentImageIndex);
        }

        // A rotated UI image is mirrored on its back side. Flip its UVs there
        // so each meal image reads normally as it turns toward the viewer.
        bool backFacing = Mathf.Repeat(mealRotation, 360f) > 90f &&
                          Mathf.Repeat(mealRotation, 360f) < 270f;
        EatenImages[currentImageIndex].uvRect = backFacing
            ? new Rect(1f, 0f, -1f, 1f) : new Rect(0f, 0f, 1f, 1f);

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
