using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to connect a UI volume slider to actual functionality
/// </summary>
public class LectorVolumeBridge : MonoBehaviour
{

    /// <summary>
    /// Reference to UI controller, allows updating slider position
    /// </summary>
    [SerializeField]
    private UIController uIController = default;

    /// <summary>
    /// Passes a callback to the event called when slider value changes
    /// </summary>
    private void Awake()
    {
        uIController.OnFullscreenViewOpened.AddListener(SetLectorVolume);
    }

    /// <summary>
    /// Initiates procedure to send volume level change message 
    /// </summary>
    /// <param name="volume">New volume value</param>
    public void ChangeLectorVolume(float volume)
    {
        uIController.CurrentUI.SetLectorVolume(volume);
    }

    /// <summary>
    /// Adjust the slider value to client value
    /// </summary>
    /// <param name="client">Client to read volume level from</param>
    public void SetLectorVolume(ClientData client)
    {
        GetComponent<Slider>().value = client.lectorVolumeLevel;
    }

    /// <summary>
    /// Unsubscribes the callback from ui controller
    /// </summary>
    private void OnDestroy()
    {
        uIController.OnFullscreenViewOpened.RemoveListener(SetLectorVolume);
    }
}