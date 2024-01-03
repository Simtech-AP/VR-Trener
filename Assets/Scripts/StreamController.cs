using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using System;

/// <summary>
/// Stores and handles all client streams connected to the app 
/// </summary>
public class StreamController : MonoBehaviour
{
    /// <summary>
    /// Contains all streams
    /// </summary>
    private List<Stream> streams = new List<Stream>();

    /// <summary>
    /// Reference to ui controller
    /// </summary>
    [SerializeField]
    private UIController uiController = default;

    /// <summary>
    /// Returns a collection of all streams registered in the app
    /// </summary>
    public List<Stream> GetStreams()
    {
        return streams;
    }

    /// <summary>
    /// Creates a new stream for a given client 
    /// </summary>
    /// <param name="client">Newly registered cleint</param>
    public void CreateStream(IClient client)
    {
        Stream newStream = new Stream();
        newStream.client = client;
        newStream.index = client.ID;
        newStream.streamUI = uiController.CreateStream();
        streams.Add(newStream);
    }

    /// <summary>
    /// Destroys given client's stream 
    /// </summary>
    /// <param name="client">Newly registered client</param>
    public void DestroyStream(IClient client)
    {
        Stream toDestroy = streams.Find(x => x.client == client);

        try
        {
            uiController.DestroyStream(toDestroy.streamUI);
            streams.Remove(toDestroy);
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Stream not found for this client. Make sure it was created. Error: " + e.Message);
        }
    }

    /// <summary>
    /// Updates ui text for mudule scenario or step id 
    /// </summary>
    /// <param name="data">new information to be displayed</param>
    /// <param name="client">Target client</param>
    public void SetStreamData(string data, IClient client)
    {
        Stream s = streams.Find(x => x.client == client);

        try
        {
            if (data.Contains("module"))
                s.streamUI.SetModuleText(data.Replace("module", ""));
            else if (data.Contains("scenario"))
                s.streamUI.SetScenarioText(data.Replace("scenario", ""));
            else if (data.Contains("step"))
                s.streamUI.SetStepText(data.Replace("step", ""));
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Stream not found for this client. Make sure it was created. Error: " + e.Message);
        }


    }

    /// <summary>
    /// Allows client to enable continuation to the next module 
    /// </summary>
    /// <param name="client">Target client</param>
    public void EnableNextModule(IClient client)
    {
        Stream s = streams.Find(x => x.client == client);

        try
        {
            uiController.EnableNextModuleButton(s.streamUI);
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Stream not found for this client. Make sure it was created. Error: " + e.Message);
        }
    }

    /// <summary>
    /// Force starts the next module for the specified client
    /// </summary>
    /// <param name="ui">Target client streamUI</param>
    public void StartNextModule(StreamUI ui)
    {
        Stream s = streams.Find(x => x.streamUI == ui);

        try
        {
            FindObjectOfType<ConnectionController>().NextModule(s.client);
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Stream not found. Make sure it was created. Error: " + e.Message);
        }
    }

    /// <summary>
    /// Enables attention frame after the client requested assistance
    /// </summary>
    /// <param name="client">Source client of teh request</param>
    public void RequestAssistance(IClient client)
    {
        foreach (Stream s in streams)
        {
            if (s.client == client)
            {
                s.streamUI.EnableAttentionFrame();
            }
        }
    }

    /// <summary>
    /// Returns stream identified by ID
    /// </summary>
    /// <param name="streamUI">Target client streamUI</param>

    public Stream GetStreamByStreamUI(StreamUI streamUI)
    {
        return streams.Find(x => x.streamUI == streamUI);
    }
}
