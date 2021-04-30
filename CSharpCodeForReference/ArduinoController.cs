using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    public enum Mode { Off, Live, Playback };

    public SerialCommunication serial;
    public bool on = true;

    private Mode mode;
    private Mode newMode;
    private int previousThumbVal = 255;
    private int previousIndexVal = 255;
    private int previousMiddleVal = 255;
    private int previousRingVal = 255;
    private int previousPinkyVal = 255;

    private bool recording;

    public event Action<bool> OnRecordingSet;

    public event Action<Mode> OnModeChanged;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SendFingerPositionsCoroutine());
        SendModeChange();
        SetFile(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && mode == Mode.Live)
            SendRecording(!recording);
    }

    public void SetFile(int file)
    {
        serial.Send(250);
        serial.Send(file + 1);
    }

    public void SetMode(Mode mode)
    {
        newMode = mode;
        if (OnModeChanged != null)
            OnModeChanged(newMode);
    }

    public void SetModeOff(bool on)
    {
        if (on)
            SetMode(Mode.Off);
    }

    public void SetModeLive(bool on)
    {
        if (on)
            SetMode(Mode.Live);
    }

    public void SetModePlayback(bool on)
    {
        if (on)
            SetMode(Mode.Playback);
    }

    IEnumerator SendFingerPositionsCoroutine()
    {
        while (on)
        {
            if (newMode != mode)
            {
                mode = newMode;
                SendModeChange();
            }
            switch (mode)
            {
                case Mode.Off:
                    break;
                case Mode.Live:
                    SendLiveData();
                    break;
                case Mode.Playback:
                    break;
            }
            serial.Write();
            yield return new WaitForSecondsRealtime(.05f);
        }
    }

    private void SendLiveData()
    {
        serial.Send(253);
        foreach (var finger in FingerManager.instance.fingers)
        {
            int newVal = (int)(128 * finger.percentageClosed) + 1;

            switch (finger.type)
            {
                case Leap.Finger.FingerType.TYPE_THUMB:
                    if (newVal != previousThumbVal || recording)
                    {
                        serial.Send('T');
                        serial.Send(newVal);
                        previousThumbVal = newVal;
                    }
                    break;
                case Leap.Finger.FingerType.TYPE_INDEX:
                    if (newVal != previousIndexVal || recording)
                    {
                        serial.Send('I');
                        serial.Send(newVal);
                        previousIndexVal = newVal;
                    }
                    break;
                case Leap.Finger.FingerType.TYPE_MIDDLE:
                    if (newVal != previousMiddleVal || recording)
                    {
                        serial.Send('M');
                        serial.Send(newVal);
                        previousMiddleVal = newVal;
                    }
                    break;
                case Leap.Finger.FingerType.TYPE_RING:
                    if (newVal != previousRingVal || recording)
                    {
                        serial.Send('R');
                        serial.Send(newVal);
                        previousRingVal = newVal;
                    }
                    break;
                case Leap.Finger.FingerType.TYPE_PINKY:
                    if (newVal != previousPinkyVal || recording)
                    {
                        serial.Send('P');
                        serial.Send(newVal);
                        previousPinkyVal = newVal;
                    }
                    break;
            }
        }
        serial.Send(254);
    }

    private void SendModeChange()
    {
        serial.Send(255);
        switch (mode)
        {
            case Mode.Off:
                serial.Send(1);
                break;
            case Mode.Live:
                serial.Send(2);
                break;
            case Mode.Playback:
                serial.Send(3);
                break;
        }
    }

    public void SendRecording(bool record)
    {
        if (recording == record)
            return;
        recording = record;
        OnRecordingSet(record);
        if (record)
            serial.Send(251);
        else
            serial.Send(252);
    }
}
