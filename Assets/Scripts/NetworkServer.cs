using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using System.Net.Sockets;
using Unity.VisualScripting;

public class NetworkServer : MonoBehaviour
{
    /* ~~ USER DATA ~~ */

    private string _lg = "root";
    private string _pw = "toor";

    /* ~~ END OF USER DATA ~~ */
    /* ~~ SERVER CONFIGURATION ~~ */

    int recHostID;
    int recConnectionID;

    private int _hostID;
    private readonly int _socketPort = 5491;
    private readonly int _maxConnections = 1000;

    private int _reliableChannelID;
    private int _unreliableChannelID;

    /* ~~ END OF SERVER CONFIGURATION ~~ */

    [Obsolete]
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("\nLocal IP: " + LocalIPAddress() + "\nPort:" + _socketPort);
    }

    // Update is called once per frame
    [Obsolete]
    private void Update()
    {
        while (!NetworkTransport.IsStarted)
        {
            RunServer();
        }

        UpdateNetworkConnection();

        if (Input.GetKeyDown(KeyCode.S))
        {
            SendMessageToClient("\nI am your server! Your ID: " + recHostID, recConnectionID);
        }
    }

    private void RunServer()
    {
        NetworkTransport.Init(); // init network protocol

        // declaring vars
        ConnectionConfig config;
        HostTopology topology;

        // writing config
        config = new ConnectionConfig();
        _reliableChannelID = config.AddChannel(QosType.Reliable);
        _unreliableChannelID = config.AddChannel(QosType.Unreliable);

        // writing config and connections into topology 
        topology = new HostTopology(config, _maxConnections);
        _hostID = NetworkTransport.AddHost(topology, _socketPort, null);
        Debug.Log("Server is online! : " + gameObject);
    }

    
    

    [Obsolete("Obsolete")]
    private void UpdateNetworkConnection()
    {
        
        var recBuffer = new byte[1024];
        var bufferSize = 1024;
        int dataSize;
        byte error = 0;

        var recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID,
            out _, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Client connected! Connection ID: " + recConnectionID + gameObject);
                SendMessageToClient("You are connected to the server! Your ID: " + recConnectionID, recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                var msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Client id: " + recConnectionID + "\nMessage: " + msg);  
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Client disconnected! Connection ID: " + recConnectionID + gameObject);
                break;
        }
    }



    [Obsolete]
    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        var buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(_hostID, id, _reliableChannelID, buffer, msg.Length * sizeof(char), out error);
        Debug.Log("Message sent!" + gameObject);
    }


    private static string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "0.0.0.0";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
}