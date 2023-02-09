using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonMapping : MonoBehaviour
{
    public string index;
    public GameObject iosBind;

    // Start is called before the first frame update
    void Start()
    {
        iosBind = GameObject.Find("iOSNative");
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
