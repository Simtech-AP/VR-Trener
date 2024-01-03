using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using DarkRift.Server;

/// <summary>
/// Contains all elements required to handle a connection with a single client
/// </summary>
public class Stream
{
    /// <summary>
    /// Darkdrift client interface
    /// </summary>
    public IClient client;

    /// <summary>
    /// this client's index
    /// </summary>
    public int index;

    /// <summary>
    /// UI object assigned to this stream
    /// </summary>
    public StreamUI streamUI;
}
