using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeDebug : MonoBehaviour
{
    public Text text;
    public ArduinoController arduinoController;

    // Start is called before the first frame update
    void Start()
    {
        arduinoController.OnModeChanged += HandleModeChanged;
    }

    private void HandleModeChanged(ArduinoController.Mode mode)
    {
        text.text = mode.ToString();
    }

}
