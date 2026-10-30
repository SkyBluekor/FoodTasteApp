using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using MySql.Data;
using System;
using TMPro;
using System.IO;
using UnityEngine.EventSystems;

public class SignupPanelManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField IDInput;
    [SerializeField] private TMP_InputField PWDInput;
    [SerializeField] private TMP_InputField PWDCHeckInput;
    [SerializeField] private TMP_InputField NickNameInput;
    [SerializeField] private TMP_Dropdown TenDigitInput;
    [SerializeField] private TMP_Dropdown OneDigitInput;
    [SerializeField] private TMP_Dropdown DoRegionInput;
    [SerializeField] private TMP_Dropdown SiguRegionInput;
    [SerializeField] private GameObject SignupPanel;
    [SerializeField] private GameObject IntroPanel;

    [SerializeField] private Button SignupBtn;
    [SerializeField] private Button BackBtn;

    [SerializeField] private GameObject SignConfirmPanel;
    [SerializeField] private TMP_Text ErrorTxt;
    [SerializeField] private Button ConfirmBtn;

    EventSystem system;
    private Selectable firstInput;

    
    string connStr = "Server=localhost;database=tasteapp;userid=root;password=" + FoodTasteDbPassword.Resolve() + ";allow user variables=true;";
    MySqlConnection conn;
    
    MySqlCommand cmd;
    int FileSize = 0;
    byte[] rawdata;
    FileStream fs;

    private String Id,PWD,PWDCheck,NickName,TenDigit,OneDigit,DoRegion,SiguRegion;
    
    private void Awake()
    {
        SetText();
    }
    

    // Start is called before the first frame update
    void Start()
    {
        system = EventSystem.current;
        if(firstInput != null)
        {
            firstInput.Select();
        }
        
        SignupBtn.onClick.AddListener(SignupAction);
        BackBtn.onClick.AddListener(BackActtion);
    }

    private void Update()
    {
        // Only handle signup navigation while the signup page is visible.
        // FindSelectableOnDown() used spatial/auto navigation and sometimes
        // jumped over PWDCheckInput. Use an explicit, stable field order.
        if (SignupPanel == null || !SignupPanel.activeInHierarchy)
            return;

        if (PWDInput.isFocused || PWDCHeckInput.isFocused || IDInput.isFocused)
            Input.imeCompositionMode = IMECompositionMode.Off;
        else
            Input.imeCompositionMode = IMECompositionMode.On;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable[] order = {
                IDInput, PWDInput, PWDCHeckInput, NickNameInput,
                TenDigitInput, OneDigitInput, DoRegionInput, SiguRegionInput
            };
            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject : null;
            Selectable current = selected != null ? selected.GetComponent<Selectable>() : null;
            int index = Array.IndexOf(order, current);
            int step = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -1 : 1;
            int nextIndex = index < 0 ? (step > 0 ? 0 : order.Length - 1)
                : (index + step + order.Length) % order.Length;
            if (current is TMP_InputField currentInput)
                currentInput.DeactivateInputField();
            Selectable target = order[nextIndex];
            if (target != null)
            {
                target.Select();
                if (target is TMP_InputField input)
                    input.ActivateInputField();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Enter on a dropdown selects its option; it must not submit signup.
            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null || selected.GetComponent<TMP_Dropdown>() == null)
                SignupBtn.onClick.Invoke();
        }
    }

    private void SetText()
    {
        SignupPanel.GetComponent<GameObject>();
        IntroPanel.GetComponent<GameObject>();

        IDInput.GetComponent<TMP_InputField>();
        PWDInput.GetComponent<TMP_InputField>();
        PWDCHeckInput.GetComponent<TMP_InputField>();
        NickNameInput.GetComponent<TMP_InputField>();
        TenDigitInput.GetComponent<TMP_Dropdown>();
        OneDigitInput.GetComponent<TMP_Dropdown>();
        DoRegionInput.GetComponent<TMP_Dropdown>();
        SiguRegionInput.GetComponent<TMP_Dropdown>();

        SignupBtn.GetComponent<Button>();
        BackBtn.GetComponent<Button>();

        SignConfirmPanel.GetComponent<GameObject>();
        ConfirmBtn.GetComponent<Button>();
        ErrorTxt.GetComponent<TextMeshPro>();
        
    }

    private void BackActtion()
    {
        SignupPanel.SetActive(false);
        IntroPanel.SetActive(true);
    }

    private void SignupAction()
    {
        string errorstr = "";
        Id = IDInput.text;
        PWD = PWDInput.text;
        PWDCheck = PWDCHeckInput.text;
        NickName = NickNameInput.text;
        TenDigit = TenDigitInput.options[TenDigitInput.value].text;
        OneDigit = OneDigitInput.options[OneDigitInput.value].text;
        DoRegion = DoRegionInput.options[DoRegionInput.value].text;
        SiguRegion = SiguRegionInput.options[SiguRegionInput.value].text;
        Debug.Log(Id + " " + PWD + " " + NickName + " " + TenDigit + " " + OneDigit + " " + DoRegion + " " + SiguRegion);
        int age = int.Parse(TenDigit)*10+int.Parse(OneDigit);
        string strage = age.ToString();
        int NickCheck = 0;
        int IDCheck = 0;

        string imgdir = ImageLoader.ImgDirectory;

        Debug.Log("signup directory"+imgdir);

        Debug.Log("PWd" + PWD + " pwdcheck" + PWDCheck);
        
        conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            Debug.Log("conn start");
            if (imgdir != null)
            {
                fs = new FileStream(imgdir, FileMode.Open, FileAccess.Read);
                FileSize = (int)fs.Length;

                rawdata = new byte[FileSize];
                fs.Read(rawdata, 0, FileSize);
                fs.Close();
            }
            string checkIdQuery = "select count(ID) from user where ID = '" + Id + "'";
            try
            {
                cmd = new MySqlCommand(checkIdQuery, conn);
                cmd.ExecuteNonQuery();
                MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
                while(mySqlDataReader.Read())
                {
                    IDCheck = mySqlDataReader.GetInt32(0);
                }
                mySqlDataReader.Close();
                Debug.Log("idcheck" + IDCheck);
            }
            catch(MySql.Data.MySqlClient.MySqlException e)
            {
                Debug.Log(e.Message);
            }

            string selectQuery = "select count(nickname) from user where Nickname = '" + NickName +"'";
            try
            {
                cmd = new MySqlCommand(selectQuery, conn);
                NickCheck = cmd.ExecuteNonQuery();
                MySqlDataReader mySqlDataReader = cmd.ExecuteReader();
                while (mySqlDataReader.Read())
                {
                    NickCheck = mySqlDataReader.GetInt32(0);
                }
                mySqlDataReader.Close();
                Debug.Log(NickCheck);
            }
            catch(MySql.Data.MySqlClient.MySqlException e)
            {
                Debug.Log(e.Message);
            }

            string insertnullImg = "Insert into user(Id,Pwd,NickName,Age,Region) values('" + Id + "','" + PWD + "','" + NickName + "','" + age + "','" + DoRegion +" " + SiguRegion;
            string insertQuery = " Insert into user(Id,Pwd,NickName,Age,Region,ProImg) values('" + Id + "','" + PWD + "','" + NickName + "','" + age + "','" + DoRegion +" " + SiguRegion + "', + @image)";
            
            Debug.Log(insertQuery);
            Debug.Log("Imgdir " + imgdir);
            if (imgdir == "")
            {
                cmd = new MySqlCommand(insertnullImg, conn);
            }
            else if (!(imgdir == ""))
            {
                cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@image", rawdata);
            }
            if (FileSize <= 16777215 && NickCheck != 1 && PWD.Equals(PWDCheck) && IDCheck !=1 && Id != "" && PWD != "" && PWDCheck != "" && NickName != "" && DoRegion !="도 선택" && SiguRegion !="구 선택")
            {
                cmd.ExecuteNonQuery();
                errorstr = "가입이 완료되었습니다.";
                IDInput.text = "";
                PWDInput.text = "";                
                PWDCHeckInput.text = "";
                NickNameInput.text = "";
                TenDigitInput.value = 0;
                OneDigitInput.value = 0;
                DoRegionInput.value = 0;
                SiguRegionInput.value = 0;
                TenDigitInput.RefreshShownValue();
                OneDigitInput.RefreshShownValue();
                SiguRegionInput.RefreshShownValue();
                DoRegionInput.RefreshShownValue();
                //SignConfirmPanel.SetActive(true);
            }
            else
            {
                if (IDCheck == 1)
                {
                    errorstr += "이미 존재하는 ID 입니다. \n";
                }
                if (!PWD.Equals(PWDCheck))
                {
                    errorstr += "비밀번호가 일치하지 않습니다. \n";
                }
                if (NickCheck == 1)
                {
                    errorstr += "이미 존재하는 닉네임 입니다. \n";
                }
                if (FileSize >= 1677215)
                {
                    errorstr = "이미지 파일 16MB 이하로 설정해주세요. \n";
                }
               if (Id == "")
                {
                    errorstr += "아이디를 입력해주세요. \n";
                }
               if(PWD == "")
                {
                    errorstr += "비밀번호를 입력해주세요. \n";
                }
                if (PWDCheck == "")
                {
                    errorstr += "비밀번호를 입력해주세요. \n";
                }
                if (NickName == "")
                {
                    errorstr += "닉네임을 입력해주세요. \n";
                }
                if(DoRegion == "도 선택" || SiguRegion =="구 선택")
                {
                    errorstr += "지역을 선택해주세요. \n";
                }
            }
            ErrorTxt.SetText(errorstr);
            SignConfirmPanel.SetActive(true);
            conn.Close();

        }
        catch (MySql.Data.MySqlClient.MySqlException e)
        {
            Debug.Log(e.Message);

        }
        
       
        ConfirmBtn.onClick.AddListener(delegate { ActConfirmBtn(errorstr); });


       
        conn.Close();
    }
    private void ActConfirmBtn(string errstr)
    {
        if (errstr.Equals("가입이 완료되었습니다."))
        {
            SignConfirmPanel.SetActive(false);
            SignupPanel.SetActive(false);
            IntroPanel.SetActive(true);
        }
        else
        {
            SignConfirmPanel.SetActive(false);
            SignupPanel.SetActive(true);
            IntroPanel.SetActive(false);
        }
    }
}
