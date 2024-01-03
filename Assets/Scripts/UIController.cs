using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DarkRift.Server;
using System;
using TMPro;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// Custom event
/// </summary>
public class FullscreenViewOpenEvent : UnityEvent<ClientData> { }

/// <summary>
/// Controlls various UI bahaviours
/// </summary>
public class UIController : MonoBehaviour
{
    /// <summary>
    /// Prefab instance for Stream UIs
    /// </summary>
    [SerializeField]
    private GameObject streamUIPrefab = default;

    /// <summary>
    /// Container if all stream UI objects 
    /// </summary>
    private List<StreamUI> streamUIs = new List<StreamUI>();

    /// <summary>
    /// Reference to parent object of all stream UIs
    /// </summary>
    [SerializeField]
    private GridLayoutGroup gridParent = default;

    /// <summary>
    /// Reference to parent object of fullscreen stream UI view
    /// </summary>
    [SerializeField]
    private RectTransform fullscreenParent = default;

    /// <summary>
    /// Currently selected stream UI
    /// </summary>
    private StreamUI currentUI = default;

    /// <summary>
    /// Text field in fullscreen view debug section displaying network SSID
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI SSID = default;

    /// <summary>
    /// Text field in fullscreen view debug section displaying signal power
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI signalPower = default;

    /// <summary>
    /// Text field in fullscreen view debug section displaying client's FPS
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI frameCount = default;

    /// <summary>
    /// Text field in fullscreen view debug section displaying pendant battery status
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI batteryVoltStatus = default;

    /// <summary>
    /// Text field in fullscreen view debug section displaying client's MAC adress
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI networkMAC = default;

    /// <summary>
    /// UI interface for running a specific step of a current scenario 
    /// </summary>
    [SerializeField]
    private GameObject stepStartContainer = default;

    /// <summary>
    /// Parent object of debug information in fullscreen view
    /// </summary>
    [SerializeField]
    private GameObject debugContainer = default;

    /// <summary>
    /// Dropdown for selecting clients' app language (common options)
    /// </summary>
    [SerializeField]
    private TMP_Dropdown languagesDropdown = default;

    /// <summary>
    /// UI popup allowing to change options comman to all clients
    /// </summary>
    [SerializeField]
    private GameObject CommonOptionsPopup = default;

    /// <summary>
    /// Stores the scroll position of scenario selection dropdown
    /// </summary>
    public Vector2 savedScenarioScrollPosition = default;

    /// <summary>
    /// Public accessor for currently selected steam UI
    /// </summary>
    public StreamUI CurrentUI { get { return currentUI; } }


    /// <summary>
    /// Event invoked when fullscreen view is opened  
    /// </summary>
    [SerializeField]
    public FullscreenViewOpenEvent OnFullscreenViewOpened = new FullscreenViewOpenEvent();

    /// <summary>
    /// Instantiates new Stream instance
    /// </summary>
    /// <returns>Reference to controlling element of a stream object</returns>
    public StreamUI CreateStream()
    {
        return Instantiate(streamUIPrefab, gridParent.transform).GetComponent<StreamUI>();
    }

    /// <summary>
    /// Restroys specified stream UI object
    /// </summary>
    /// <param name="streamUI">target stream UI object</param>
    public void DestroyStream(StreamUI streamUI)
    {
        streamUIs.Remove(streamUI);
        Destroy(streamUI.gameObject);
    }

    /// <summary>
    /// Adjust specified stream to fullscreen display mode
    /// </summary>
    /// <param name="stream">target stream</param>
    /// <param name="video">target stream's video panel</param>
    public void EnlargeStream(StreamUI stream, RawImage video)
    {
        if (currentUI) return;
        currentUI = stream;
        fullscreenParent.gameObject.SetActive(true);
        video.GetComponent<RectTransform>().SetParent(fullscreenParent, true);
        video.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.3f);
        video.GetComponent<RectTransform>().DOSizeDelta(Vector2.zero, 0.3f);
        video.GetComponent<Canvas>().sortingOrder = 15;
        OnFullscreenViewOpened?.Invoke(stream.fullscreenClientData);
    }

    /// <summary>
    /// Contracts stream view form fullscreen to grid either because of button click, or client disconnection. 
    /// </summary>
    /// <param name="wasDisconnected">whether fullscreen is being closed due to client disconnecting</param>
    public void ContractStream(bool wasDisconnected)
    {
        if (wasDisconnected)
        {
            Destroy(fullscreenParent.GetComponentInChildren<RawImage>().gameObject);
            fullscreenParent.GetComponentsInChildren<Toggle>().ToList().ForEach(x => x.isOn = false);
            fullscreenParent.gameObject.SetActive(false);
            currentUI = null;
        }
        else
        {
            if (!currentUI) return;
            fullscreenParent.GetComponentInChildren<RawImage>().transform.SetParent(currentUI.GetComponent<RectTransform>(), true);
            fullscreenParent.gameObject.SetActive(false);
            currentUI.ContractStream();
            currentUI = null;
        }
    }

    /// <summary>
    /// Enables next module button in specified stream ui
    /// </summary>
    /// <param name="ui">target stream ui</param>
    public void EnableNextModuleButton(StreamUI ui)
    {
        ui.SetNextModuleButton(true);
    }

    /// <summary>
    /// return currently selected stream ui
    /// </summary>
    /// <returns>Reference to controlling element of a stream object</returns>
    public StreamUI GetSelectedStreamUI()
    {
        return currentUI;
    }

    /// <summary>
    /// Closes app or ends unity editor playmode
    /// </summary>
    public void ExitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Populates debug section text fields based on information received from client
    /// </summary>
    /// <param name="data">debug data as single string</param>
    /// <param name="clientUI">specified client</param>
    public void ProcessDebugData(string data, IClient clientUI)
    {
        foreach (Stream s in FindObjectOfType<StreamController>().GetStreams())
        {
            if (s.client == clientUI)
            {
                string[] parsedData = data.Split(',');
                switch (parsedData[1])
                {
                    case "20":
                        SSID.text = parsedData[2];
                        break;
                    case "21":
                        signalPower.text = parsedData[2];
                        break;
                    case "22":
                        frameCount.text = parsedData[2];
                        break;
                    case "23":
                        batteryVoltStatus.text = parsedData[2];
                        break;
                    case "24":
                        networkMAC.text = parsedData[2];
                        break;
                }
            }
            break;
        }
    }

    /// <summary>
    /// Populates languages dropdown based on information received from client
    /// </summary>
    /// <param name="data">languages data as a single string</param>
    public void ProcessLanguages(string data)
    {
        string[] parsedData = data.Split(',');
        parsedData = parsedData.Skip(1).ToArray();
        Array.Resize(ref parsedData, parsedData.Length - 1);
        languagesDropdown.ClearOptions();
        languagesDropdown.AddOptions(parsedData.ToList());
    }

    /// <summary>
    /// Toggles debug mode on or off
    /// </summary>
    /// <param name="enabled">whether debug mode should be toggled on or off</param>
    public void EnableDebugMode(bool enabled)
    {
        debugContainer.SetActive(enabled);
        stepStartContainer.SetActive(enabled);
    }

    /// <summary>
    /// Toggles common options popup state (visibility)
    /// </summary>
    public void ToggleCommonOptionsPopupState()
    {
        var currentStatus = CommonOptionsPopup.activeSelf;
        CommonOptionsPopup.SetActive(!currentStatus);
    }
}
