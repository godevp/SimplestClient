using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

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
    public GameObject GamePart;
    public GameObject stopWatching;
    [SerializeField]
    private TMP_InputField logField;


    public GameObject chat;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_InputField NewRoomField;
    [SerializeField]
    private TMP_InputField textField;
    [SerializeField]
    private GameObject prefabText;

    [SerializeField]
    private GameObject GridForTexts;
    [SerializeField]
    private TMP_Text ErrorText;
    public Slot _slot;
    public TMP_Text loserORwinner;

    public bool turn = true; // true = first player(X), false = second player(O)
    public bool canMove = false;



    //Indetifiers
    static short login_i = 0;
    static short registration_i = 1;
    static short room_i = 2;
    static short observer_i = 8;

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
                case "GamePart":

                    GamePart = obj;
                    break;

                default:
                    break;
            }

        }


        logButton.GetComponent<Button>().onClick.AddListener(LogInF);
        registerButton.GetComponent<Button>().onClick.AddListener(RegisterF);
    }

    // Update is called once per frame
    void Update()
    {
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
            //192.168.0.156 home 10.0.254.6
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
        string[] splitter = msg.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
        switch (splitter[0])
        {
            case "LoginApproved":
                GameManager._instance.UpdateGameState(GameState.accountState);
                break;
            case "LoginDenied":
                ErrorText.gameObject.SetActive(true);
                ErrorText.text = "Something wrong or somebody using the account";
                break;
            case "FirstPlayer":
                GameManager._instance.UpdateGameState(GameState.gameState);
                ActivateSomeStuff();
                break;

            case "SecondPlayer":
                GameManager._instance.UpdateGameState(GameState.gameState);
                ActivateSomeStuff();

                break;

            case "5":
                _slot.EmptyButtons();
                GameManager._instance.UpdateGameState(GameState.gameState);
                canMove = false;
                turn = false;
                loserORwinner.gameObject.SetActive(false);
                textField.gameObject.SetActive(false);
                GridForTexts.gameObject.SetActive(false);
                stopWatching.gameObject.SetActive(true);
                chat.gameObject.SetActive(false);

                break;
            case "6":
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Xsprite;

                break;
            case "7":
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Osprite;


                break;
            case "111":
                canMove = true;
                turn = true;//means we Xplayer
                loserORwinner.text = "Your Move";

                break;

            case "222":
                canMove = false;
                turn = false;//means we Oplayer

                break;


            case "999":
                //set the sprite to X in the button[splitter[1]]
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Xsprite;
                
                if (turn)
                {
                    canMove = false;
                    loserORwinner.text = "";
                }
                else
                {
                    loserORwinner.text = "Your Move";
                    canMove = true;
                }

                break;
            case "888":
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Osprite;
                if (!turn)
                {
                    canMove = false;
                    loserORwinner.text = "";
                }
                else
                {
                    loserORwinner.text = "Your Move";
                    canMove = true;
                }
                break;

            case "7777":  //win 
                canMove = false;
                loserORwinner.text = "Won";
                break;

            case "6666"://lose
                canMove = false;
                loserORwinner.text = "Lost";
                break;

            case "3333"://tie
                canMove = false;
                loserORwinner.text = "NO WINNER";
                break;

            case "123":
                _slot.EmptyButtons();
                canMove = false;
                loserORwinner.text = "";
                break;

            case "555":

                var newText = Instantiate(prefabText, GridForTexts.transform);
                newText.GetComponent<TMP_Text>().text = splitter[1];
                
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
        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(room_i.ToString() + ',' + NewRoomField.text);
        }
        
    }

    public void ConnectAsObserver()
    {
        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(observer_i.ToString() + ',' + NewRoomField.text);
        }
    }
    public void SendMessageToChat()
    {
        if(textField.text.Length != 0)
        {
            SendMessageToHost(555.ToString() + ',' + textField.text);
            var newText = Instantiate(prefabText, GridForTexts.transform);
            if(turn)
            newText.GetComponent<TMP_Text>().text = textField.text;
            else
            newText.GetComponent<TMP_Text>().text = textField.text;
            textField.text = "";
        }
    }

   void ActivateSomeStuff()
    {
        loserORwinner.gameObject.SetActive(true);
        textField.gameObject.SetActive(true);
        GridForTexts.gameObject.SetActive(true);
        stopWatching.gameObject.SetActive(false);
        chat.gameObject.SetActive(true);

    }
    public void StopWatching()
    {
        GameManager._instance.UpdateGameState(GameState.accountState);
        SendMessageToHost("7");
    }


    private void OnApplicationQuit()
    {
        SendMessageToHost(69.ToString());
        SendMessageToHost(321.ToString() + ',' + login);
    }

}