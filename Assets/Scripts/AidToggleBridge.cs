using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to connect a UI button to actual functionality
/// </summary>
public class AidToggleBridge : MonoBehaviour
{

    /// <summary>
    /// Hints categories in the project
    /// </summary>
    public enum AidesType
    {
        Cabinet = 0,
        Casette = 1,
        Console = 2,
        Pendant = 3
    }

    /// <summary>
    /// Reference to UI controller, allows updating checkboxes status
    /// </summary>
    [SerializeField]
    private UIController uIController = default;

    /// <summary>
    /// Defines which category of hints is controlled by this instance 
    /// </summary>
    [SerializeField]
    private AidesType aidType = default;

    /// <summary>
    /// Passes a callback to the event called on UI expansion when object is created
    /// </summary>
    private void Awake()
    {
        uIController.OnFullscreenViewOpened.AddListener(SetStartingHintState);
    }

    /// <summary>
    /// Assigned to UI checkbox item
    /// </summary>
    public void ToggleHintStatus(bool targetStatus)
    {
        uIController.CurrentUI.ToggleHint(targetStatus, (int)aidType);
    }

    /// <summary>
    /// Updates hint status after client selection to reflest actual hints status within client
    /// </summary>
    public void SetStartingHintState(ClientData client)
    {
        GetComponent<Toggle>().isOn = client.hintTable[(int)aidType];
    }

    /// <summary>
    /// Removes the callback from the event called on UI expansion, when item is destroyed
    /// </summary>
    private void OnDestroy()
    {
        uIController.OnFullscreenViewOpened.RemoveListener(SetStartingHintState);
    }
}
