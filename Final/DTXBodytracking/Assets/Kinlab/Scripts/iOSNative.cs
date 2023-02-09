using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class iOSNative : MonoBehaviour
{
    public TMP_Text label;
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
    internal static extern void __iOS_SettingComp_LabelTime(double time);
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        __iOS_Initialize();
    }
    public void TCPStart(GameObject window)
    {
        if (ipnumber != string.Empty && portnumber != -1)
        {
            window.SetActive(false);
            __iOS_TCPStart(ipnumber, portnumber);
        }
    }
    public void TCPEnd()
    {
        __iOS_TCPEnd();
    }
    public void SetPortnuber(string portstring)
    {
        int.TryParse(portstring, out _portnumber);
    }



    // Native

    public void __fromnative_Request_LabelSetting()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        double cur_time = (System.DateTime.UtcNow - epochStart).TotalMilliseconds;
        _KINLAB.GM_DataRecorder.instance.Enequeue_Data("DTX", "LABEL");
        Debug.Log(cur_time);
        __iOS_SettingComp_LabelTime(cur_time);
    }

    public void __fromnative_Request_LabelTimeSetting(string label)
    {
        _KINLAB.GM_DataRecorder.instance.Enequeue_Data("Label", label);
    }

    public void __fromnative_selfNumber(string label)
    {
        Debug.Log($"{label}");
    }
    public void __fromnative_LabelDataSend(string data)
    {
        Debug.Log($"{data}");
        label.text = data;
    }
}
