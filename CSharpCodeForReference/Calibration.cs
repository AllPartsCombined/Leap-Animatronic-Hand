using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibration : MonoBehaviour
{
    public enum State { None, Start, PreCalibrateOpen, CalibratingOpen, PreCalibrateFist, CalibratingFist, Done }
    public State state;

    public event System.Action<State> OnStateChange;
    public event System.Action<float> OnProgressUpdated;

    private void Start()
    {
        SetState(State.Start);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (state)
            {
                case State.Start:
                    SetState(State.PreCalibrateOpen);
                    break;
                case State.PreCalibrateOpen:
                    SetState(State.CalibratingOpen);
                    break;
                case State.PreCalibrateFist:
                    SetState(State.CalibratingFist);
                    break;
            }
        }
    }

    public void SetState(State newState)
    {
        if (state == newState)
            return;
        Debug.Log("Set State to " + newState.ToString());
        state = newState;
        if (OnStateChange != null)
            OnStateChange(state);
        switch (state)
        {
            case State.CalibratingOpen:
                StartCoroutine(Calibrate(true));
                break;
            case State.CalibratingFist:
                StartCoroutine(Calibrate(false));
                break;
        }
    }

    public void ClearCalibration()
    {
        PlayerPrefs.DeleteAll();
    }

    IEnumerator Calibrate(bool open)
    {
        List<float> averageDistances = new List<float>();
        foreach (var finger in FingerManager.instance.fingers)
        {
            averageDistances.Add(0);
        }
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < FingerManager.instance.fingers.Count; j++)
            {
                averageDistances[j] += FingerManager.instance.fingers[j].distanceFromPalm / 100f;
            }
            if (OnProgressUpdated != null)
                OnProgressUpdated((float)(i + 1) / 100f);
            yield return null;
        }
        for (int i = 0; i < FingerManager.instance.fingers.Count; i++)
        {
            PlayerPrefs.SetFloat(FingerManager.instance.fingers[i].type.ToString() + (open ? "OpenDistance" : "ClosedDistance"),
                averageDistances[i]);
        }
        if (open)
            SetState(State.PreCalibrateFist);
        else
            SetState(State.Done);
    }
}
