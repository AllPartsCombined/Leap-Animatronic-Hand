using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class SerialCommunication : MonoBehaviour
{
    public string portName = "COM6";
    public int baudRate = 9600;
    public Parity parity = Parity.None;
    public bool writeEveryFrame = false;
    public bool debugOut;
    private SerialPort port;
    private List<byte> bytes = new List<byte>();

    public event Action<string> OnDataReceived;

    // Start is called before the first frame update
    void Start()
    {
        port = new SerialPort(portName, baudRate, parity);
        port.Open();
    }

    private void Update()
    {
        if (writeEveryFrame)
            Write();
        while (port.BytesToRead > 0)
        {
            string indata = port.ReadLine();
            if (!String.IsNullOrEmpty(indata))
            {
                if (OnDataReceived != null)
                    OnDataReceived(indata + "\n");
            }
        }
    }

    public void Write()
    {
        if (bytes.Count > 0)
        {
            if (debugOut)
                LogBytes();
            port.Write(bytes.ToArray(), 0, bytes.Count);
            bytes.Clear();
        }
    }

    void LogBytes()
    {
        string s = "";
        foreach (byte b in bytes)
        {
            s += Convert.ToInt16(b).ToString();
            s += " ";
        }
        Debug.Log("Outgoing Bytes: " + s);
    }

    public void Send(int val)
    {
        Send(Convert.ToByte(val));
    }

    public void Send(char val)
    {
        Send(Convert.ToByte(val));
    }

    public void Send(string val)
    {
        port.Write(val);
    }

    public void Send(byte val)
    {
        bytes.Add(val);
    }

    private void OnDestroy()
    {
        port.Close();
    }

}
