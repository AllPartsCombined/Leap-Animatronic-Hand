using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class FingerManager : MonoBehaviour
{
    public static FingerManager instance;
    public Controller controller;
    public List<FingerData> fingers;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = new Controller();
    }

    // Update is called once per frame
    void Update()
    {
        Frame frame = controller.Frame();
        if (frame.Hands.Count > 0)
        {
            List<Hand> hands = frame.Hands;
            Hand rightHand = null;
            foreach (Hand hand in hands)
            {
                if (hand.IsRight)
                {
                    rightHand = hand;
                    break;
                }
            }
            if (rightHand != null)
            {
                foreach (var finger in rightHand.Fingers)
                {
                    foreach (var fingerData in fingers)
                    {
                        if (finger.Type == fingerData.type)
                        {
                            fingerData.distanceFromPalm = rightHand.PalmPosition.DistanceTo(finger.TipPosition);
                            float openDistance = PlayerPrefs.GetFloat(finger.Type.ToString() + "OpenDistance", -1f);
                            float closedDistance = PlayerPrefs.GetFloat(finger.Type.ToString() + "ClosedDistance", -1f);
                            if (openDistance == -1 || closedDistance == -1)
                            {
                                continue;
                            }
                            fingerData.percentageClosed =  1 - Mathf.Clamp01((fingerData.distanceFromPalm - closedDistance) / (openDistance - closedDistance));
                        }
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class FingerData
{
    public Finger.FingerType type;
    public float distanceFromPalm;
    public float percentageClosed;
}
