using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Net.Sockets;

public class iOSNative : MonoBehaviour
{

    public GameObject button;
    public TMP_Text number;
    public TMP_Text number2;
    public GameObject canvas_scrollview;
    public List<int> providerNum;
    public GameObject panel;



    public GameObject panel2;
    public Button check_time;
    public TMP_Text times;
    public Button check_labelingData;
    public TMP_InputField label;


    public int device_number;
    public int bind_number;

    string _ipnumber = string.Empty;
    public string ipnumber
    {
        get { return _ipnumber; }
        set { _ipnumber = value; }
    }
    int _portnumber = -1;
    public int portnumber
    {
        get { return _portnumber; }
        set { _portnumber = value; }
    }
    #region iOSNativePlugin
    [DllImport("__Internal")]
    internal static extern void __iOS_Initialize();
    [DllImport("__Internal")]
    internal static extern void __iOS_TCPStart(string ipnumber, int port);
    [DllImport("__Internal")]
    internal static extern void __iOS_TCPEnd();
    [DllImport("__Internal")]
    internal static extern void __iOS_RequestGetProvider();
    [DllImport("__Internal")]
    internal static extern void __iOS_RequestBindProvider(string num);
    [DllImport("__Internal")]
    internal static extern void __iOS_SetLabelString(string labelstring);
    [DllImport("__Internal")]
    internal static extern void __iOS_Set_LabelTime();

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        __iOS_Initialize();
    }
    public void TCPStart()
    {
        __iOS_TCPStart(ipnumber, portnumber);
    }
    public void TCPEnd()
    {
        __iOS_TCPEnd();
    }
    public void RequestGetProvider()
    {
        __iOS_RequestGetProvider();
    }
    public void BindProvider(string providerNum)
    {
        Debug.Log($"ProviderNum = {providerNum}");
        __iOS_RequestBindProvider(providerNum);
        panel.SetActive(false);
    }
    public void GetProvider(string num)
    {
      //  __iOS_GetProvider(num);
        providerNum.Add(int.Parse(num));
        ResetButton();
       // MakeProviderButton();
    }
    public void MakeProviderButton(string num)
    {
        GameObject tmp = Instantiate(button, canvas_scrollview.transform);
        tmp.GetComponent<ButtonMapping>().index = num;
        tmp.transform.GetChild(0).GetComponent<TMP_Text>().text = "Provider : " + num.ToString();
        tmp.GetComponent<Button>().onClick.AddListener(() => BindProvider(num.ToString()));
    }
    private void ResetButton()
    {
        var c = canvas_scrollview.transform.childCount;
        if(c>=1)
        {
            for (int i = 0; i < c; i++)
            {
                GameObject tne = canvas_scrollview.transform.GetChild(0).gameObject;
                Destroy(tne);
            }
        }
    }

    public void LabelString_Send()
    {
        __iOS_SetLabelString(label.text);
        label.text = "";

    }
    public void LabelTimeSetting()
    {
        __iOS_Set_LabelTime();
        times.text = "Time Setting...";
    }
    

    /// <summary>
    /// Ios 에서 unity로 전달하기 위한 sendmessage 함수 모음
    /// </summary>
    /// <param name="data"></param>
    public void __fromnative_Request_ProviderList(string data)
    {
        int datas=int.Parse(data);
        Debug.Log(datas);
        MakeProviderButton(data);
    }
    public void __fromnative_selfNumber(string data)
    {
        device_number=int.Parse(data);
        number.text = $"Labeler Number : {data}";
    }
    public void __fromnative_disconnectProvider(string data)
    {
        number.text = $"DisConnected : {data}";
    }
    public void __fromnative_bindNumber(string data)
    {
        bind_number = int.Parse(data);
        number2.text = $"Provider: {data}";
        times.text = $"Data Send. Resetting Please";
    }
    public void __fromnative_LabelTimeSet(string time)
    {
        times.text = $"Time : {time}";
    }
    public void __fromnative_LabelDataSend(string data)
    {
        label.text = "전송완료";
    }

}
