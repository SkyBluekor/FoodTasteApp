using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateProfileFunc : MonoBehaviour
{
    public Image ChProfileIMg;
    public Image SettingPanelProImg;
    public Button ChangeBtn;
    public GameObject ChProPanel;
    public Button CloseProPanelBtn;
    public Button OpenProChangePanelBtn;
    public TextMeshProUGUI StateCheckText;

    private string previousText;
    private string currentID = "";
    private string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    private MySqlConnection conn;


    // Start is called before the first frame update
    void Start()
    {
        currentID = PlayerPrefs.GetString("playerid");
        ChangeBtn.onClick.AddListener(UpdateProfileImg);
        CloseProPanelBtn.onClick.AddListener(CloseFunc);
        OpenProChangePanelBtn.onClick.AddListener(OpenFunc);
    }

    private void UpdateProfileImg()
    {
        conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            if (StateCheckText.text == "사용할수 없는\n파일형식입니다.")
            {
                return;
            }
            byte[] ImgBytes = ConvertImgtoByteArr();
            previousText = StateCheckText.text;
            if(ImgBytes.Length > 16777215) 
            {
                Debug.Log("파일의 크기가 너무 큽니다.");
                StartCoroutine(MessageShow());
                return;
            }
            MySqlCommand cmd = new MySqlCommand("update user set proimg = @img where id = @userid ",conn);
            cmd.Parameters.AddWithValue("@img", ImgBytes);
            cmd.Parameters.AddWithValue("@userid", currentID);
            cmd.ExecuteNonQuery();
            SettingPanelProImg.sprite = ChProfileIMg.sprite;
            StartCoroutine(ChSuccessFunc());
        }
        catch(MySqlException e) 
        { 
            Debug.Log("동작실패: " + e);
        }
        finally
        {
            conn.Close();
        }
    }

   private byte[] ConvertImgtoByteArr()
    {
        Texture2D tex = ChProfileIMg.sprite.texture;
        byte[] bytes = tex.EncodeToPNG();
        return bytes;
    }

    private void CloseFunc()
    {
        ChProPanel.SetActive(false);
    }

    private void OpenFunc()
    {
        ChProPanel.SetActive(true);
    }
    private IEnumerator MessageShow()
    {
        while(true) 
        {
            StateCheckText.text = "이미지 파일의 크기가 너무 큽니다.";
            yield return new WaitForSeconds(1f);
            StateCheckText.text = previousText;

        }
    }

    private IEnumerator ChSuccessFunc() 
    {
        StateCheckText.text = "프로필 이미지가 변경되었습니다.";
        yield return new WaitForSeconds(1f);
        StateCheckText.text = "파일 정보";
        ChProPanel.SetActive(false);

    }
}
