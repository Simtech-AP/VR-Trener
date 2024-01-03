using Adrenak.UniVoice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Tool allowing clients to talk with the trainer via voice chat using Adrenak 
/// </summary>
public class VoiceController : MonoBehaviour
{

    /// <summary>
    /// Networking client
    /// </summary>
    private UdpClient client;

    /// <summary>
    /// Audiosource reference used to play the sound
    /// </summary>
    [SerializeField]
    private AudioSource voiceSource = default;

    /// <summary>
    /// Adrenak voice utility
    /// </summary>
    private Voice voice;

    /// <summary>
    /// Creates instance of chat room client for trainer app 
    /// </summary>
    [ContextMenu("Create")]
    public void ConnectVoice()
    {
        voice = Voice.New(voiceSource);
        voice.Join("Simtech", status => Debug.Log("Room join status: " + status));
    }

    /// <summary>
    /// Rises a flag alowing for communication to begin
    /// </summary>
    [ContextMenu("Start")]
    public void StartSending()
    {
        voice.Speaking = true;
    }

    /// <summary>
    /// Lowers a flag ending communication 
    /// </summary>
    [ContextMenu("Stop")]
    public void StopSending()
    {
        voice.Speaking = false;
    }
}
