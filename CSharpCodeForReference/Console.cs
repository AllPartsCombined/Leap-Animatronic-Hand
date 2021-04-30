using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    public SerialCommunication serial;
    public GameObject consolePanel;
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public Text text;
    public InputField input;
    public bool jumpToBottom = true;
    public int maxLines = 500;
    float scrollbarValue = 1;
    // Start is called before the first frame update
    void Start()
    {
        serial.OnDataReceived += AddText;
        scrollbar.onValueChanged.AddListener(HandleScrollRectPositionChange);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            TogglePanel();
        if (Input.GetKeyDown(KeyCode.Return))
            SendInput();
    }

    private void SendInput()
    {
        if (!String.IsNullOrEmpty(input.text))
        {
            serial.Send(input.text);
            input.text = String.Empty;
        }
    }

    private void TogglePanel()
    {
        consolePanel.SetActive(!consolePanel.activeSelf);
    }

    private void HandleScrollRectPositionChange(float position)
    {
        if (position < scrollbar.size / 4f)
            jumpToBottom = true;
        else if (scrollbarValue != position && scrollbar.size < .5f)
            jumpToBottom = false;
    }

    IEnumerator AddTextCoroutine(string str)
    {
        text.text += str;
        int numLines = text.text.Length - text.text.Replace("\n", string.Empty).Length;
        if (numLines > maxLines)
            text.text = text.text.Substring(text.text.IndexOf("\n") + 1);
        if (jumpToBottom)
        {
            yield return new WaitForEndOfFrame();
            scrollbarValue = scrollbar.value;
            scrollRect.verticalNormalizedPosition = 0;
        }
    }

    public void AddText(string str)
    {
        StartCoroutine(AddTextCoroutine(str));
    }
}
