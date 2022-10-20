using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;


public class NetworkedClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;
    GameObject logButton;
    GameObject registerButton;
    public GameObject AccountPart;
    public GameObject LoginPart;
    [SerializeField]
    private TMP_InputField logField;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_InputField NewRoomField;
    [SerializeField]
    private TMP_Text ErrorText;



    //Indetifiers
    static short login_i = 0;
    static short registration_i = 1;
    static short room_i = 2;

    string login;
    string password;
    // Start is called before the first frame update
    void Start()
    {
       


        Connect();
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allGameObjects)
        {
            switch (obj.name)
            {
                case "Login_b":

                   
                    logButton = obj;
                    break;
                case "Register_b":
                
                    registerButton = obj;
                    break;
                case "AccountPart":
                    
                    AccountPart = obj;
                    break;
                case "LoginPart":
                  
                    LoginPart = obj;
                    break;

                default:
                    break;
            }

        }


        logButton.GetComponent<Button>().onClick.AddListener(LogInF);
        registerButton.GetComponent<Button>().onClick.AddListener(RegisterF);

       // logField.GetComponentInChildren<Text>().
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.S))
            SendMessageToHost("Hello from client");*/

        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }

    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "192.168.0.156", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }

    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }

    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
        switch(msg)
        {
            case "LoginApproved":
                GameManager._instance.UpdateGameState(GameState.accountState);
                break;
            case "LoginDenied":
                ErrorText.gameObject.SetActive(true);
                ErrorText.text = "Wrong login or password, try again";
                break;

            default:
                break;
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }
    public void LogInF()
    {
        if(logField.text.Length != 0)
            login = logField.text;
        if (passwordField.text.Length != 0)
            password = passwordField.text;


        if(logField.text.Length != 0 && passwordField.text.Length != 0)
        {
            SendMessageToHost(login_i.ToString() + ',' + login + ',' +                              
                              password);
        }
    }
    public void RegisterF()
    {
        if (logField.text.Length != 0)
            login = logField.text;
        if (passwordField.text.Length != 0)
            password = passwordField.text;


        if (logField.text.Length != 0 && passwordField.text.Length != 0)
        {
            SendMessageToHost(registration_i.ToString() + ',' + login + ',' +
                              password);
        }
    }
    public void CreateRoom()
    {
        //here we want to send the name of the room to server.
        //Debug.Log("Create/Join Room");

        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(room_i.ToString() + ',' + NewRoomField.text);
        }
    }

}