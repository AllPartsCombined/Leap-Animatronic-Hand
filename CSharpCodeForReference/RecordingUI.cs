using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordingUI : MonoBehaviour
{
    public Toggle toggle;
    public ArduinoController arduinoController;

    // Start is called before the first frame update
    void Start()
    {
        arduinoController.OnRecordingSet += HandleRecordingSet;
    }

    private void HandleRecordingSet(bool on)
    {
        if(toggle.isOn != on)
            toggle.isOn = on;
    }

}
