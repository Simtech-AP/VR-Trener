using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;
using TMPro;
using System.Linq;

/// <summary>
/// Responsible for storing clients and casting messages to correct clients, and handling messages received from clients
/// </summary>
public class ConnectionController : MonoBehaviour
{
    /// <summary>
    /// Reference to Darkdrift server
    /// </summary>
    XmlUnityServer server;

    /// <summary>
    /// List of connected clients
    /// </summary>
    List<IClient> clients = new List<IClient>();

    /// <summary>
    /// Reference to streamController - another layer of abstraction between ui and messaging logic
    /// </summary>
    [SerializeField]
    StreamController streamController = default;

    /// <summary>
    /// Is the scenario list parsed?
    /// </summary>
    private bool scenariosFilled = false;

    void Start()
    {
        server = GetComponent<XmlUnityServer>();
        SetDarkdriftCallbacks();
    }

    /// <summary>
    /// Subscribes relevent methods to darkdrift events
    /// </summary>
    private void SetDarkdriftCallbacks()
    {
        server.Server.ClientManager.ClientConnected += OnClientConnected;
        server.Server.ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    /// <summary>
    /// Darkdrift callback, invoked when new client connects
    /// </summary>
    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        streamController.CreateStream(e.Client);
        clients.Add(e.Client);
        e.Client.MessageReceived += ProcessMessage;
    }


    /// <summary>
    /// Darkdrift callback, invoked when new client disconnects
    /// </summary>
    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        if (FindObjectOfType<UIController>().GetSelectedStreamUI() != null)
        {
            foreach (Stream stream in streamController.GetStreams())
            {
                if (stream.client == e.Client)
                {
                    FindObjectOfType<UIController>().ContractStream(true);
                }
            }
        }
        streamController.DestroyStream(e.Client);
        clients.Remove(e.Client);
        e.Client.MessageReceived -= ProcessMessage;
    }


    /// <summary>
    /// Darkdrift callback, invoked when a mesage is received from a client
    /// Handles various messages based on its header
    /// </summary>
    private void ProcessMessage(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                string data = reader.ReadString();
                switch (data)
                {
                    case var s when data.StartsWith("finished"):
                        streamController.EnableNextModule(e.Client);
                        break;
                    case var s when data.StartsWith("request"):
                        RequestAssistance(e.Client);
                        break;
                    case var s when data.StartsWith("scenarios"):
                        if (!scenariosFilled)
                        {
                            FillScenariosDropdown(data.Replace("scenarios,", "").Trim().Split(','));
                            scenariosFilled = true;
                        }
                        break;
                    case var s when data.StartsWith("debug"):
                        FindObjectOfType<UIController>().ProcessDebugData(data, e.Client);
                        break;
                    case var s when data.StartsWith("languages"):
                        FindObjectOfType<UIController>().ProcessLanguages(data);
                        break;
                    default:
                        streamController.SetStreamData(data, e.Client);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Sends message to the specified client, prompting to run the next module
    /// </summary>
    /// <param name="client">specified client</param>
    public void NextModule(IClient client)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("next");
        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Unreliable);
    }

    /// <summary>
    /// Sends message to the specified client, prompting to run module specified by id
    /// </summary>
    /// <param name="module">Id of the module to be run</param>
    /// <param name="client">specified client</param>
    public void RunModule(int module, IClient client)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("module" + module);
        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    /// <summary>
    /// Sends message to the specified client, prompting to run scenario specified by id
    /// </summary>
    /// <param name="scenario">Id of the scenario to be run</param>
    /// <param name="client">specified client</param>
    public void RunScenario(int scenario, IClient client)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("scenario" + scenario);
        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    /// <summary>
    /// Sends message to the specified client, prompting to run scenario specified by name
    /// </summary>
    /// <param name="scenario">Id of the scenario to be run</param>
    /// <param name="client">specified client</param>
    public void RunScenario(string scenario, IClient client)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("runScenario" + scenario);
        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    /// <summary>
    /// Sends message to the specified client, prompting to run a step specified by id within current scenario
    /// </summary>
    /// <param name="step">Id of the step to be run</param>
    /// <param name="client">specified client</param>
    public void RunStep(int step, IClient client)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("step" + step);
        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    /// <summary>
    /// Executed on receiving message from the client requesting assistance
    /// </summary>
    /// <param name="client">specified client</param>
    private void RequestAssistance(IClient client)
    {
        streamController.RequestAssistance(client);
    }

    /// <summary>
    /// Broadcasts a run module message to all connected clients
    /// </summary>
    /// <param name="input">Input field from which the module id is to be parsed</param>
    public void RunModuleForAll(TMP_InputField input)
    {
        var moduleNumber = int.Parse(input.text);
        foreach (Stream stream in streamController.GetStreams())
        {
            RunModule(moduleNumber, stream.client);
        }
    }

    /// <summary>
    /// Broadcasts a run scenario message to all connected clients based on text input
    /// </summary>
    /// <param name="input">Input field from which the scenario id is to be parsed</param>
    public void RunScenarioForAll(TMP_InputField input)
    {
        var scenarioNumber = int.Parse(input.text);
        foreach (Stream stream in streamController.GetStreams())
        {
            RunScenario(scenarioNumber, stream.client);
        }
    }

    /// <summary>
    /// Broadcasts a run module message to all connected clients based on selected dropdown list option 
    /// </summary>
    /// <param name="input">Dropdown menu which contains selected module to run</param>
    public void RunScenarioForAll(TMP_Dropdown input)
    {
        var scenario = input.options[input.value].text;
        string scenarioId = scenario.Split(' ')[0].Trim(':');
        foreach (Stream stream in streamController.GetStreams())
        {
            RunScenario(scenarioId, stream.client);
        }
    }

    /// <summary>
    /// Broadcasts a run step message to all connected clients based on text input
    /// </summary>
    /// <param name="input">Input field from which the step id is to be parsed</param>
    public void RunStepForAll(TMP_InputField input)
    {
        var stepNumber = int.Parse(input.text);
        foreach (Stream stream in streamController.GetStreams())
        {
            RunStep(stepNumber, stream.client);
        }
    }

    /// <summary>
    /// Populate UI dropdown list of scenarios
    /// </summary>
    /// <param name="scenarios">List of scenarios received from client</param>
    private void FillScenariosDropdown(string[] scenarios)
    {
        var dropdowns = Resources.FindObjectsOfTypeAll<TMP_Dropdown>();
        foreach (TMP_Dropdown dropdown in dropdowns)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(scenarios.ToList());
        }
    }

    /// <summary>
    /// Sends a run module message to currently active client 
    /// </summary>
    /// <param name="input">Input field from which the module id is to be parsed</param>
    public void RunModuleForCurrent(TMP_InputField input)
    {
        var moduleNumber = int.Parse(input.text);
        foreach (Stream stream in streamController.GetStreams())
        {
            if (stream.streamUI == FindObjectOfType<UIController>().GetSelectedStreamUI())
            {
                RunModule(moduleNumber, stream.client);
                return;
            }
        }
    }

    /// <summary>
    /// Sends a run step message to currently active client 
    /// </summary>
    /// <param name="input">Input field from which the step id is to be parsed</param>
    public void RunStepForCurrent(TMP_InputField input)
    {
        var stepNumber = int.Parse(input.text);
        foreach (Stream stream in streamController.GetStreams())
        {
            if (stream.streamUI == FindObjectOfType<UIController>().GetSelectedStreamUI())
            {
                RunStep(stepNumber, stream.client);
                return;
            }
        }
    }

    /// <summary>
    /// Sends a run scenario message to currently active client 
    /// </summary>
    /// <param name="input">Dropdown menu which contains selected module to run</param>
    public void RunScenarioForCurrent(TMP_Dropdown input)
    {
        var scenario = input.options[input.value].text;
        foreach (Stream stream in streamController.GetStreams())
        {
            if (stream.streamUI == FindObjectOfType<UIController>().GetSelectedStreamUI())
            {
                RunScenario(scenario, stream.client);
                return;
            }
        }
    }

    /// <summary>
    /// Sends a customized message to selected client 
    /// </summary>
    /// <param name="input">Input field with message contents</param>
    public void SendMessage(TMP_InputField input)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("message\n" + input.text);
        foreach (Stream stream in streamController.GetStreams())
        {
            if (stream.streamUI == FindObjectOfType<UIController>().GetSelectedStreamUI())
            {
                using (Message message = Message.Create(0, writer))
                    stream.client.SendMessage(message, SendMode.Unreliable);
                return;
            }
        }
    }

    /// <summary>
    /// Shows or hides an indicator signifying the trainer is watching client feed
    /// </summary>
    /// <param name="isOn">Whether the indicator should be toggled on or off</param>
    public void SetWatchingIndicator(bool isOn)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        if (isOn)
            writer.Write("watch");
        else
            writer.Write("stop");
        foreach (Stream stream in streamController.GetStreams())
        {
            if (stream.streamUI == FindObjectOfType<UIController>().GetSelectedStreamUI())
            {
                using (Message message = Message.Create(0, writer))
                    stream.client.SendMessage(message, SendMode.Unreliable);
                return;
            }
        }
    }

    /// <summary>
    /// Sends message to selected client requesting resseting the pendant connection.
    /// </summary>
    public void ResetPilotConnection()
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("resetpilot\n");
        foreach (Stream stream in streamController.GetStreams())
        {
            if (stream.streamUI == FindObjectOfType<UIController>().GetSelectedStreamUI())
            {
                using (Message message = Message.Create(0, writer))
                    stream.client.SendMessage(message, SendMode.Unreliable);
                return;
            }
        }
    }

    /// <summary>
    /// Sends message to selected client requesting application language change
    /// </summary>
    /// <param name="languagesDropdown">Dropdon list, with selected language option</param>
    public void SetLanguage(TMP_Dropdown languagesDropdown)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("language:" + languagesDropdown.options[languagesDropdown.value].text);
        foreach (Stream stream in streamController.GetStreams())
        {
            using (Message message = Message.Create(0, writer))
                stream.client.SendMessage(message, SendMode.Reliable);
            return;
        }
    }

    /// <summary>
    /// Sends message to selected client requesting adjusting tracker's battery level at which it should start charging
    /// </summary>
    /// <param name="batteryLevel">Input field with value to be parsed</param>
    public void SetTrackerLimitBatteryLevel(TMP_InputField batteryLevel)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        try
        {
            float.TryParse(batteryLevel.text, out float parsedLevel);
            parsedLevel = Mathf.Clamp(parsedLevel, 0, 100);
            // parsedLevel /= 100f;
            writer.Write("tracker:" + parsedLevel.ToString());
            foreach (Stream stream in streamController.GetStreams())
            {
                using (Message message = Message.Create(0, writer))
                    stream.client.SendMessage(message, SendMode.Reliable);
                return;
            }
        }
        catch { }

    }

    /// <summary>
    /// Sends message to selected client forcing him to connect with specified pendant
    /// </summary>
    /// <param name="client">Target client</param>
    /// <param name="pilotID">Id of the pendant</param>
    public void SetPilotIDForUser(IClient client, string pilotID)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("pilotID:" + pilotID);
        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    /// <summary>
    /// Sends message to selected client requesting him to show or hide all hints of specified type
    /// </summary>
    /// <param name="client">Target client</param>
    /// <param name="hintType">Type of hints to perform this operation on</param>
    /// <param name="targetState">Whether hints should be toggled on or off</param>
    public void SetHintStateForUser(IClient client, int hintType, bool targetState)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("toggleHints:" + hintType + ":" + targetState);

        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    /// <summary>
    /// Sends message to selected client requesting him to adjust lector volume accordingly
    /// </summary>
    /// <param name="client">Target client</param>
    /// <param name="_volume">New lector volume to be set</param>
    public void SetLectorVolume(IClient client, float _volume)
    {
        DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write("lectorVolume:" + _volume);

        using (Message message = Message.Create(0, writer))
            client.SendMessage(message, SendMode.Reliable);
    }

    //DEBUG
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    RunModule(2, clients[0]);
        //}
    }
}
