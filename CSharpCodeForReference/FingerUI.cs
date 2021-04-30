using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FingerUI : MonoBehaviour
{
    public Text thumbText, indexText, middleText, ringText, pinkyText;

    private void SetText(Text text, string str)
    {
        if (text != null)
            text.text = str;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var finger in FingerManager.instance.fingers)
        {
            switch (finger.type)
            {
                case Leap.Finger.FingerType.TYPE_THUMB:
                    SetText(thumbText, (finger.percentageClosed * 100).ToString("0") + "%");
                    break;
                case Leap.Finger.FingerType.TYPE_INDEX:
                    SetText(indexText, (finger.percentageClosed * 100).ToString("0") + "%");
                    break;
                case Leap.Finger.FingerType.TYPE_MIDDLE:
                    SetText(middleText, (finger.percentageClosed * 100).ToString("0") + "%");
                    break;
                case Leap.Finger.FingerType.TYPE_RING:
                    SetText(ringText, (finger.percentageClosed * 100).ToString("0") + "%");
                    break;
                case Leap.Finger.FingerType.TYPE_PINKY:
                    SetText(pinkyText, (finger.percentageClosed * 100).ToString("0") + "%");
                    break;
            }
        }
    }
}
