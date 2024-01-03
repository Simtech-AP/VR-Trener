using System;
using UnityEngine;

/// <summary>
/// Stores client specific variables, that are controlled from the trainer application
/// </summary>
public class ClientData
{
    /// <summary>
    /// Volume level of 
    /// </summary>
    public float lectorVolumeLevel = 1.0f;

    /// <summary>
    /// Status of all hint categories
    /// </summary>
    public bool[] hintTable = default;

    /// <summary>
    /// Initialize hit table with a number of values corresponding to amount of categories
    /// </summary>
    public ClientData()
    {
        int hintCount = Enum.GetNames(typeof(AidToggleBridge.AidesType)).Length;
        if (hintCount > 0)
        {
            hintTable = new bool[hintCount];
            for (int i = 0; i < hintCount; i++)
                hintTable[i] = false;
        }
        else
        {
            hintTable = new bool[0];
            Debug.Log("Unsafe fullscreen's hint toggles count = " + hintCount);
        }
    }
}
