using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

public class IntroManager : MonoBehaviour
{
    public GameObject Intropanel;
    public Button EnterSignupBtn, LoginBtn;
    public GameObject SignupPanel, LoginFail , LoginSuccess;
    public TMP_InputField IDInputField, PwdInputField,CheckPwdInputField;
    EventSystem system;
    private Selectable firstinput;
    private bool tabKeyPressed = false;
    // Start is called before the first frame update
    public static string ID;
    void Start()
    {
        PwdInputField.onEndEdit.AddListener(SubmitInput);
        system = EventSystem.current;
        if(firstinput != null)
        {
            firstinput.Select();
        }

        OpenFilePanel OFP = (OpenFilePanel)ScriptableObject.CreateInstance(typeof(OpenFilePanel));
        
        EnterSignupBtn.onClick.AddListener(OnPanel);
        LoginBtn.onClick.AddListener(SigninBtnAction);

        var connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";";
        MySqlConnection conn = new MySqlConnection(connStr);

        try
        {
            conn.Open();
            Debug.Log("conn start");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        
        conn.Close();
    }

    private void SubmitInput(string inputText)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoginBtn.onClick.Invoke();
        }
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
    }

    void Update()
    {
        if(IDInputField.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                if (next != null)
                {
                    next.Select();
                }
            }
        }
        else
        {
            Input.imeCompositionMode= IMECompositionMode.On;
        }

        if(PwdInputField.isFocused || CheckPwdInputField.isFocused || IDInputField.isFocused) 
        {
            Input.imeCompositionMode = IMECompositionMode.Off;
        }
        else
        {
            Input.imeCompositionMode = IMECompositionMode.On;
        }

    }


    void OnPanel()
    {
        Intropanel.SetActive(false);
        SignupPanel.SetActive(true);
    }

    public void SigninBtnAction()
    {
        string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand cmd;
        string Id = IDInputField.text;
        string pwd = PwdInputField.text;
        Debug.Log("id " + Id + "pwd "+ pwd);
        PlayerPrefs.SetString("playerid", Id);
       
        bool IDCheck = false;
        bool PwdCheck = false;
        string LoginIdCh = "select Id from user where lower(ID) = '" + Id.ToLower() + "'";
        string LoginPwdCh = "select Pwd from user where pwd = '" + pwd + "'";
        

        try
        {
            conn.Open();
            cmd = new MySqlCommand(LoginIdCh, conn);
            MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
            while(mySqlDataReader.Read())
            {
                if(Id.Equals(mySqlDataReader.GetString(0).ToLower()))
                {
                    IDCheck = true;
                }
                else
                {
                    IDCheck = false;
                }
            }
            mySqlDataReader.Close();

            cmd = new MySqlCommand(LoginPwdCh, conn);
            cmd.ExecuteNonQuery();
            mySqlDataReader = cmd.ExecuteReader();
            while(mySqlDataReader.Read())
            {
                if(pwd.Equals(mySqlDataReader.GetString(0)))
                {
                    PwdCheck = true;
                }
                else
                {
                    PwdCheck = false;
                }
            }
            if(IDCheck && PwdCheck)
            {
                Debug.Log("·Î±×ÀÎ ¼º°ø");
                SuccessLogin();
                string playerid = PlayerPrefs.GetString("playerid");
                SceneManager.LoadScene("MainFuncScene");
            }
            else
            {
                ActivePanelForSeconds(2);
            }
           
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        conn.Close();
    }
    
    public class OpenFilePanel :EditorWindow
    {
        static void Apply()
        {
            Texture2D texture = Selection.activeObject as Texture2D;
            if(texture == null)
            {
                EditorUtility.DisplayDialog("Select Texture", "You Must select a texture first", "OK");
                return;
            }
            string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
            if(path.Length !=0 )
            {
                var fileContent = File.ReadAllBytes(path);
                texture.LoadImage(fileContent);
            }
        }
    }

    public void ActivePanelForSeconds(float seconds)
    {
        LoginFail.SetActive(true);
        PwdInputField.text = "";
        Invoke("DeActiveLoginFail", seconds);
        PwdInputField.Select();
        PwdInputField.ActivateInputField();
    }

    private void DeActiveLoginFail()
    {
        LoginFail.SetActive(false);
    }

    private async void SuccessLogin()
    {
        LoginSuccess.SetActive(true);
        await Task.Delay(1000);
    }
}
