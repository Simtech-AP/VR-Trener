using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlls various UI elements of client's stream
/// </summary>
public class StreamUI : MonoBehaviour
{
    /// <summary>
    /// UI object displaying video feed from the user
    /// </summary>
    [SerializeField]
    private RawImage texture = default;

    /// <summary>
    /// Default anchors of rect transform 
    /// </summary>
    private Vector2 defaultAnchors = default;

    /// <summary>
    /// default size of rect transform
    /// </summary>
    private Vector2 defaultSize = default;

    /// <summary>
    /// Text field displaying current module id
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI moduleText = default;

    /// <summary>
    /// Text field displaying current scenario id
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI scenarioText = default;

    /// <summary>
    /// Text field displaying current step id
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI stepText = default;

    /// <summary>
    /// Button object advancing client to the next step
    /// </summary>
    [SerializeField]
    private GameObject nextModuleButton = default;

    /// <summary>
    /// UI objects signifying that the client's asking for trainer's attention
    /// </summary>
    [SerializeField]
    private GameObject attentionFrame = default;

    /// <summary>
    /// Input field alowing to force client to attempt connection with specific pendant 
    /// </summary>
    [SerializeField]
    private TMP_InputField pilotIDInput = default;

    /// <summary>
    /// Reference to connection controller, reqired to send some messages
    /// </summary>
    private ConnectionController connectionController = default;

    /// <summary>
    /// Client's data to be displayed in the fullscreen view
    /// </summary>
    public ClientData fullscreenClientData = new ClientData();


    /// <summary>
    /// Initializes runtime dependant fields
    /// </summary>
    [ContextMenu("GetSize")]
    void Start()
    {
        defaultAnchors = texture.GetComponent<RectTransform>().anchoredPosition;
        defaultSize = texture.GetComponent<RectTransform>().sizeDelta;
        connectionController = FindObjectOfType<ConnectionController>();
    }

    /// <summary>
    /// Causes the stream to enter fullscreen mode
    /// </summary>
    public void Enlarge()
    {
        FindObjectOfType<UIController>().EnlargeStream(this, texture);
        connectionController.SetWatchingIndicator(true);
        if (attentionFrame.activeInHierarchy)
        {
            attentionFrame.SetActive(false);
        }
    }

    /// <summary>
    /// Causes the stream to exit fullscreen mode
    /// </summary>
    public void ContractStream()
    {
        texture.GetComponent<RectTransform>().DOAnchorPos3D(defaultAnchors, 0.3f);
        texture.GetComponent<RectTransform>().DOSizeDelta(defaultSize, 0.3f);
        texture.GetComponent<Canvas>().sortingOrder = 5;
        connectionController.SetWatchingIndicator(false);
    }

    /// <summary>
    /// Assigns module signature text
    /// </summary>
    /// <param name="module">text to assign</param>
    public void SetModuleText(string module)
    {
        moduleText.text = module;
    }

    /// <summary>
    /// Assigns scenario signature text
    /// </summary>
    /// <param name="scenario">text to assign</param>
    public void SetScenarioText(string scenario)
    {
        scenarioText.text = scenario;
    }

    /// <summary>
    /// Assigns step signature text
    /// </summary>
    /// <param name="step">text to assign</param>
    public void SetStepText(string step)
    {
        stepText.text = step;
    }

    /// <summary>
    /// Shows or hides the next step button
    /// </summary>
    /// <param name="isOn">whether the button should be shown or hidden</param>
    public void SetNextModuleButton(bool isOn)
    {
        nextModuleButton.SetActive(isOn);
    }

    /// <summary>
    /// Starts the next module
    /// </summary>
    public void StartNextModule()
    {
        FindObjectOfType<StreamController>().StartNextModule(this);
    }

    /// <summary>
    /// Enables attention frame
    /// </summary>
    public void EnableAttentionFrame()
    {
        attentionFrame.SetActive(true);
    }

    /// <summary>
    /// Sets custom pendant id, that the cliant has to use
    /// </summary>
    public void SetPilotID()
    {
        connectionController.SetPilotIDForUser(FindObjectOfType<StreamController>().GetStreamByStreamUI(this).client, pilotIDInput.text);
    }

    /// <summary>
    /// Changes state of specified group of hints
    /// </summary>
    /// <param name="status">status to be set</param>
    /// <param name="aidTypeId">affected hint</param>
    public void ToggleHint(bool status, int aidTypeId)
    {
        connectionController.SetHintStateForUser(FindObjectOfType<StreamController>().GetStreamByStreamUI(this).client, aidTypeId, status);
        this.fullscreenClientData.hintTable[aidTypeId] = status;
    }

    /// <summary>
    /// Sets client's lector volume
    /// </summary>
    /// <param name="value">new volume value</param>
    public void SetLectorVolume(float volume)
    {
        connectionController.SetLectorVolume(FindObjectOfType<StreamController>().GetStreamByStreamUI(this).client, volume);
        this.fullscreenClientData.lectorVolumeLevel = Mathf.Clamp(volume, 0.0f, 1.0f);
    }
}
