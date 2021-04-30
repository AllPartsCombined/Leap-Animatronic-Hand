using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationUI : MonoBehaviour
{
    public Calibration calibration;
    public GameObject startScreen, preOpenScreen, openScreen, preFistScreen, fistScreen, doneScreen;
    public Image progressBar;

    private void Toggle(GameObject go, bool on)
    {
        if (go != null)
            go.SetActive(on);
    }

    private void ToggleScreen(GameObject screen)
    {
        Toggle(startScreen, false);
        Toggle(preOpenScreen, false);
        Toggle(openScreen, false);
        Toggle(preFistScreen, false);
        Toggle(fistScreen, false);
        Toggle(doneScreen, false);
        Toggle(screen, true);
    }

    // Start is called before the first frame update
    void Awake()
    {
        calibration.OnStateChange += HandleStateChange;
        calibration.OnProgressUpdated += HandleProgressUpdated;
    }

    private void HandleProgressUpdated(float val)
    {
        if (progressBar == null)
            return;
        progressBar.fillAmount = val;
    }

    private void HandleStateChange(Calibration.State newState)
    {
        switch (newState)
        {
            case Calibration.State.Start:
                ToggleScreen(startScreen);
                break;
            case Calibration.State.PreCalibrateOpen:
                ToggleScreen(preOpenScreen);
                break;
            case Calibration.State.CalibratingOpen:
                ToggleScreen(openScreen);
                break;
            case Calibration.State.PreCalibrateFist:
                ToggleScreen(preFistScreen);
                break;
            case Calibration.State.CalibratingFist:
                ToggleScreen(fistScreen);
                break;
            case Calibration.State.Done:
                ToggleScreen(doneScreen);
                break;
        }
    }


}
