/*
 * TrelloDemoCard.cs
 * Demonstration of a system information card.
 *  
 * by Àdam Carballo under MIT license.
 * https://github.com/AdamCarballo/Unity-Trello
 */

using System;
using System.Globalization;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Trello;

public class TrelloUITookitDemoCard : MonoBehaviour {

    public TrelloSendSO trelloSend;

    //[Header("UI References")]
    private TextField title;
    private TextField desc;
    private RadioButton screenshot;
    private Button toggleButton;
    private Button submitButton;
    private VisualElement wrapper;

    [Header("Transform Reference")]
    public Transform trackedObject;

    private byte[] file = null;
    
    //protected VisualElement documentRoot;

    private string titleInitialText;

    void Awake() {

        UIDocument document = GetComponent<UIDocument>();
        VisualElement documentRoot = document.rootVisualElement;

        title = documentRoot.Q<TextField>(name: "trello-title");
        desc  = documentRoot.Q<TextField>(name: "trello-desc");
        screenshot  = documentRoot.Q<RadioButton>(name: "trello-screenshot");
        toggleButton  = documentRoot.Q<Button>(name: "show-feedback-form");
        submitButton  = documentRoot.Q<Button>(name: "trello-submit");
        wrapper = documentRoot.Q<VisualElement>(name: "trello-wrapper");

        titleInitialText = title.text;

        wrapper.style.display = DisplayStyle.None;
        toggleButton.clicked += () => ToggleForm();
        submitButton.clicked += () => DemoSendCard();

    }

    private void ToggleForm(){
        if( wrapper.style.display == DisplayStyle.None ){
            wrapper.style.display = DisplayStyle.Flex;
        }else{
            wrapper.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Send a Trello Card using the info provided on the UI.
    /// </summary>
    public void DemoSendCard() {
        //do some validation
        if( title.text == "" || title.text == titleInitialText ){
            Debug.Log("Please fill in a title");
        }

        StartCoroutine(DemoSendCard_Internal());
    }

    private IEnumerator DemoSendCard_Internal() {

        TrelloCard card = new TrelloCard();

        card.name = title.text;
        card.due = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        card.desc = "#" + "Demo " + Application.version + "\n" +
            "___\n" +
            "###System Information\n" +
            "- " + SystemInfo.operatingSystem + "\n" +
            "- " + SystemInfo.processorType + "\n" +
            "- " + SystemInfo.systemMemorySize + " MB\n" +
            "- " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsDeviceType + ")\n" +
            "\n" +
            "___\n" +
            "###User Description\n" +
            "```\n" +
            desc.text + "\n" +
            "```\n" +
            "___\n" +
            "###Other Information\n" +
            "Playtime: " + String.Format("{0:0}:{1:00}", Mathf.Floor(Time.time / 60), Time.time % 60) + "h" + "\n" +
            "Tracked object position: " + trackedObject.position;

        if (screenshot.value) {
            StartCoroutine(UploadJPG());
            while (file == null) {
                yield return null;
            }
            card.fileSource = file;
            card.fileName = DateTime.UtcNow.ToString() + ".jpg";
        }

        //trelloSend.SendNewCard(card);
        StartCoroutine( trelloSend.SendRoutine(card) );
    }

    /// <summary>
    /// Captures the screen with UI and returns a byte array.
    /// </summary>
    /// <returns>Byte array of a jpg image</returns>
    private  IEnumerator UploadJPG() {

        // Only read the screen after all rendering is complete
        yield return new WaitForEndOfFrame();
        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        // Encode texture into JPG
        file = tex.EncodeToJPG();
        Destroy(tex);
    }
}