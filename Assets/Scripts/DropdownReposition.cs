using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Utility class allowing to save position in dropdown list upon closing it
/// and restore that position when reopening
/// </summary>
public class DropdownReposition : MonoBehaviour
{
    /// <summary>
    /// Scroll rect of the dropdown list
    /// </summary>
    [SerializeField] private ScrollRect scrollRect;

    /// <summary>
    /// Reference to ui controller, when the information about saved position is stored
    /// </summary>
    [SerializeField] private UIController uIController;

    /// <summary>
    /// Initializes reposition procedure
    /// </summary>
    private void OnEnable()
    {
        if (isTemplateUiObject()) return;

        StartCoroutine(DelayReposition());
    }

    /// <summary>
    /// Assigns saved position to the list's scroll rect
    /// </summary>
    private IEnumerator DelayReposition()
    {
        // Prevents reposition from happening before the list is populated
        yield return new WaitForEndOfFrame();

        scrollRect.normalizedPosition = uIController.savedScenarioScrollPosition;

    }

    /// <summary>
    /// Stores current scroll position in ui controller
    /// </summary>
    private void OnDisable()
    {
        if (isTemplateUiObject()) return;

        uIController.savedScenarioScrollPosition = scrollRect.normalizedPosition;
    }

    /// <summary>
    /// Checks whether current object is templete ui object or actually a functional instance
    /// </summary>
    private bool isTemplateUiObject()
    {
        return gameObject.name == "Template";
    }
}
